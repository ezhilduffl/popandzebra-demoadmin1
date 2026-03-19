using Microsoft.EntityFrameworkCore;
using PopZebra.Models;

namespace PopZebra.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<HomeSection> HomeSections { get; set; }
        public DbSet<AboutSection> AboutSections { get; set; }
        public DbSet<AboutIcon> AboutIcons { get; set; }
        public DbSet<WorkItem> WorkItems { get; set; }
        public DbSet<ShopItem> ShopItems { get; set; }
        public DbSet<ShopItemHistory> ShopItemHistories { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AdminUser>(entity =>
            {
                entity.ToTable("AdminUsers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.OtpCode).HasMaxLength(10);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<HomeSection>(entity =>
            {
                entity.ToTable("HomeSections");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.SingleImagePath).HasMaxLength(500);
                entity.Property(e => e.MobileImagePath).HasMaxLength(500);
                entity.Property(e => e.DesktopImagePath).HasMaxLength(500);
                // ── NEW ─────────────────────────────────────────────────
                entity.Property(e => e.LinkUrl).HasMaxLength(500);
                // ────────────────────────────────────────────────────────
            });

            modelBuilder.Entity<AboutSection>(entity =>
            {
                entity.ToTable("AboutSections");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("GETUTCDATE()");
                entity.HasMany(e => e.Icons)
                      .WithOne(i => i.AboutSection)
                      .HasForeignKey(i => i.AboutSectionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AboutIcon>(entity =>
            {
                entity.ToTable("AboutIcons");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.IconPath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.LinkUrl).IsRequired().HasMaxLength(500);
            });

            modelBuilder.Entity<WorkItem>(entity =>
            {
                entity.ToTable("WorkItems");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.LinkUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ImagePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.DisplayOrder).IsUnique();
            });

            modelBuilder.Entity<ShopItem>(entity =>
            {
                entity.ToTable("ShopItems");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.ImagePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedOn).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedOn).HasDefaultValueSql("GETUTCDATE()");
                entity.HasMany(e => e.History)
                      .WithOne(h => h.ShopItem)
                      .HasForeignKey(h => h.ShopItemId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ShopItemHistory>(entity =>
            {
                entity.ToTable("ShopItemHistory");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.ImagePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.SavedOn).HasDefaultValueSql("GETUTCDATE()");
            });


            modelBuilder.Entity<ErrorLog>(entity =>
            {
                entity.ToTable("ErrorLogs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ErrorMessage).IsRequired();
                entity.Property(e => e.PageName).HasMaxLength(500);
                entity.Property(e => e.ErrorLine).HasMaxLength(500);
                entity.Property(e => e.AddedDate)
                      .HasDefaultValueSql("GETUTCDATE()");
            });






        }
    }
}