using ClientPortal.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClientPortal.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // KYC related DbSets
        public DbSet<ChecklistAck> ChecklistAcks => Set<ChecklistAck>();
        public DbSet<IndividualKyc> IndividualKycs => Set<IndividualKyc>();
        public DbSet<CompanyKyc> CompanyKycs => Set<CompanyKyc>();
        public DbSet<KycDocument> KycDocuments => Set<KycDocument>();
        public DbSet<KycScreening> KycScreenings => Set<KycScreening>();

        // Dashboard related DbSets
        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<Claim> Claims => Set<Claim>();
        public DbSet<Renewal> Renewals => Set<Renewal>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<Interaction> Interactions => Set<Interaction>();
        public DbSet<Notification> Notifications => Set<Notification>();

        // NEW: needed by Products & Services + Recent activity
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Quotation> Quotations => Set<Quotation>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);
           


            // ApplicationUser configuration
            b.Entity<ApplicationUser>()
               .Property(u => u.AccountBalance)
               .HasPrecision(18, 2);

            // KYC Screening configuration
            b.Entity<KycScreening>()
                .HasKey(ks => ks.Id);

            b.Entity<KycScreening>()
                .HasOne(ks => ks.Company)
                .WithMany(u => u.KycScreenings)
                .HasForeignKey(ks => ks.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<KycScreening>()
                .Property(ks => ks.ScreeningId)
                .IsRequired()
                .HasMaxLength(100);

            // KycScreening indexes (correct generic syntax)
            b.Entity<KycScreening>().HasIndex(ks => ks.ScreeningId).IsUnique();
            b.Entity<KycScreening>().HasIndex(ks => ks.CompanyId);
            b.Entity<KycScreening>().HasIndex(ks => ks.Status);
            b.Entity<KycScreening>().HasIndex(ks => ks.SubmittedDate);


            // Policy configuration
            b.Entity<Policy>()
                .Property(p => p.Premium)
                .HasPrecision(18, 2);

            b.Entity<Policy>()
                .HasOne(p => p.User)
                .WithMany(u => u.Policies)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Policy>().HasIndex(p => p.UserId);
            b.Entity<Policy>().HasIndex(p => p.Status);
            b.Entity<Policy>().HasIndex(p => p.EndDate);

            // Claim configuration
            b.Entity<Claim>()
                .Property(c => c.Amount)
                .HasPrecision(18, 2);

            b.Entity<Claim>()
                .HasOne(c => c.User)
                .WithMany(u => u.Claims)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Claim>().HasIndex(c => c.UserId);
            b.Entity<Claim>().HasIndex(c => c.Status);
            b.Entity<Claim>().HasIndex(c => c.UpdatedAt);

            // Renewal configuration
            b.Entity<Renewal>()
                .HasOne(r => r.User)
                .WithMany(u => u.Renewals)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Renewal>().HasIndex(r => r.UserId);
            b.Entity<Renewal>().HasIndex(r => r.Status);

            // Document configuration
            b.Entity<Document>()
                .HasOne(d => d.User)
                .WithMany(u => u.Documents)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Document>().HasIndex(d => d.UserId);
            b.Entity<Document>().HasIndex(d => d.ExpiryDate);

            // Interaction configuration
            b.Entity<Interaction>()
                .HasOne(i => i.User)
                .WithMany(u => u.Interactions)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Interaction>().HasIndex(i => i.UserId);
            b.Entity<Interaction>().HasIndex(i => i.InteractionDate);

            // Notification configuration
            b.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Notification>().HasIndex(n => n.UserId);
            b.Entity<Notification>().HasIndex(n => new { n.IsRead, n.CreatedDate });

            // Quotation configuration (NEW)
            b.Entity<Quotation>()
                .HasOne(q => q.User)
                .WithMany(u => u.Quotations)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Quotation>().HasIndex(q => q.UserId);
            b.Entity<Quotation>().HasIndex(q => q.Status);
            b.Entity<Quotation>().HasIndex(q => q.UpdatedAt);

            // Product configuration (NEW)
            b.Entity<Product>().HasIndex(p => p.Active);
            b.Entity<Product>().HasIndex(p => p.Class);

            // Unique constraints / dedupe
            b.Entity<ChecklistAck>()
                .HasIndex(x => new { x.UserId, x.Type })
                .IsUnique();

            b.Entity<IndividualKyc>().HasIndex(x => x.UserId).IsUnique();
            b.Entity<CompanyKyc>().HasIndex(x => x.UserId).IsUnique();
        }
    }
}
