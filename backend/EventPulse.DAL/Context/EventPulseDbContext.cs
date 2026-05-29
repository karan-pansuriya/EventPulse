using EventPulse.DAL.Entities;
using EventPulse.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.DAL.Context
{
    public class EventPulseDbContext : DbContext
    {
        public EventPulseDbContext(DbContextOptions<EventPulseDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Venue> Venues => Set<Venue>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<EventPoster> EventPosters => Set<EventPoster>();
        public DbSet<Ticket> Tickets => Set<Ticket>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Role ──────────────────────────────────────────────────────────
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => r.Name).IsUnique();
                entity.Property(r => r.Name).HasMaxLength(50).IsRequired();
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(r => r.IsDeleted).HasDefaultValue(false);
                entity.HasQueryFilter(r => !r.IsDeleted);
            });

            // ── User ──────────────────────────────────────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(u => u.UpdatedAt).HasDefaultValueSql("now()");
                entity.Property(u => u.IsActive).HasDefaultValue(true);
                entity.Property(u => u.IsDeleted).HasDefaultValue(false);

                entity.HasMany(u => u.UserRoles)
                      .WithOne(ur => ur.User)
                      .HasForeignKey(ur => ur.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.RefreshTokens)
                      .WithOne(rt => rt.User)
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(u => !u.IsDeleted);
            });

            // ── UserRole ──────────────────────────────────────────────────────
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("user_roles");
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.Role)
                      .WithMany(r => r.UserRoles)
                      .HasForeignKey(ur => ur.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── RefreshToken ───────────────────────────────────────────────────
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");
                entity.HasKey(rt => rt.Id);
                entity.Property(rt => rt.Token).IsRequired();
                entity.Property(rt => rt.ExpiresAt).IsRequired();
                entity.Property(rt => rt.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(rt => rt.IsRevoked).HasDefaultValue(false);

                entity.HasIndex(rt => rt.Token).IsUnique();
            });

            // ── Category ─────────────────────────────────────────────────────
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.Name).IsUnique();
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(c => c.UpdatedAt).HasDefaultValueSql("now()");
                entity.Property(c => c.IsActive).HasDefaultValue(true);
                entity.Property(c => c.IsDeleted).HasDefaultValue(false);

                entity.HasQueryFilter(c => !c.IsDeleted);
            });

            // ── Event ─────────────────────────────────────────────────────────
            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("events");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                entity.HasOne(e => e.Organizer)
                      .WithMany(u => u.OrganizedEvents)
                      .HasForeignKey(e => e.OrganizerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Events)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Venue)
                      .WithMany(v => v.Events)
                      .HasForeignKey(e => e.VenueId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ── Venue ─────────────────────────────────────────────────────────
            modelBuilder.Entity<Venue>(entity =>
            {
                entity.ToTable("venues");
                entity.HasKey(v => v.Id);
                entity.Property(v => v.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(v => v.UpdatedAt).HasDefaultValueSql("now()");
                entity.Property(v => v.IsActive).HasDefaultValue(true);
                entity.Property(v => v.IsDeleted).HasDefaultValue(false);

                entity.HasQueryFilter(v => !v.IsDeleted);
            });

            // ── EventPoster ──────────────────────────────────────────────────
            modelBuilder.Entity<EventPoster>(entity =>
            {
                entity.ToTable("event_posters");
                entity.HasKey(ep => ep.Id);
                entity.Property(ep => ep.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(ep => ep.IsDeleted).HasDefaultValue(false);

                entity.HasOne(ep => ep.Event)
                      .WithMany(e => e.Posters)
                      .HasForeignKey(ep => ep.EventId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(ep => !ep.IsDeleted);
            });

            // ── Booking ───────────────────────────────────────────────────────
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("bookings");
                entity.HasKey(b => b.Id);

                entity.HasIndex(b => b.UniqueCode).IsUnique();

                entity.Property(b => b.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(b => b.UpdatedAt).HasDefaultValueSql("now()");
                entity.Property(b => b.IsDeleted).HasDefaultValue(false);
                entity.Property(b => b.Quantity).HasDefaultValue(1);

                entity.Property(b => b.PaymentStatus)
                      .HasConversion<string>()
                      .HasDefaultValue(Enums.PaymentStatus.Pending);

                entity.Property(b => b.BookingStatus)
                      .HasConversion<string>()
                      .HasDefaultValue(Enums.BookingStatus.Confirmed);

                entity.HasOne(b => b.User)
                      .WithMany(u => u.Bookings)
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Event)
                      .WithMany(e => e.Bookings)
                      .HasForeignKey(b => b.EventId)
                      .OnDelete(DeleteBehavior.Restrict);

            });

            // ── Ticket ────────────────────────────────────────────────────────
modelBuilder.Entity<Ticket>(entity =>
{
    entity.ToTable("tickets");

    entity.HasKey(t => t.Id);

    entity.HasIndex(t => t.TicketCode)
          .IsUnique();

    entity.Property(t => t.TicketCode)
          .IsRequired();

    entity.Property(t => t.QrCode)
          .HasColumnName("qr_code");

    entity.Property(t => t.QrCodePath)
          .HasColumnName("qr_code_path");

    entity.Property(t => t.PdfPath)
          .HasColumnName("pdf_path");

    entity.Property(t => t.CreatedAt)
          .HasDefaultValueSql("now()");

    entity.HasOne(t => t.Booking)
          .WithMany(b => b.Tickets)
          .HasForeignKey(t => t.BookingId)
          .OnDelete(DeleteBehavior.Cascade);
});

            // ── Seed roles ────────────────────────────────────────────────────
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", CreatedAt = now },
                new Role { Id = 2, Name = "Organizer", CreatedAt = now },
                new Role { Id = 3, Name = "Customer", CreatedAt = now }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Concerts & Music", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 2, Name = "Sports", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 3, Name = "Tech", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 4, Name = "Comedy", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 5, Name = "Movies & Screenings", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 6, Name = "Theatre & Drama", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 7, Name = "Workshops", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 8, Name = "Conferences", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 9, Name = "Festivals", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 10, Name = "Food & Drink", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 11, Name = "Arts & Exhibitions", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 12, Name = "Gaming & Esports", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 13, Name = "Education", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 14, Name = "Health & Wellness", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 15, Name = "Networking", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 16, Name = "Travel & Adventure", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 17, Name = "Business", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 18, Name = "Fashion", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 19, Name = "Cultural Events", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 20, Name = "Expos & Trade Shows", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 21, Name = "Webinars", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true },
                new Category { Id = 22, Name = "Career & Jobs", ImagePath = "/categories/category_image.png", CreatedAt = now, IsActive = true }
            );
        }

        public override int SaveChanges()
        {
            StampUpdatedAt();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            StampUpdatedAt();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void StampUpdatedAt()
        {
            var entries = ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
