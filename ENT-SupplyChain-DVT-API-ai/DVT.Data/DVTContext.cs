using DVT.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DVT.Data;

public partial class DVTContext : DbContext
{
    public DVTContext(DbContextOptions<DVTContext> options)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<UserInfo> UserInfos { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobFile> JobFiles { get; set; }
    public DbSet<MasterData> MasterData { get; set; }
    public DbSet<ConfigSetting> ConfigSettings { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "en_US.utf8");

        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.ToTable("user_info");
            entity.HasKey(e => e.UserInfoId)
                .HasName("user_PK");
            entity.Property(e => e.UserInfoId)
                .HasColumnName("user_info_id");
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.EmailAddress)
              .IsRequired()
              .HasMaxLength(200)
              .HasColumnName("email_address");
            entity.Property(e => e.LoadFolder)
               .HasMaxLength(1000)
               .HasColumnName("load_directory");
            entity.Property(e => e.LogFolder)
              .HasMaxLength(1000)
              .HasColumnName("log_directory");
            entity.Property(e => e.ProductionFolder)
             .HasMaxLength(1000)
             .HasColumnName("output_directory");
            entity.Property(e => e.UpdateBy)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .HasColumnName("update_date");
            entity.Property(e => e.Deleted)
                .HasColumnName("deleted");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.ToTable("job");
            entity.HasKey(e => e.JobId)
                .HasName("job_pkey");
            entity.Property(e => e.JobId)
               .IsRequired()
               .HasColumnName("job_id");
            entity.Property(e => e.DivisionId)
                .IsRequired()
                .HasColumnName("division_id");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.FeedNumber)
                .HasColumnName("feed_number");
            entity.Property(e => e.ArchiveFilePath)
              .HasMaxLength(500)
              .HasColumnName("archive_file_path");
            entity.Property(e => e.UserInfoId)
                .HasColumnName("user_info_id");
            entity.Property(e => e.CreateBy)
               .IsRequired()
               .HasMaxLength(200)
              .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
             .IsRequired()
              .HasColumnName("create_date");
            entity.Property(e => e.UpdateBy)
                .IsRequired()
                .HasMaxLength(200)
               .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
                .IsRequired()
                .HasColumnName("update_date");
            entity.Property(e => e.Deleted)
                .IsRequired()
                .HasColumnName("deleted");
        });

        modelBuilder.Entity<JobFile>(entity =>
        {
            entity.ToTable("job_file");
            entity.HasKey(e => e.JobFileId)
                .HasName("job_file_pkey");
            entity.Property(e => e.JobFileId)
               .IsRequired()
               .HasColumnName("job_file_id");
            entity.Property(e => e.JobId)
                .IsRequired()
                .HasColumnName("job_id");
            entity.Property(e => e.FileType)
                .HasMaxLength(20)
                .HasColumnName("file_type");
            entity.Property(e => e.TableName)
                .HasMaxLength(100)
                .HasColumnName("table_name");
            entity.Property(e => e.SortOrder)
                .HasColumnName("sort_order");
            entity.Property(e => e.DependsOnFileType)
                .HasMaxLength(100)
                .HasColumnName("depends_on_file_type");
            entity.Property(e => e.FileName)
             .HasMaxLength(200)
             .HasColumnName("file_name");
            entity.Property(e => e.FilePath)
              .HasMaxLength(500)
              .HasColumnName("file_path");
            entity.Property(e => e.Status)
               .HasMaxLength(20)
               .HasColumnName("status");
            entity.Property(e => e.FileCreationTimestamp)
                .HasColumnName("file_creation_timestamp");
            entity.Property(e => e.FileLastModifiedTimestamp)
                .HasColumnName("file_last_modified_timestamp");
            entity.Property(e => e.RecordCount)
               .HasColumnName("record_count");
            entity.Property(e => e.LoadDate)
              .HasColumnName("load_date");
            entity.Property(e => e.ValidationMessages)
             .HasColumnName("validation_message");
            entity.Property(e => e.ValidationStats)
           .HasColumnName("validation_stats");
            entity.Property(e => e.UpdateBy)
               .IsRequired()
               .HasMaxLength(200)
              .HasColumnName("update_by");
            entity.Property(e => e.UpdateDate)
             .IsRequired()
              .HasColumnName("update_date");
            entity.Property(e => e.Deleted)
                .IsRequired()
                .HasColumnName("deleted");
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("activity_log");
            entity.HasKey(e => e.LogId)
                .HasName("log_pkey");
            entity.Property(e => e.LogId)
               .IsRequired()
               .HasColumnName("log_id");
            entity.Property(e => e.EntityId)
                .IsRequired()
                .HasColumnName("entity_id");
            entity.Property(e => e.Entity)
                .HasMaxLength(100)
                .HasColumnName("entity");
            entity.Property(e => e.MessageType)
                .HasMaxLength(50)
                .HasColumnName("message_type");
            entity.Property(e => e.Message)
                .HasMaxLength(1000)
                .HasColumnName("message");
            entity.Property(e => e.CreateBy)
               .IsRequired()
               .HasMaxLength(200)
              .HasColumnName("create_by");
            entity.Property(e => e.CreateDate)
             .IsRequired()
              .HasColumnName("create_date");
            entity.Property(e => e.Deleted)
                .IsRequired()
                .HasColumnName("deleted");
        });

        modelBuilder.Entity<MasterData>(entity =>
        {
            entity.ToTable("master_data");
            entity.HasKey(e => e.ItemId)
                .HasName("master_data_pkey");
            entity.Property(e => e.ItemId)
               .IsRequired()
               .HasColumnName("item_id");
            entity.Property(e => e.TableName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("table_name");
            entity.Property(e => e.TextId)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("text_id");
            entity.Property(e => e.ItemName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("item_name");
            entity.Property(e => e.ItemNameAbbrev)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("item_name_abbrev");
            entity.Property(e => e.Text1)
                .HasMaxLength(100)
                .HasColumnName("text1");
            entity.Property(e => e.Text2)
                .HasMaxLength(100)
                .HasColumnName("text2");
            entity.Property(e => e.Text3)
                .HasMaxLength(100)
                .HasColumnName("text3");
            entity.Property(e => e.Text4)
                .HasMaxLength(100)
                .HasColumnName("text4");
            entity.Property(e => e.Text5)
                .HasMaxLength(100)
                .HasColumnName("text5");
            entity.Property(e => e.Text6)
                .HasMaxLength(100)
                .HasColumnName("text6");
            entity.Property(e => e.UpdateDate)
             .IsRequired()
              .HasColumnName("update_date");
            entity.Property(e => e.UpdateBy)
              .IsRequired()
              .HasMaxLength(200)
             .HasColumnName("update_by");
            entity.Property(e => e.Deleted)
                .IsRequired()
                .HasColumnName("deleted");
        });

        modelBuilder.Entity<ConfigSetting>(entity =>
        {
            entity.HasKey(e => e.SettingId)
                .HasName("setting_id");

            entity.ToTable("config_setting");
            entity.Property(e => e.SettingId)
               .IsRequired()
               .HasColumnName("setting_id");
            entity.Property(e => e.Module)
             .IsRequired()
             .HasMaxLength(200)
             .HasColumnName("module");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("name");

            entity.Property(e => e.DataType)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("data_type");

            entity.Property(e => e.Value)
                .IsRequired()
                .HasMaxLength(2000)
                .HasColumnName("value");

            entity.Property(e => e.UpdatedBy)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("updated_by");

            entity.Property(e => e.UpdatedDate)
                .IsRequired()
                .HasColumnName("updated_date");

            entity.Property(e => e.Deleted)
                .IsRequired()
                .HasColumnName("deleted");
        });

        base.OnModelCreating(modelBuilder);

        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}