using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Reflection.Emit;

namespace BookStoreKAP.Database
{
    public class BookStoreKAPDBContext : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
    {
        public BookStoreKAPDBContext(DbContextOptions<BookStoreKAPDBContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure identity tables
            builder.Entity<User>(entity => { entity.ToTable("Users"); });
            builder.Entity<User>().Property(tb => tb.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<Role>(entity => { entity.ToTable("Roles"); });
            builder.Entity<UserRole>(entity => { entity.ToTable("UserRoles"); });
            builder.Entity<UserClaim>(entity => { entity.ToTable("UserClaims"); });
            builder.Entity<UserLogin>(entity => { entity.ToTable("UserLogins"); });
            builder.Entity<RoleClaim>(entity => { entity.ToTable("RoleClaims"); });
            builder.Entity<UserToken>(entity => { entity.ToTable("UserTokens"); });

            // Drop foreign keys and primary keys before altering column types
            builder.Entity<UserToken>().HasIndex(ut => ut.UserId).IsUnique(false);
            builder.Entity<UserRole>().HasIndex(ur => ur.UserId).IsUnique(false);
            builder.Entity<UserLogin>().HasIndex(ul => ul.UserId).IsUnique(false);
            builder.Entity<UserClaim>().HasIndex(uc => uc.UserId).IsUnique(false);

            builder.Entity<UserToken>().Ignore(ut => ut.User);
            builder.Entity<UserRole>().Ignore(ur => ur.User);
            builder.Entity<UserLogin>().Ignore(ul => ul.User);
            builder.Entity<UserClaim>().Ignore(uc => uc.User);

            // Alter column types
            builder.Entity<UserToken>()
                .Property(ut => ut.UserId)
                .HasColumnType("uniqueidentifier")
                .IsRequired();

            builder.Entity<UserRole>()
                .Property(ur => ur.UserId)
                .HasColumnType("uniqueidentifier")
                .IsRequired();

            builder.Entity<UserLogin>()
                .Property(ul => ul.UserId)
                .HasColumnType("uniqueidentifier")
                .IsRequired();

            builder.Entity<UserClaim>()
                .Property(uc => uc.UserId)
                .HasColumnType("uniqueidentifier")
                .IsRequired();

            builder.Entity<RoleClaim>()
                .Property(rc => rc.RoleId)
                .HasColumnType("uniqueidentifier")
                .IsRequired();

            // Add foreign keys and primary keys back with ON DELETE NO ACTION
            builder.Entity<UserToken>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserTokens)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<UserLogin>()
                .HasOne(ul => ul.User)
                .WithMany(u => u.UserLogins)
                .HasForeignKey(ul => ul.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<UserClaim>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserClaims)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<RoleClaim>()
                .HasOne(rc => rc.Role)
                .WithMany()
                .HasForeignKey(rc => rc.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Identity Guid
            builder.Entity<Book>().Property(e => e.ID).HasDefaultValueSql("NEWID()");
            builder.Entity<Genre>().Property(e => e.ID).HasDefaultValueSql("NEWID()");
            builder.Entity<Sale>().Property(e => e.ID).HasDefaultValueSql("NEWID()");
            builder.Entity<Order>().Property(e => e.ID).HasDefaultValueSql("NEWID()");
            builder.Entity<Rating>().Property(e => e.ID).HasDefaultValueSql("NEWID()");
            builder.Entity<Series>().Property(e => e.ID).HasDefaultValueSql("NEWID()");
            builder.Entity<Role>().Property(e => e.Id).HasDefaultValueSql("NEWID()");
            builder.Entity<Role>().Property(e => e.ConcurrencyStamp).HasDefaultValueSql("NEWID()");

            // Configuration Many to Many
            #region Table BookGenres
            builder.Entity<BookGenre>().HasKey(bg => new { bg.GenreID, bg.BookID });
            builder.Entity<BookGenre>().HasOne(bg => bg.Book).WithMany(b => b.BookGenres).HasForeignKey(bg => bg.BookID);
            builder.Entity<BookGenre>().HasOne(bg => bg.Genre).WithMany(g => g.BookGenres).HasForeignKey(bg => bg.GenreID);
            #endregion

            #region Table Order Details
            builder.Entity<OrderDetail>().HasKey(od => new { od.OrderID, od.BookID });
            builder.Entity<OrderDetail>().HasOne(od => od.Order).WithMany(o => o.OrderDetails).HasForeignKey(od => od.OrderID);
            builder.Entity<OrderDetail>().HasOne(od => od.Book).WithMany(b => b.OrderDetails).HasForeignKey(od => od.BookID);
            #endregion

            #region Table Favorites
            builder.Entity<Favorite>().HasKey(f => new { f.BookID, f.CustomersID });
            builder.Entity<Favorite>().HasOne(f => f.Book).WithMany(b => b.Favorites).HasForeignKey(f => f.BookID);
            builder.Entity<Favorite>().HasOne(f => f.Customer).WithMany(c => c.Favorites).HasForeignKey(f => f.CustomersID);
            #endregion

            // Add Default Value
            #region Table Ratings
            builder.Entity<Rating>().Property(r => r.RatingCount).HasDefaultValue(1);
            #endregion

            #region Table Orders
            builder.Entity<Order>().Property(o => o.Status).HasDefaultValue(StatusType.WAITING_FOR_PROGRESSING);
            #endregion
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Series> Series { get; set; }
    }
}
