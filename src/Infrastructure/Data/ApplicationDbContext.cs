using Application.Common.Interface;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext , IApplicationDbContext
    {
        private readonly ICurrentUserService _currentUserService;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService) : base(options) { _currentUserService = currentUserService; }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User-Role relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)                  // Each User has one Role
                .WithMany(r => r.Users)               // Each Role has many Users
                .HasForeignKey(u => u.RoleId)         // Foreign key in User
                .OnDelete(DeleteBehavior.Restrict);   // Optional: prevent cascade delete
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var auditLogs = new List<AuditLog>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var tableName = entry.Metadata.GetTableName();
                var key = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString();

                var audit = new AuditLog
                {
                    TableName = tableName ?? "Unknown",
                    Key = key ?? "Unknown",
                    Action = entry.State.ToString(), // Added / Modified / Deleted
                    PerformedBy = _currentUserService.UserId,
                    IPAddress = _currentUserService.IPAddress,
                    PerformedAt = now,
                };

                // Optional: capture field-level changes
                var changes = new Dictionary<string, object>();
                foreach (var prop in entry.Properties)
                {
                    if (entry.State == EntityState.Added)
                    {
                        changes[prop.Metadata.Name] = prop.CurrentValue ?? "null";
                    }
                    else if (entry.State == EntityState.Modified && prop.IsModified)
                    {
                        changes[$"{prop.Metadata.Name}_Old"] = prop.OriginalValue ?? "null";
                        changes[$"{prop.Metadata.Name}_New"] = prop.CurrentValue ?? "null";
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        changes[prop.Metadata.Name] = prop.OriginalValue ?? "null";
                    }
                }

                audit.Changes = System.Text.Json.JsonSerializer.Serialize(changes);
                auditLogs.Add(audit);
            }

            // Save entity changes first
            var result = await base.SaveChangesAsync(cancellationToken);

            // Then save audit logs
            if (auditLogs.Any())
            {
                AuditLogs.AddRange(auditLogs);
                await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }


    }
}
