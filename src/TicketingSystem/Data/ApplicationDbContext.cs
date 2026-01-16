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
    public DbSet<InternalSystem> InternalSystems => Set<InternalSystem>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketInternalNote> TicketInternalNotes => Set<TicketInternalNote>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
    public DbSet<TicketSubscriber> TicketSubscribers => Set<TicketSubscriber>();
    public DbSet<TicketEvent> TicketEvents => Set<TicketEvent>();
    public DbSet<TicketTimeEntry> TicketTimeEntries => Set<TicketTimeEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<InternalSystem>(entity =>
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

            entity.HasOne(x => x.InternalSystem)
                .WithMany(s => s.Tickets)
                .HasForeignKey(x => x.InternalSystemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RequesterUser)
                .WithMany()
                .HasForeignKey(x => x.RequesterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssignedAdminUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedAdminUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.ReopenedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReopenedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
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
            entity.HasIndex(x => x.TempKey);

            entity.HasOne(x => x.Ticket)
                .WithMany(t => t.Attachments)
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.UploadedByUser)
                .WithMany()
                .HasForeignKey(x => x.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TicketSubscriber>(entity =>
        {
            entity.HasIndex(x => new { x.TicketId, x.UserId }).IsUnique();

            entity.HasOne(x => x.Ticket)
                .WithMany(t => t.Subscribers)
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AddedByUser)
                .WithMany()
                .HasForeignKey(x => x.AddedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TicketEvent>(entity =>
        {
            entity.HasOne(x => x.Ticket)
                .WithMany(t => t.Events)
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ActorUser)
                .WithMany()
                .HasForeignKey(x => x.ActorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TicketTimeEntry>(entity =>
        {
            entity.HasIndex(x => x.TicketId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.WorkDate);

            entity.HasOne(x => x.Ticket)
                .WithMany(t => t.TimeEntries)
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
