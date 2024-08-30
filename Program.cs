using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace En;
//Создайте базу данных Shop.Определите класс Product, заполните начальными данными.
//Используя миграции, выполните следующие действия:
	
//1) Добавьте новую сущность Order, которая будет представлять заказ в системе.
//2) Расширьте существующую сущность Product, чтобы можно было связать продукты с заказами.
	
//3) Создайте сервис, отвечающий за обработку заказов.В этом сервисе реализуйте логику добавления, удаления и просмотра заказов.Сервисом может выступать “Библиотека классов” или отдельный “Класс”.
	
//4) Протестируйте взаимодействие с системой заказов, убедитесь, что заказы правильно отображаются и обновляются.


class Program
{
    static void Main()
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            //db.Database.EnsureDeleted();
            //db.Database.EnsureCreated();
            //db.products.AddRange(new List<Product>
            //{
            //    new Product { Name = "Хлеб", Price = 25.00m },
            //    new Product { Name = "Яйца", Price = 60.00m },
            //    new Product { Name = "Сыр", Price = 120.00m }
            //});
            //db.SaveChanges();
            var orderService = new OrderService(db);

            // Добавление нового заказа
            //var newOrder = new Order
            //{
            //    OrderNumber = "ORD004",
            //    OrderDate = DateTime.Now
            //};
            //var productIds = new List<int> { 1, 3 };
            //orderService.AddOrder(newOrder, productIds);

            //Получение всех заказов
           var orders = orderService.GetAllOrders();
            foreach (var order in orders)
            {
                Console.WriteLine($"Заказ ID: {order.Id}, Номер: {order.OrderNumber}, Дата: {order.OrderDate}");
                foreach (var orderProduct in order.OrderProducts)
                {
                    var product = orderProduct.Product;
                    Console.WriteLine($"- Продукт: {product.Name}, Цена: {product.Price}");
                }
            }

            //// Удаление заказа по Id
            //int orderIdToDelete = 1; // Предположим, что заказ с Id 1 существует
            //orderService.DeleteOrder(orderIdToDelete);
            //Console.WriteLine($"Заказ с ID {orderIdToDelete} удален.");

        }

    }
   
}

public class Product
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int OrderId { get; set; }
    public List<OrderProduct> OrderProducts { get; set; }

}
public class OrderProduct
{
    public int OrderId { get; set; }
    public Order Order { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }
}
public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }

    public List<OrderProduct> OrderProducts { get; set; }
}
public class OrderService
{
    private readonly ApplicationContext _db;

    public OrderService(ApplicationContext db)
    {
        _db = db;
    }
    public void AddOrder(Order order, List<int> productIds)
    {
        order.OrderProducts = productIds.Select(id => new OrderProduct { ProductId = id }).ToList();
        _db.Orders.Add(order);
        _db.SaveChanges();
    }
    public void DeleteOrder(int orderId)
    {
        var order = _db.Orders.Include(o => o.OrderProducts)
                                   .FirstOrDefault(o => o.Id == orderId);

        if (order != null)
        {
            _db.Orders.Remove(order);
            _db.SaveChanges();
        }
        else
        {
            Console.WriteLine($"Заказ с ID {orderId} не найден.");
        }
    }
    public Order GetOrderById(int orderId)
    {
        return _db.Orders.Include(o => o.OrderProducts)
                              .ThenInclude(op => op.Product)
                              .FirstOrDefault(o => o.Id == orderId);
    }
    public List<Order> GetAllOrders()
    {
        return _db.Orders.Include(o => o.OrderProducts)
                              .ThenInclude(op => op.Product)
                              .ToList();
    }
}
public class ApplicationContext : DbContext
{

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=DESKTOP-C317JNM;Database=shop;Trusted_Connection=True;TrustServerCertificate=True;");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderProduct>()
            .HasKey(op => new { op.OrderId, op.ProductId });

        modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Order)
            .WithMany(o => o.OrderProducts)
            .HasForeignKey(op => op.OrderId);

        modelBuilder.Entity<OrderProduct>()
            .HasOne(op => op.Product)
            .WithMany(p => p.OrderProducts)
            .HasForeignKey(op => op.ProductId);

        // Начальные данные для Product
        //modelBuilder.Entity<Product>().HasData(
        //    new Product { Name = "Хлеб", Price = 25.00m },
        //    new Product { Name = "Яйца", Price = 60.00m },
        //    new Product { Name = "Сыр", Price = 120.00m }
        //);
    }
}
