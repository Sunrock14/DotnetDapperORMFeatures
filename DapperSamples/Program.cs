using DapperSamples.Data;
using DapperSamples.Data.Repositories;
using DapperSamples.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Connection Factory
builder.Services.AddSingleton<IDatabaseConnectionFactory>(sp => 
    new SqlConnectionFactory(sp.GetRequiredService<IConfiguration>()));

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();

// Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Veritabanını oluşturma ve başlatma
try
{
    var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(connectionString))
    {
        DbInitializer.Initialize(connectionString);
        Console.WriteLine("Veritabanı başarıyla oluşturuldu ve başlatıldı.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Veritabanı oluşturulurken hata: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
