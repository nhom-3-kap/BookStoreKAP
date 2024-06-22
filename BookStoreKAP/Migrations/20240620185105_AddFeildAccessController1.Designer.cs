﻿// <auto-generated />
using System;
using BookStoreKAP.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BookStoreKAP.Migrations
{
    [DbContext(typeof(BookStoreKAPDBContext))]
    [Migration("20240620185105_AddFeildAccessController1")]
    partial class AddFeildAccessController1
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.20")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BookStoreKAP.Models.Entities.AccessController", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<string>("AreaName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.HasKey("ID");

                    b.ToTable("AccessControllers");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Book", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<string>("Author")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<double>("Discount")
                        .HasColumnType("float");

                    b.Property<string>("Feedback")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Images")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Price")
                        .HasColumnType("float");

                    b.Property<int>("PublicationYear")
                        .HasColumnType("int");

                    b.Property<string>("Publisher")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<Guid?>("RatingID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("SeriesID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Synopsis")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("TagID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Thumbnail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<int>("ViewCount")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("RatingID");

                    b.HasIndex("SeriesID");

                    b.ToTable("Books");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.BookGenre", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<Guid>("BookID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("GenreID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ID");

                    b.HasAlternateKey("GenreID", "BookID");

                    b.HasIndex("BookID");

                    b.ToTable("BookGenres");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Domain", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<Guid>("AccessControllerID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<Guid>("RoleID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.HasKey("ID");

                    b.HasIndex("AccessControllerID");

                    b.HasIndex("RoleID");

                    b.ToTable("Domains");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Favorite", b =>
                {
                    b.Property<Guid>("BookID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CustomersID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("BookID", "CustomersID");

                    b.HasIndex("CustomersID");

                    b.ToTable("Favorites");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Genre", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.HasKey("ID");

                    b.ToTable("Genres");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Order", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<Guid>("CustomerID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("OrderDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int>("SubTotal")
                        .HasColumnType("int");

                    b.Property<double>("Total")
                        .HasColumnType("float");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.HasKey("ID");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.OrderDetail", b =>
                {
                    b.Property<Guid>("OrderID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("BookID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Price")
                        .HasColumnType("float");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.HasKey("OrderID", "BookID");

                    b.HasIndex("BookID");

                    b.ToTable("OrderDetails");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Rating", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<Guid>("BookID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<string>("Detail")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("RatingCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.Property<Guid>("ReviewerID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.HasKey("ID");

                    b.ToTable("Ratings");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(max)")
                        .HasDefaultValueSql("NEWID()");

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

                    b.ToTable("Roles", (string)null);
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.RoleClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("RoleClaims", (string)null);
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Sale", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<Guid>("BookID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<double>("Price")
                        .HasColumnType("float");

                    b.Property<DateTime>("SaleBegin")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("SaleEnd")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.HasKey("ID");

                    b.HasIndex("BookID");

                    b.ToTable("Sales");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Series", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<int>("Volumns")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.ToTable("Series");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Tag", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<Guid?>("BookID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<string>("Thumbnail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.HasKey("ID");

                    b.HasIndex("BookID");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("Avatar")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("BOD")
                        .HasColumnType("datetime2");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FirstName")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("LastName")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

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

                    b.Property<Guid?>("OrderID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<Guid?>("RatingID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShipAddress")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

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

                    b.HasIndex("OrderID");

                    b.HasIndex("RatingID");

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.UserClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserClaims", (string)null);
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.UserLogin", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("UserLogins", (string)null);
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.UserRole", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("RoleId1")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.HasIndex("RoleId1");

                    b.HasIndex("UserId");

                    b.ToTable("UserRoles", (string)null);
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.UserToken", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.HasIndex("UserId");

                    b.ToTable("UserTokens", (string)null);
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Book", b =>
                {
                    b.HasOne("BookStoreKAP.Models.Entities.Rating", null)
                        .WithMany("Books")
                        .HasForeignKey("RatingID");

                    b.HasOne("BookStoreKAP.Models.Entities.Series", "Series")
                        .WithMany("Books")
                        .HasForeignKey("SeriesID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Series");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.BookGenre", b =>
                {
                    b.HasOne("BookStoreKAP.Models.Entities.Book", "Book")
                        .WithMany("BookGenres")
                        .HasForeignKey("BookID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BookStoreKAP.Models.Entities.Genre", "Genre")
                        .WithMany("BookGenres")
                        .HasForeignKey("GenreID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Book");

                    b.Navigation("Genre");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Domain", b =>
                {
                    b.HasOne("BookStoreKAP.Models.Entities.AccessController", "AccessController")
                        .WithMany("Domains")
                        .HasForeignKey("AccessControllerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BookStoreKAP.Models.Entities.Role", "Role")
                        .WithMany("Domains")
                        .HasForeignKey("RoleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AccessController");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Favorite", b =>
                {
                    b.HasOne("BookStoreKAP.Models.Entities.Book", "Book")
                        .WithMany("Favorites")
                        .HasForeignKey("BookID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BookStoreKAP.Models.Entities.User", "Customer")
                        .WithMany("Favorites")
                        .HasForeignKey("CustomersID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Book");

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.OrderDetail", b =>
                {
                    b.HasOne("BookStoreKAP.Models.Entities.Book", "Book")
                        .WithMany("OrderDetails")
                        .HasForeignKey("BookID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BookStoreKAP.Models.Entities.Order", "Order")
                        .WithMany("OrderDetails")
                        .HasForeignKey("OrderID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Book");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.RoleClaim", b =>
                {
                    b.HasOne("BookStoreKAP.Models.Entities.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Sale", b =>
                {
                    b.HasOne("BookStoreKAP.Models.Entities.Book", "Book")
                        .WithMany("Sales")
                        .HasForeignKey("BookID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Book");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Tag", b =>
                {
                    b.HasOne("BookStoreKAP.Models.Entities.Book", null)
                        .WithMany("Tags")
                        .HasForeignKey("BookID");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.User", b =>
                {
                    b.HasOne("BookStoreKAP.Models.Entities.Order", null)
                        .WithMany("Customers")
                        .HasForeignKey("OrderID");

                    b.HasOne("BookStoreKAP.Models.Entities.Rating", null)
                        .WithMany("Reviewers")
                        .HasForeignKey("RatingID");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.UserClaim", b =>
                {
                    b.HasOne("BookStoreKAP.Models.Entities.User", "User")
                        .WithMany("UserClaims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.UserLogin", b =>
                {
                    b.HasOne("BookStoreKAP.Models.Entities.User", "User")
                        .WithMany("UserLogins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.UserRole", b =>
                {
                    b.HasOne("BookStoreKAP.Models.Entities.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BookStoreKAP.Models.Entities.Role", null)
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId1");

                    b.HasOne("BookStoreKAP.Models.Entities.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.UserToken", b =>
                {
                    b.HasOne("BookStoreKAP.Models.Entities.User", "User")
                        .WithMany("UserTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.AccessController", b =>
                {
                    b.Navigation("Domains");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Book", b =>
                {
                    b.Navigation("BookGenres");

                    b.Navigation("Favorites");

                    b.Navigation("OrderDetails");

                    b.Navigation("Sales");

                    b.Navigation("Tags");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Genre", b =>
                {
                    b.Navigation("BookGenres");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Order", b =>
                {
                    b.Navigation("Customers");

                    b.Navigation("OrderDetails");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Rating", b =>
                {
                    b.Navigation("Books");

                    b.Navigation("Reviewers");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Role", b =>
                {
                    b.Navigation("Domains");

                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.Series", b =>
                {
                    b.Navigation("Books");
                });

            modelBuilder.Entity("BookStoreKAP.Models.Entities.User", b =>
                {
                    b.Navigation("Favorites");

                    b.Navigation("UserClaims");

                    b.Navigation("UserLogins");

                    b.Navigation("UserRoles");

                    b.Navigation("UserTokens");
                });
#pragma warning restore 612, 618
        }
    }
}