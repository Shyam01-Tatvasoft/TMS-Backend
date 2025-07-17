using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TMS.Repository.Data;

public partial class TmsContext : DbContext
{
    public TmsContext()
    {
    }

    public TmsContext(DbContextOptions<TmsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Counter> Counters { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<CountryTimezone> CountryTimezones { get; set; }

    public virtual DbSet<Hash> Hashes { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<Jobparameter> Jobparameters { get; set; }

    public virtual DbSet<Jobqueue> Jobqueues { get; set; }

    public virtual DbSet<List> Lists { get; set; }

    public virtual DbSet<Lock> Locks { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Schema> Schemas { get; set; }

    public virtual DbSet<Server> Servers { get; set; }

    public virtual DbSet<Set> Sets { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<SubTask> SubTasks { get; set; }

    public virtual DbSet<SystemConfiguration> SystemConfigurations { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskAction> TaskActions { get; set; }

    public virtual DbSet<TaskAssign> TaskAssigns { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserOtp> UserOtps { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=tms;Username=postgres;Password=Tatva@123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Counter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("counter_pkey");

            entity.ToTable("counter", "hangfire");

            entity.HasIndex(e => e.Expireat, "ix_hangfire_counter_expireat");

            entity.HasIndex(e => e.Key, "ix_hangfire_counter_key");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Expireat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expireat");
            entity.Property(e => e.Key).HasColumnName("key");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("country_pkey");

            entity.ToTable("country");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Flag)
                .HasMaxLength(100)
                .HasColumnName("flag");
            entity.Property(e => e.IsoCode)
                .HasMaxLength(100)
                .HasColumnName("iso_code");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PhoneCode)
                .HasMaxLength(100)
                .HasColumnName("phone_code");
        });

        modelBuilder.Entity<CountryTimezone>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("timezone_pkey");

            entity.ToTable("country_timezone");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('timezone_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.FkCountryId).HasColumnName("fk_country_id");
            entity.Property(e => e.Offset)
                .HasMaxLength(100)
                .HasColumnName("offset");
            entity.Property(e => e.Timezone)
                .HasMaxLength(100)
                .HasColumnName("timezone");
            entity.Property(e => e.Zone)
                .HasMaxLength(100)
                .HasColumnName("zone");

            entity.HasOne(d => d.FkCountry).WithMany(p => p.CountryTimezones)
                .HasForeignKey(d => d.FkCountryId)
                .HasConstraintName("timezone_country_id_fkey");
        });

        modelBuilder.Entity<Hash>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hash_pkey");

            entity.ToTable("hash", "hangfire");

            entity.HasIndex(e => new { e.Key, e.Field }, "hash_key_field_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Expireat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expireat");
            entity.Property(e => e.Field).HasColumnName("field");
            entity.Property(e => e.Key).HasColumnName("key");
            entity.Property(e => e.Updatecount).HasColumnName("updatecount");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("job_pkey");

            entity.ToTable("job", "hangfire");

            entity.HasIndex(e => e.Statename, "ix_hangfire_job_statename");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Arguments).HasColumnName("arguments");
            entity.Property(e => e.Createdat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Expireat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expireat");
            entity.Property(e => e.Invocationdata).HasColumnName("invocationdata");
            entity.Property(e => e.Stateid).HasColumnName("stateid");
            entity.Property(e => e.Statename).HasColumnName("statename");
            entity.Property(e => e.Updatecount).HasColumnName("updatecount");
        });

        modelBuilder.Entity<Jobparameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("jobparameter_pkey");

            entity.ToTable("jobparameter", "hangfire");

            entity.HasIndex(e => new { e.Jobid, e.Name }, "ix_hangfire_jobparameter_jobidandname");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Jobid).HasColumnName("jobid");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Updatecount).HasColumnName("updatecount");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Job).WithMany(p => p.Jobparameters)
                .HasForeignKey(d => d.Jobid)
                .HasConstraintName("jobparameter_jobid_fkey");
        });

        modelBuilder.Entity<Jobqueue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("jobqueue_pkey");

            entity.ToTable("jobqueue", "hangfire");

            entity.HasIndex(e => new { e.Jobid, e.Queue }, "ix_hangfire_jobqueue_jobidandqueue");

            entity.HasIndex(e => new { e.Queue, e.Fetchedat }, "ix_hangfire_jobqueue_queueandfetchedat");

            entity.HasIndex(e => new { e.Queue, e.Fetchedat, e.Jobid }, "jobqueue_queue_fetchat_jobid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Fetchedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fetchedat");
            entity.Property(e => e.Jobid).HasColumnName("jobid");
            entity.Property(e => e.Queue).HasColumnName("queue");
            entity.Property(e => e.Updatecount).HasColumnName("updatecount");
        });

        modelBuilder.Entity<List>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("list_pkey");

            entity.ToTable("list", "hangfire");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Expireat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expireat");
            entity.Property(e => e.Key).HasColumnName("key");
            entity.Property(e => e.Updatecount).HasColumnName("updatecount");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Lock>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("lock", "hangfire");

            entity.HasIndex(e => e.Resource, "lock_resource_key").IsUnique();

            entity.Property(e => e.Acquired)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("acquired");
            entity.Property(e => e.Resource).HasColumnName("resource");
            entity.Property(e => e.Updatecount).HasColumnName("updatecount");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("logs_pkey");

            entity.ToTable("logs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .HasColumnName("action");
            entity.Property(e => e.Data).HasColumnName("data");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.FkUserId).HasColumnName("fk_user_id");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Stacktrash).HasColumnName("stacktrash");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notification_pkey");

            entity.ToTable("notification");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.FkTaskId).HasColumnName("fk_task_id");
            entity.Property(e => e.FkUserId).HasColumnName("fk_user_id");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.FkTask).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.FkTaskId)
                .HasConstraintName("notification_fk_task_id_fkey");

            entity.HasOne(d => d.FkUser).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.FkUserId)
                .HasConstraintName("notification_fk_user_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("role_pkey");

            entity.ToTable("role");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Schema>(entity =>
        {
            entity.HasKey(e => e.Version).HasName("schema_pkey");

            entity.ToTable("schema", "hangfire");

            entity.Property(e => e.Version)
                .ValueGeneratedNever()
                .HasColumnName("version");
        });

        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("server_pkey");

            entity.ToTable("server", "hangfire");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Data).HasColumnName("data");
            entity.Property(e => e.Lastheartbeat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lastheartbeat");
            entity.Property(e => e.Updatecount).HasColumnName("updatecount");
        });

        modelBuilder.Entity<Set>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("set_pkey");

            entity.ToTable("set", "hangfire");

            entity.HasIndex(e => new { e.Key, e.Value }, "set_key_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Expireat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expireat");
            entity.Property(e => e.Key).HasColumnName("key");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.Updatecount).HasColumnName("updatecount");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("state_pkey");

            entity.ToTable("state", "hangfire");

            entity.HasIndex(e => e.Jobid, "ix_hangfire_state_jobid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Data).HasColumnName("data");
            entity.Property(e => e.Jobid).HasColumnName("jobid");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Updatecount).HasColumnName("updatecount");

            entity.HasOne(d => d.Job).WithMany(p => p.States)
                .HasForeignKey(d => d.Jobid)
                .HasConstraintName("state_jobid_fkey");
        });

        modelBuilder.Entity<SubTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sub_task_pkey");

            entity.ToTable("sub_task");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FkTaskId).HasColumnName("fk_task_id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");

            entity.HasOne(d => d.FkTask).WithMany(p => p.SubTasks)
                .HasForeignKey(d => d.FkTaskId)
                .HasConstraintName("sub_task_fk_task_id_fkey");
        });

        modelBuilder.Entity<SystemConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("system_configuratiions_pkey");

            entity.ToTable("system_configurations");

            entity.HasIndex(e => e.ConfigName, "unique_config_name").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('system_configuratiions_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.ConfigName)
                .HasMaxLength(100)
                .HasColumnName("config_name");
            entity.Property(e => e.ConfigValue).HasColumnName("config_value");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("task_pkey");

            entity.ToTable("task");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<TaskAction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("task_action_pkey");

            entity.ToTable("task_action");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FkTaskId).HasColumnName("fk_task_id");
            entity.Property(e => e.FkUserId).HasColumnName("fk_user_id");
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("submitted_at");
            entity.Property(e => e.SubmittedData)
                .HasColumnType("jsonb")
                .HasColumnName("submitted_data");

            entity.HasOne(d => d.FkTask).WithMany(p => p.TaskActions)
                .HasForeignKey(d => d.FkTaskId)
                .HasConstraintName("task_action_fk_task_id_fkey");

            entity.HasOne(d => d.FkUser).WithMany(p => p.TaskActions)
                .HasForeignKey(d => d.FkUserId)
                .HasConstraintName("task_action_fk_user_id_fkey");
        });

        modelBuilder.Entity<TaskAssign>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("task_assign_pkey");

            entity.ToTable("task_assign");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DueDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("due_date");
            entity.Property(e => e.EndAfter).HasColumnName("end_after");
            entity.Property(e => e.FkSubtaskId).HasColumnName("fk_subtask_id");
            entity.Property(e => e.FkTaskId).HasColumnName("fk_task_id");
            entity.Property(e => e.FkUserId).HasColumnName("fk_user_id");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValueSql("false")
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsRecurrence)
                .HasDefaultValueSql("false")
                .HasColumnName("is_recurrence");
            entity.Property(e => e.Priority)
                .HasDefaultValueSql("1")
                .HasColumnName("priority");
            entity.Property(e => e.RecurrenceId)
                .HasMaxLength(100)
                .HasColumnName("recurrence_id");
            entity.Property(e => e.RecurrenceOn).HasColumnName("recurrence_on");
            entity.Property(e => e.RecurrencePattern).HasColumnName("recurrence_pattern");
            entity.Property(e => e.RecurrenceTo)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("recurrence_to");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("1")
                .HasColumnName("status");
            entity.Property(e => e.TaskData)
                .HasColumnType("jsonb")
                .HasColumnName("task_data");

            entity.HasOne(d => d.FkSubtask).WithMany(p => p.TaskAssigns)
                .HasForeignKey(d => d.FkSubtaskId)
                .HasConstraintName("task_assign_fk_subtask_id_fkey");

            entity.HasOne(d => d.FkTask).WithMany(p => p.TaskAssigns)
                .HasForeignKey(d => d.FkTaskId)
                .HasConstraintName("task_assign_fk_task_id_fkey");

            entity.HasOne(d => d.FkUser).WithMany(p => p.TaskAssigns)
                .HasForeignKey(d => d.FkUserId)
                .HasConstraintName("task_assign_fk_user_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_pkey");

            entity.ToTable("user");

            entity.HasIndex(e => e.Username, "user_unique").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthType)
                .HasDefaultValueSql("1")
                .HasColumnName("auth_type");
            entity.Property(e => e.BlockedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("blocked_at");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.FkCountryId).HasColumnName("fk_country_id");
            entity.Property(e => e.FkCountryTimezone).HasColumnName("fk_country_timezone");
            entity.Property(e => e.FkRoleId).HasColumnName("fk_role_id");
            entity.Property(e => e.InvalidLoginAttempts)
                .HasDefaultValueSql("0")
                .HasColumnName("invalid_login_attempts");
            entity.Property(e => e.IsBlocked)
                .HasDefaultValueSql("false")
                .HasColumnName("is_blocked");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValueSql("false")
                .HasColumnName("is_deleted");
            entity.Property(e => e.IsTwoFaEnabled)
                .HasDefaultValueSql("false")
                .HasColumnName("is_two_fa_enabled");
            entity.Property(e => e.LastInvalidAttemptAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_invalid_attempt_at");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.ModifiedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_at");
            entity.Property(e => e.Password)
                .HasMaxLength(200)
                .HasColumnName("password");
            entity.Property(e => e.PasswordExpiryDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("password_expiry_date");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsFixedLength()
                .HasColumnName("phone");
            entity.Property(e => e.ProfileImage).HasColumnName("profile_image");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.FkCountry).WithMany(p => p.Users)
                .HasForeignKey(d => d.FkCountryId)
                .HasConstraintName("user_country_id_fkey");

            entity.HasOne(d => d.FkCountryTimezoneNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.FkCountryTimezone)
                .HasConstraintName("user_countrytimezone_fkey");

            entity.HasOne(d => d.FkRole).WithMany(p => p.Users)
                .HasForeignKey(d => d.FkRoleId)
                .HasConstraintName("user_fk_role_id_fkey");
        });

        modelBuilder.Entity<UserOtp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_otp_pkey");

            entity.ToTable("user_otp");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.ExpiryTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expiry_time");
            entity.Property(e => e.OtpHash)
                .HasMaxLength(100)
                .HasColumnName("otp_hash");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
