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

    public virtual DbSet<Timezone> Timezones { get; set; }

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
            entity.Property(e => e.CountryName)
                .HasMaxLength(100)
                .HasColumnName("country_name");
            entity.Property(e => e.Flag)
                .HasMaxLength(10)
                .HasColumnName("flag");
            entity.Property(e => e.IsoCode)
                .HasMaxLength(10)
                .HasColumnName("iso_code");
            entity.Property(e => e.PhoneCode)
                .HasMaxLength(10)
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

        modelBuilder.Entity<Timezone>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("timezone_pkey");

            entity.ToTable("timezone");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FkCountryId).HasColumnName("fk_country_id");
            entity.Property(e => e.GmtOffsetName)
                .HasMaxLength(100)
                .HasColumnName("gmt_offset_name");
            entity.Property(e => e.TimezoneName)
                .HasMaxLength(100)
                .HasColumnName("timezone_name");
            entity.Property(e => e.ZoneName)
                .HasMaxLength(100)
                .HasColumnName("zone_name");

            entity.HasOne(d => d.FkCountry).WithMany(p => p.Timezones)
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
            entity.Property(e => e.Password)
                .HasMaxLength(200)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsFixedLength()
                .HasColumnName("phone");
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
