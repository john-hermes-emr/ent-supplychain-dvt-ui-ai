using Azure.Identity;
using DVT.Api.Contracts;
using DVT.Api.CustomExceptionMiddleware;
using DVT.Api.Extensions;
using DVT.Api.Models;
using DVT.Core;
using DVT.Core.Models;
using DVT.Core.Services;
using DVT.Core.Validators;
using DVT.Data;
using FluentValidation;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;
using Okta.AspNetCore;
using System.Threading.Tasks;

namespace DVT.Api
{
    public class Program
    {
        private static StartUpLog _startupLog = new StartUpLog();

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            _startupLog.EnvironmentName = builder.Environment.EnvironmentName;
            _startupLog.Add($"Starting application at {DateTime.UtcNow} UTC");

            SetupConfigurationSources(builder);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = OktaDefaults.ApiAuthenticationScheme;
                options.DefaultChallengeScheme = OktaDefaults.ApiAuthenticationScheme;
                options.DefaultSignInScheme = OktaDefaults.ApiAuthenticationScheme;
            })
            .AddOktaWebApi(new OktaWebApiOptions()
            {
                OktaDomain = builder.Configuration.GetValue<string>("Okta:OktaDomain")
            });

            SetupFileShareServices(builder);

            ConfigureDbContext(builder);

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            RegisterServices(builder);

            AddSwagger(builder);

            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                   policy.WithOrigins(
                           "https://localhost:8000",
                           "http://127.0.0.1:3000",
                           "https://dvt-dev.emerson.com/",
                           "https://dvt-stage.emerson.com/",
                           "https://dvt.emerson.com/")
                   .AllowAnyHeader()
                   .AllowCredentials());
            });

            builder.Services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.All;
                logging.RequestHeaders.Add("sec-ch-ua");
                logging.ResponseHeaders.Add("my-response-header");
                logging.MediaTypeOptions.AddText("application/javascript");
                logging.RequestBodyLogLimit = 4096;
                logging.ResponseBodyLogLimit = 4096;
            });

            //Finish setting up the about service and pass the log so we can return it if needed            
            _startupLog.SetStatusSuccess("Application started successfully.");
            builder.Services.AddSingleton<IAboutService>(sp =>
            {
                return new AboutService(_startupLog);
            });

            var app = builder.Build();

            if (app.Environment.EnvironmentName == Environments.Local)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpLogging();

            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHttpsRedirection();

            app.MapControllers();

            app.ConfigureCustomExceptionMiddleware();

            app.Run();
        }

        private static void RegisterServices(WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IValidator<MasterData>, MasterDataValidator>();
            builder.Services.AddTransient<IValidator<UserInfo>, UserInfoValidator>();
            builder.Services.AddTransient<IValidator<ActivityLog>, ActivityLogValidator>();
            builder.Services.AddTransient<IValidator<Job>, JobValidator>();
            builder.Services.AddSignalR();
            builder.Services.AddScoped<IRealtimeStatusReportService, RealtimeStatusReportService>();
            builder.Services.AddTransient<RealtimeStatusReportService>();
            builder.Services.AddTransient<IMasterDataService, MasterDataService>();
            builder.Services.AddTransient<IUserInfoService, UserInfoService>();
            builder.Services.AddTransient<ILogFileService, LogFileService>();
            builder.Services.AddTransient<IOutputFileService, OutputFileService>();
            builder.Services.AddTransient<IStorageService, StorageService>();
            builder.Services.AddTransient<IActivityLogService, ActivityLogService>();
            builder.Services.AddTransient<IFileLoadService, FileLoadService>();
            builder.Services.AddTransient<IJobFileService, JobFileService>();
            builder.Services.AddTransient<IJobService, JobService>();
            builder.Services.AddTransient<IDbHealthCheckService, DbHealthCheckService>();
            builder.Services.AddTransient<IValidationService, ValidationService>();
            builder.Services.AddTransient<IConfigSettingService>(sp =>
            {
                var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
                var environmentName = builder.Environment.EnvironmentName;
                return new ConfigSettingService(unitOfWork, environmentName);
            });
        }

        private static void SetupFileShareServices(WebApplicationBuilder builder)
        {
            try
            {
                var storageAccountConnectionString = builder.Configuration["StorageAccountConnectionString"];
                var userShareName = builder.Configuration["UserFileShareName"];
                var mainShareName = builder.Configuration["MainFileShareName"];

                _startupLog.Add("MainFileShareName: " + mainShareName);
                _startupLog.Add("UserFileShareName: " + userShareName);

                //If any of these are not configured, log the error and set the startup failed flag to true
                if (string.IsNullOrEmpty(storageAccountConnectionString) || string.IsNullOrEmpty(userShareName) || string.IsNullOrEmpty(mainShareName))
                {
                    _startupLog.Add("StorageAccountConnectionString, UserFileShareName or MainFileShareName is not configured.");
                    _startupLog.SetStatusFailed();
                    return;
                }

                builder.Services.AddSingleton<IShareClientMainShare>(sp =>
                {
                    return new ShareClientMainShareWrapper(storageAccountConnectionString, mainShareName);
                });

                builder.Services.AddSingleton<IShareClientUserShare>(sp =>
                {
                    return new ShareClientUserShareWrapper(storageAccountConnectionString, userShareName);
                });
            }
            catch (Exception ex)
            {
                _startupLog.Add($"Error setting up file share services: {ex.Message}");
                _startupLog.SetStatusFailed();
            }

            _startupLog.Add("File Share services successfully set up.");
        }

        private static void ConfigureDbContext(WebApplicationBuilder builder)
        {
            //Because dev and stage share the same environment in Azure, we need to differentiate them here
            //The connection string for dev doesn't have a tag next to it. Similarly, when this is deployed to 
            //production, the connection string won't have a tag next to it either.

            var connectionStringKey = builder.Environment.EnvironmentName == "Stage" ? "PostgreSQLConnectionString-Stage" : "PostgreSQLConnectionString";

            var dbConnectionString = builder.Configuration[connectionStringKey]?.ToString();

            if (dbConnectionString == null)
            {
                _startupLog.Add("PostgreSQLConnectionString is not configured.");
                return;
            }

            _startupLog.Add("PostgreSQLConnectionString: " + dbConnectionString?.Substring(0, 20));

            //Set up the DB Context
            builder.Services.AddDbContext<DVTContext>(options => options.UseNpgsql(dbConnectionString));

            _startupLog.Add("Database successfully set up.");
        }

        private static void SetupConfigurationSources(WebApplicationBuilder builder)
        {
            try
            {
                //If the environment is Local, connect to the Key Vault using the developer's credentials
                if (builder.Environment.EnvironmentName == Environments.Local)
                {
                    _startupLog.Add("Using developer credentials to access Key Vault.");
                }
                else //In other environments, we're going to get the settings from the keyvault via environment variables
                {
                    _startupLog.Add("Using Environment Variables to get settings.");
                    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
                }

                //Configure azure app configuration
                var azureAppConfigEndpoint = builder.Configuration["AppConfigUrl"];

                // Prioritize Visual Studio credential for local development
                var credential = new ChainedTokenCredential(
                    new VisualStudioCredential(),
                    new AzureCliCredential(),
                    new DefaultAzureCredential());

                var azureAppConfig = builder.Configuration.AddAzureAppConfiguration(options =>
                {
                    options.Connect(new Uri(azureAppConfigEndpoint), credential)
                    .Select(KeyFilter.Any, null)
                    .ConfigureRefresh(refreshOptions =>
                    {
                        // Trigger full configuration refresh when any selected key changes.
                        //refreshOptions.RegisterAll();
                        refreshOptions.Register("*", refreshAll: true);
                        refreshOptions.SetRefreshInterval(TimeSpan.FromSeconds(5));
                    })
                    .UseFeatureFlags(featureFlagOptions =>
                    {
                        featureFlagOptions.Select("*");
                        featureFlagOptions.SetRefreshInterval(TimeSpan.FromSeconds(5));
                    })
                    .ConfigureKeyVault(kv =>
                    {
                        kv.SetCredential(credential);
                    });
                });

                builder.Services.AddAzureAppConfiguration();
            }
            catch (Exception ex)
            {
                _startupLog.Add($"Error setting up configuration sources: {ex.Message}");
                _startupLog.SetStatusFailed();
            }

            _startupLog.Add("Configuration sources successfully set up.");
        }

        private static void AddSwagger(WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo { Title = "DVT API", Version = "v1" });

                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                });
                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }, new string[] { }
                    }
                });
            });
        }
    }

    public struct Environments
    {
        public const string Local = "Local";
        public const string Dev = "Development";
        public const string Stage = "Stage";
        public const string Prod = "Production";
    }
}
