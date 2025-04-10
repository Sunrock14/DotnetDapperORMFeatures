using Dapper;
using Microsoft.Data.SqlClient;

namespace DapperSamples.Data;

public static class DbInitializer
{
    public static void Initialize(string connectionString)
    {
        var masterConnectionString = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        }.ConnectionString;
        
        using var masterConnection = new SqlConnection(masterConnectionString);
        masterConnection.Open();
        
        // Veritabanının zaten var olup olmadığını kontrol ediyoruz
        var dbName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        var dbExists = masterConnection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM sys.databases WHERE name = @name", 
            new { name = dbName }) > 0;
        
        if (!dbExists)
        {
            // Veritabanı yoksa oluşturuyoruz
            masterConnection.Execute($"CREATE DATABASE [{dbName}]");
        }
        
        // Ana veritabanına bağlanıyoruz
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        
        // Tabloları oluşturuyoruz
        CreateTables(connection);
        
        // Örnek veri ekliyoruz
        if (connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Categories") == 0)
        {
            SeedData(connection);
        }
    }
    
    private static void CreateTables(SqlConnection connection)
    {
        // Categories tablosu
        connection.Execute(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
            BEGIN
                CREATE TABLE Categories (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Name NVARCHAR(100) NOT NULL,
                    Description NVARCHAR(500),
                    IsActive BIT DEFAULT 1
                );
            END");
        
        // Products tablosu
        connection.Execute(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
            BEGIN
                CREATE TABLE Products (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Name NVARCHAR(100) NOT NULL,
                    Price DECIMAL(18,2) NOT NULL,
                    Description NVARCHAR(500),
                    CategoryId INT NOT NULL,
                    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
                    IsActive BIT DEFAULT 1,
                    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
                );
            END");
        
        // Orders tablosu
        connection.Execute(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
            BEGIN
                CREATE TABLE Orders (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    CustomerName NVARCHAR(100) NOT NULL,
                    ContactEmail NVARCHAR(100),
                    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
                    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
                    Status INT NOT NULL DEFAULT 0
                );
            END");
        
        // OrderItems tablosu
        connection.Execute(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderItems')
            BEGIN
                CREATE TABLE OrderItems (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    OrderId INT NOT NULL,
                    ProductId INT NOT NULL,
                    Quantity INT NOT NULL,
                    UnitPrice DECIMAL(18,2) NOT NULL,
                    TotalPrice DECIMAL(18,2) NOT NULL,
                    FOREIGN KEY (OrderId) REFERENCES Orders(Id),
                    FOREIGN KEY (ProductId) REFERENCES Products(Id)
                );
            END");
        
        // CreateOrder Stored Procedure
        connection.Execute(@"
            IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'CreateOrder')
            BEGIN
                EXEC('
                    CREATE PROCEDURE CreateOrder
                        @CustomerName NVARCHAR(100),
                        @ContactEmail NVARCHAR(100),
                        @TotalAmount DECIMAL(18,2),
                        @OrderDate DATETIME,
                        @Status INT,
                        @OrderId INT OUTPUT
                    AS
                    BEGIN
                        INSERT INTO Orders (CustomerName, ContactEmail, TotalAmount, OrderDate, Status)
                        VALUES (@CustomerName, @ContactEmail, @TotalAmount, @OrderDate, @Status);
                        
                        SET @OrderId = SCOPE_IDENTITY();
                        
                        RETURN @OrderId;
                    END
                ')
            END");
    }
    
    private static void SeedData(SqlConnection connection)
    {
        // Kategoriler ekleniyor
        var categories = new[]
        {
            new { Name = "Elektronik", Description = "Elektronik ürünler" },
            new { Name = "Beyaz Eşya", Description = "Beyaz eşya ürünleri" },
            new { Name = "Mobilya", Description = "Ev ve ofis mobilyaları" }
        };
        
        foreach (var category in categories)
        {
            connection.Execute(@"
                INSERT INTO Categories (Name, Description, IsActive)
                VALUES (@Name, @Description, 1)", category);
        }
        
        // Ürünler ekleniyor
        var productSql = @"
            INSERT INTO Products (Name, Price, Description, CategoryId, IsActive)
            VALUES (@Name, @Price, @Description, @CategoryId, 1)";
        
        var electronics = connection.QueryFirst<int>("SELECT Id FROM Categories WHERE Name = 'Elektronik'");
        var homeApp = connection.QueryFirst<int>("SELECT Id FROM Categories WHERE Name = 'Beyaz Eşya'");
        var furniture = connection.QueryFirst<int>("SELECT Id FROM Categories WHERE Name = 'Mobilya'");
        
        var products = new[]
        {
            new { Name = "Laptop", Price = 9999.99m, Description = "Yüksek performanslı", CategoryId = electronics },
            new { Name = "Akıllı Telefon", Price = 5999.99m, Description = "Son model", CategoryId = electronics },
            new { Name = "Tablet", Price = 2999.99m, Description = "10 inç ekran", CategoryId = electronics },
            new { Name = "Buzdolabı", Price = 7999.99m, Description = "No-frost", CategoryId = homeApp },
            new { Name = "Çamaşır Makinesi", Price = 4999.99m, Description = "8 kg", CategoryId = homeApp },
            new { Name = "Koltuk Takımı", Price = 8999.99m, Description = "3+2+1", CategoryId = furniture },
            new { Name = "Yemek Masası", Price = 2499.99m, Description = "6 kişilik", CategoryId = furniture }
        };
        
        foreach (var product in products)
        {
            connection.Execute(productSql, product);
        }
        
        // Örnek sipariş ekleniyor
        var orderId = connection.QuerySingle<int>(@"
            INSERT INTO Orders (CustomerName, ContactEmail, TotalAmount, Status)
            VALUES ('Ali Veli', 'ali@example.com', 0, 0);
            SELECT CAST(SCOPE_IDENTITY() as int)");
        
        // Sipariş öğeleri ekleniyor
        var laptop = connection.QueryFirst<int>("SELECT Id FROM Products WHERE Name = 'Laptop'");
        var phone = connection.QueryFirst<int>("SELECT Id FROM Products WHERE Name = 'Akıllı Telefon'");
        
        var orderItems = new[]
        {
            new { OrderId = orderId, ProductId = laptop, Quantity = 1, UnitPrice = 9999.99m, TotalPrice = 9999.99m },
            new { OrderId = orderId, ProductId = phone, Quantity = 2, UnitPrice = 5999.99m, TotalPrice = 11999.98m }
        };
        
        foreach (var item in orderItems)
        {
            connection.Execute(@"
                INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, TotalPrice)
                VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @TotalPrice)", item);
        }
        
        // Siparişin toplam tutarını güncelliyoruz
        connection.Execute(@"
            UPDATE Orders
            SET TotalAmount = (SELECT SUM(TotalPrice) FROM OrderItems WHERE OrderId = @OrderId)
            WHERE Id = @OrderId", new { OrderId = orderId });
    }
}