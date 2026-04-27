using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DVT.WorkerService
{
    class Program
    {
        private static IConfiguration _configuration;

        private static string _environmentName = "";
        private static string _dvtClientId = "";
        //private static string _dvtTenantId = "";
        private static string _keyVaultName = "";
        private static string _keyVaultAccessClientSecret = "";
        private static string _keyVaultUri = "";

        static async Task<int> Main(string[] args)
        {
            try
            {
                //Initialize configurations
                _environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                _dvtClientId = Environment.GetEnvironmentVariable("KV_CLIENT_ID");
                //_dvtTenantId = Environment.GetEnvironmentVariable("KV_TENANT_ID");
                _keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
                _keyVaultAccessClientSecret = Environment.GetEnvironmentVariable("KV_CLIENT_SECRET");
                _keyVaultUri = "https://" + _keyVaultName + ".vault.azure.net";

                _configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile($"appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{_environmentName}.json", true, true)
                    .AddEnvironmentVariables()
                    .AddAzureKeyVault(_keyVaultUri, _dvtClientId, _keyVaultAccessClientSecret, new DefaultKeyVaultSecretManager())
                .Build();

                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                Log.Logger = new LoggerConfiguration()
                   .ReadFrom.Configuration(_configuration)
                   .CreateLogger();
                Log.Information("Worker Started. Ready to go...");

                await SayHelloAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);

                if (ex.InnerException != null)
                    Log.Error(ex.InnerException.Message + "\r\n" + ex.InnerException.StackTrace);

                Log.CloseAndFlush();

                return -1;
            }

            Log.CloseAndFlush();
            return 0;
        }

        public static Task SayHelloAsync()
        {
            try
            {
                Console.WriteLine("Hello from the worker service!");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return Task.CompletedTask;
        }
    }
}
