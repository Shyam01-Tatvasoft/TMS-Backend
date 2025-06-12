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

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SubTask> SubTasks { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskAssign> TaskAssigns { get; set; }

    public virtual DbSet<TimezoneDetail> TimezoneDetails { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=tms;Username=postgres;Password=Tatva@123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("role_pkey");

            entity.ToTable("role");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
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

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("task_pkey");

            entity.ToTable("task");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<TaskAssign>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("task_assign_pkey");

            entity.ToTable("task_assign");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.FkSubtaskId).HasColumnName("fk_subtask_id");
            entity.Property(e => e.FkTaskId).HasColumnName("fk_task_id");
            entity.Property(e => e.FkUserId).HasColumnName("fk_user_id");
            entity.Property(e => e.Priority)
                .HasDefaultValueSql("1")
                .HasColumnName("priority");
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

        modelBuilder.Entity<TimezoneDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("timezone_pkey");

            entity.ToTable("timezone_detail");

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

            entity.HasOne(d => d.FkCountry).WithMany(p => p.TimezoneDetails)
                .HasForeignKey(d => d.FkCountryId)
                .HasConstraintName("timezone_country_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_pkey");

            entity.ToTable("user");

            entity.HasIndex(e => e.Username, "user_unique").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.FkCountryId).HasColumnName("fk_country_id");
            entity.Property(e => e.FkCountryTimezone).HasColumnName("fk_country_timezone");
            entity.Property(e => e.FkRoleId).HasColumnName("fk_role_id");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValueSql("false")
                .HasColumnName("is_deleted");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.ModifiedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_at");
            entity.Property(e => e.Password)
                .HasMaxLength(200)
                .HasColumnName("password");
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
