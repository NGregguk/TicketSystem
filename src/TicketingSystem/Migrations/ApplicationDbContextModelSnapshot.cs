using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TicketingSystem.Data;

#nullable disable

namespace TicketingSystem.Migrations;

[DbContext(typeof(ApplicationDbContext))]
public partial class ApplicationDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.8")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
        {
            b.Property<string>("Id")
                .HasColumnType("nvarchar(450)");

            b.Property<string>("ConcurrencyStamp")
                .IsConcurrencyToken()
                .HasColumnType("nvarchar(max)");

            b.Property<string>("Name")
                .HasMaxLength(256)
                .HasColumnType("nvarchar(256)");

            b.Property<string>("NormalizedName")
                .HasMaxLength(256)
                .HasColumnType("nvarchar(256)");

            b.HasKey("Id");

            b.HasIndex("NormalizedName")
                .IsUnique()
                .HasDatabaseName("RoleNameIndex")
                .HasFilter("[NormalizedName] IS NOT NULL");

            b.ToTable("AspNetRoles", (string)null);
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasAnnotation("SqlServer:Identity", "1, 1");

            b.Property<string>("ClaimType")
                .HasColumnType("nvarchar(max)");

            b.Property<string>("ClaimValue")
                .HasColumnType("nvarchar(max)");

            b.Property<string>("RoleId")
                .IsRequired()
                .HasColumnType("nvarchar(450)");

            b.HasKey("Id");

            b.HasIndex("RoleId");

            b.ToTable("AspNetRoleClaims", (string)null);
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasAnnotation("SqlServer:Identity", "1, 1");

            b.Property<string>("ClaimType")
                .HasColumnType("nvarchar(max)");

            b.Property<string>("ClaimValue")
                .HasColumnType("nvarchar(max)");

            b.Property<string>("UserId")
                .IsRequired()
                .HasColumnType("nvarchar(450)");

            b.HasKey("Id");

            b.HasIndex("UserId");

            b.ToTable("AspNetUserClaims", (string)null);
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
        {
            b.Property<string>("LoginProvider")
                .HasMaxLength(128)
                .HasColumnType("nvarchar(128)");

            b.Property<string>("ProviderKey")
                .HasMaxLength(128)
                .HasColumnType("nvarchar(128)");

            b.Property<string>("ProviderDisplayName")
                .HasColumnType("nvarchar(max)");

            b.Property<string>("UserId")
                .IsRequired()
                .HasColumnType("nvarchar(450)");

            b.HasKey("LoginProvider", "ProviderKey");

            b.HasIndex("UserId");

            b.ToTable("AspNetUserLogins", (string)null);
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
        {
            b.Property<string>("UserId")
                .HasColumnType("nvarchar(450)");

            b.Property<string>("RoleId")
                .HasColumnType("nvarchar(450)");

            b.HasKey("UserId", "RoleId");

            b.HasIndex("RoleId");

            b.ToTable("AspNetUserRoles", (string)null);
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
        {
            b.Property<string>("UserId")
                .HasColumnType("nvarchar(450)");

            b.Property<string>("LoginProvider")
                .HasMaxLength(128)
                .HasColumnType("nvarchar(128)");

            b.Property<string>("Name")
                .HasMaxLength(128)
                .HasColumnType("nvarchar(128)");

            b.Property<string>("Value")
                .HasColumnType("nvarchar(max)");

            b.HasKey("UserId", "LoginProvider", "Name");

            b.ToTable("AspNetUserTokens", (string)null);
        });

        modelBuilder.Entity("TicketingSystem.Models.ApplicationUser", b =>
        {
            b.Property<string>("Id")
                .HasColumnType("nvarchar(450)");

            b.Property<int>("AccessFailedCount")
                .HasColumnType("int");

            b.Property<string>("ConcurrencyStamp")
                .IsConcurrencyToken()
                .HasColumnType("nvarchar(max)");

            b.Property<string>("DisplayName")
                .HasColumnType("nvarchar(max)");

            b.Property<string>("Email")
                .HasMaxLength(256)
                .HasColumnType("nvarchar(256)");

            b.Property<bool>("EmailConfirmed")
                .HasColumnType("bit");

            b.Property<bool>("LockoutEnabled")
                .HasColumnType("bit");

            b.Property<DateTimeOffset?>("LockoutEnd")
                .HasColumnType("datetimeoffset");

            b.Property<string>("NormalizedEmail")
                .HasMaxLength(256)
                .HasColumnType("nvarchar(256)");

            b.Property<string>("NormalizedUserName")
                .HasMaxLength(256)
                .HasColumnType("nvarchar(256)");

            b.Property<string>("PasswordHash")
                .HasColumnType("nvarchar(max)");

            b.Property<string>("PhoneNumber")
                .HasColumnType("nvarchar(max)");

            b.Property<bool>("PhoneNumberConfirmed")
                .HasColumnType("bit");

            b.Property<string>("SecurityStamp")
                .HasColumnType("nvarchar(max)");

            b.Property<bool>("TwoFactorEnabled")
                .HasColumnType("bit");

            b.Property<string>("UserName")
                .HasMaxLength(256)
                .HasColumnType("nvarchar(256)");

            b.HasKey("Id");

            b.HasIndex("NormalizedEmail")
                .HasDatabaseName("EmailIndex");

            b.HasIndex("NormalizedUserName")
                .IsUnique()
                .HasDatabaseName("UserNameIndex")
                .HasFilter("[NormalizedUserName] IS NOT NULL");

            b.ToTable("AspNetUsers", (string)null);
        });

        modelBuilder.Entity("TicketingSystem.Models.Category", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasAnnotation("SqlServer:Identity", "1, 1");

            b.Property<bool>("IsActive")
                .HasColumnType("bit");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.HasKey("Id");

            b.HasIndex("Name")
                .IsUnique();

            b.ToTable("Categories");
        });

        modelBuilder.Entity("TicketingSystem.Models.Ticket", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasAnnotation("SqlServer:Identity", "1, 1");

            b.Property<string>("AssignedAdminUserId")
                .HasColumnType("nvarchar(450)");

            b.Property<int>("CategoryId")
                .HasColumnType("int");

            b.Property<DateTime?>("ClosedAtUtc")
                .HasColumnType("datetime2");

            b.Property<DateTime>("CreatedAtUtc")
                .HasColumnType("datetime2");

            b.Property<string>("Description")
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            b.Property<int>("Priority")
                .HasColumnType("int");

            b.Property<string>("RequesterUserId")
                .IsRequired()
                .HasColumnType("nvarchar(450)");

            b.Property<byte[]>("RowVersion")
                .IsRowVersion()
                .HasColumnType("rowversion");

            b.Property<int>("Status")
                .HasColumnType("int");

            b.Property<string>("Title")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            b.Property<DateTime>("UpdatedAtUtc")
                .HasColumnType("datetime2");

            b.HasKey("Id");

            b.HasIndex("AssignedAdminUserId");

            b.HasIndex("CategoryId");

            b.HasIndex("RequesterUserId");

            b.ToTable("Tickets");
        });

        modelBuilder.Entity("TicketingSystem.Models.TicketAttachment", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasAnnotation("SqlServer:Identity", "1, 1");

            b.Property<string>("ContentType")
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            b.Property<string>("OriginalFileName")
                .IsRequired()
                .HasMaxLength(260)
                .HasColumnType("nvarchar(260)");

            b.Property<long>("SizeBytes")
                .HasColumnType("bigint");

            b.Property<string>("StoredFileName")
                .IsRequired()
                .HasMaxLength(260)
                .HasColumnType("nvarchar(260)");

            b.Property<int>("TicketId")
                .HasColumnType("int");

            b.Property<DateTime>("UploadedAtUtc")
                .HasColumnType("datetime2");

            b.Property<string>("UploadedByUserId")
                .IsRequired()
                .HasColumnType("nvarchar(450)");

            b.HasKey("Id");

            b.HasIndex("TicketId");

            b.HasIndex("UploadedByUserId");

            b.ToTable("TicketAttachments");
        });

        modelBuilder.Entity("TicketingSystem.Models.TicketComment", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasAnnotation("SqlServer:Identity", "1, 1");

            b.Property<string>("AuthorUserId")
                .IsRequired()
                .HasColumnType("nvarchar(450)");

            b.Property<string>("Body")
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            b.Property<DateTime>("CreatedAtUtc")
                .HasColumnType("datetime2");

            b.Property<bool>("IsPublic")
                .HasColumnType("bit");

            b.Property<int>("TicketId")
                .HasColumnType("int");

            b.HasKey("Id");

            b.HasIndex("AuthorUserId");

            b.HasIndex("TicketId");

            b.ToTable("TicketComments");
        });

        modelBuilder.Entity("TicketingSystem.Models.TicketInternalNote", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int")
                .HasAnnotation("SqlServer:Identity", "1, 1");

            b.Property<string>("AuthorUserId")
                .IsRequired()
                .HasColumnType("nvarchar(450)");

            b.Property<string>("Body")
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            b.Property<DateTime>("CreatedAtUtc")
                .HasColumnType("datetime2");

            b.Property<int>("TicketId")
                .HasColumnType("int");

            b.HasKey("Id");

            b.HasIndex("AuthorUserId");

            b.HasIndex("TicketId");

            b.ToTable("TicketInternalNotes");
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
        {
            b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                .WithMany()
                .HasForeignKey("RoleId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
        {
            b.HasOne("TicketingSystem.Models.ApplicationUser", null)
                .WithMany()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
        {
            b.HasOne("TicketingSystem.Models.ApplicationUser", null)
                .WithMany()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
        {
            b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                .WithMany()
                .HasForeignKey("RoleId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne("TicketingSystem.Models.ApplicationUser", null)
                .WithMany()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
        {
            b.HasOne("TicketingSystem.Models.ApplicationUser", null)
                .WithMany()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("TicketingSystem.Models.Ticket", b =>
        {
            b.HasOne("TicketingSystem.Models.ApplicationUser", "AssignedAdminUser")
                .WithMany()
                .HasForeignKey("AssignedAdminUserId")
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne("TicketingSystem.Models.Category", "Category")
                .WithMany("Tickets")
                .HasForeignKey("CategoryId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasOne("TicketingSystem.Models.ApplicationUser", "RequesterUser")
                .WithMany()
                .HasForeignKey("RequesterUserId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.Navigation("AssignedAdminUser");

            b.Navigation("Category");

            b.Navigation("RequesterUser");
        });

        modelBuilder.Entity("TicketingSystem.Models.TicketAttachment", b =>
        {
            b.HasOne("TicketingSystem.Models.Ticket", "Ticket")
                .WithMany("Attachments")
                .HasForeignKey("TicketId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne("TicketingSystem.Models.ApplicationUser", "UploadedByUser")
                .WithMany()
                .HasForeignKey("UploadedByUserId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.Navigation("Ticket");

            b.Navigation("UploadedByUser");
        });

        modelBuilder.Entity("TicketingSystem.Models.TicketComment", b =>
        {
            b.HasOne("TicketingSystem.Models.ApplicationUser", "AuthorUser")
                .WithMany()
                .HasForeignKey("AuthorUserId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasOne("TicketingSystem.Models.Ticket", "Ticket")
                .WithMany("Comments")
                .HasForeignKey("TicketId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.Navigation("AuthorUser");

            b.Navigation("Ticket");
        });

        modelBuilder.Entity("TicketingSystem.Models.TicketInternalNote", b =>
        {
            b.HasOne("TicketingSystem.Models.ApplicationUser", "AuthorUser")
                .WithMany()
                .HasForeignKey("AuthorUserId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.HasOne("TicketingSystem.Models.Ticket", "Ticket")
                .WithMany("InternalNotes")
                .HasForeignKey("TicketId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.Navigation("AuthorUser");

            b.Navigation("Ticket");
        });
    }
}
