using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Models;

namespace TicketingSystem.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketInternalNote> TicketInternalNotes => Set<TicketInternalNote>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<Ticket>(entity =>
        {
            entity.Property(x => x.RowVersion).IsRowVersion();

            entity.HasOne(x => x.Category)
                .WithMany(c => c.Tickets)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RequesterUser)
                .WithMany()
                .HasForeignKey(x => x.RequesterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssignedAdminUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedAdminUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<TicketComment>(entity =>
        {
            entity.HasOne(x => x.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.AuthorUser)
                .WithMany()
                .HasForeignKey(x => x.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TicketInternalNote>(entity =>
        {
            entity.HasOne(x => x.Ticket)
                .WithMany(t => t.InternalNotes)
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.AuthorUser)
                .WithMany()
                .HasForeignKey(x => x.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TicketAttachment>(entity =>
        {
            entity.HasOne(x => x.Ticket)
                .WithMany(t => t.Attachments)
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.UploadedByUser)
                .WithMany()
                .HasForeignKey(x => x.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
