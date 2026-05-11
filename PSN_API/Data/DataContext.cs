using Microsoft.EntityFrameworkCore;
using PetrolStationNetwork.Data;
using PSN_API.Models;

namespace PSN_API.Data
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users  { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<DeliveryItem> DeliveryItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<WarehouseItem> WarehouseItems { get; set; }
        public DbSet<ShopItem> ShopItems { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(Config.connection, Config.version);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");

            // Настрока связи один-к-одному между User и Supplier
            modelBuilder.Entity<Supplier>()
                .ToTable("Suppliers")
                .HasKey(s => s.user_id);

            modelBuilder.Entity<Supplier>()
                .HasOne(s => s.User)
                .WithOne(u => u.Supplier)
                .HasForeignKey<Supplier>(s => s.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            // Настрока связи один-к-одному между User и Staff
            modelBuilder.Entity<Staff>()
                .ToTable("Staff")
                .HasKey(s => s.user_id);

            modelBuilder.Entity<Staff>()
                .HasOne(s => s.User)
                .WithOne(u => u.Staff)
                .HasForeignKey<Staff>(s => s.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            // Конфигурация для свойства Role в Staff
            modelBuilder.Entity<Staff>()
                .Property(s => s.Role)
                .HasConversion<string>()
                .HasColumnName("role");

            // Настройка связи Delivery И DeliveryItem для получения серийного номера поставки на основе id
            modelBuilder.Entity<DeliveryItem>()
                .HasOne(di => di.Delivery)
                .WithMany()
                .HasForeignKey(di => di.Delivery_id);
            // Настройка связи Product И DeliveryItem для получения наименования продукта на основе id
            modelBuilder.Entity<DeliveryItem>()
                .HasOne(di => di.Product)
                .WithMany()
                .HasForeignKey(di => di.Product_id);

            // Настройка связи Product и WarehouseItem для получения наименования продукта на основе id
            modelBuilder.Entity<WarehouseItem>()
                .HasOne(wi => wi.Product)
                .WithMany()
                .HasForeignKey(wi => wi.Product_id);

            // Настройка связи WarehouseItem и ShopItem для получения позиции поставки на основе id
            modelBuilder.Entity<ShopItem>()
                .HasOne(si => si.WarehouseItem)
                .WithMany()
                .HasForeignKey(si => si.Warehouse_item_id);

            base.OnModelCreating(modelBuilder);
        }
    }
}
