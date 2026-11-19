using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_ageyavich.Data;
using wt_lab7_7postgres_efcore_ageyavich.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(); 

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = builder.Configuration["DATABASE_URL"];

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    databaseUrl = databaseUrl.Trim().Trim('"', '\''); 
    
    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var port = uri.Port > 0 ? uri.Port : 5432; // Защита от ошибки с портом -1

        connectionString = $"Host={uri.Host};" +
                           $"Port={port};" +
                           $"Database={uri.AbsolutePath.TrimStart('/')};" +
                           $"Username={Uri.UnescapeDataString(userInfo[0])};" +
                           $"Password={Uri.UnescapeDataString(userInfo[1])};" +
                           "SSL Mode=Disable";
    }
    catch (Exception ex)
    {
        // Если URL кривой, мы увидим это в логах Render и приложение не упадёт молча
        Console.WriteLine($"[ОШИБКА РЕНДЕРА] Неверный формат DATABASE_URL. Значение: '{databaseUrl}'");
        Console.WriteLine($"[ОШИБКА РЕНДЕРА] Детали: {ex.Message}");
    }
}


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// GET /
app.MapGet("/", () => "Minimal API работает с PostgreSQL!");

// GET /api/info
app.MapGet("/api/info", () => 
{
    return new
    {
        Application = "Web Technologies",
        Technology = "ASP.NET Core Minimal API + EF Core + PostgreSQL",
        Version = "1.0"
    };
});


// 1. GET /api/products (получить все)
app.MapGet("/api/products", async (AppDbContext db) => 
{
    var products = await db.Products.ToListAsync();
    return Results.Ok(products);
});

// 2. GET /api/products/{id} (получить один)
app.MapGet("/api/products/{id}", async (int id, AppDbContext db) => 
{
    var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
    if (product == null) return Results.NotFound(); // 404
    return Results.Ok(product); // 200
});

// 3. POST /api/products (создать)
app.MapPost("/api/products", async (Product product, AppDbContext db) => 
{
    db.Products.Add(product);
    await db.SaveChangesAsync(); // Сохраняем в базу
    return Results.Created($"/api/products/{product.Id}", product); // 201
});

// 4. PUT /api/products/{id} (обновить)
app.MapPut("/api/products/{id}", async (int id, Product updatedProduct, AppDbContext db) => 
{
    var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
    if (product == null) return Results.NotFound();


    product.Name = updatedProduct.Name;
    product.Category = updatedProduct.Category;
    product.Price = updatedProduct.Price;
    product.IsAvailable = updatedProduct.IsAvailable;

    await db.SaveChangesAsync();
    return Results.Ok(product); // 200
});

// 5. DELETE /api/products/{id} (удалить)
app.MapDelete("/api/products/{id}", async (int id, AppDbContext db) => 
{
    var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
    if (product == null) return Results.NotFound();

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.NoContent(); // 204
});


// LINQ 1: Поиск по названию (Шаг 15)
app.MapGet("/api/products/search", async (string? search, AppDbContext db) => 
{
    var query = db.Products.AsQueryable();
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(x => x.Name.ToLower().Contains(search.ToLower()));
    }
    var products = await query.ToListAsync();
    return Results.Ok(products);
});

// LINQ 2: Фильтрация
app.MapGet("/api/products/expensive", async (AppDbContext db) => 
{
    var products = await db.Products.Where(x => x.Price > 100).ToListAsync();
    return Results.Ok(products);
});

// LINQ 3: Сортировка по цене (по возрастанию)
app.MapGet("/api/products/by-price", async (AppDbContext db) => 
{
    var products = await db.Products.OrderBy(x => x.Price).ToListAsync();
    return Results.Ok(products);
});

// LINQ 4: Выборка с условием + сортировка (доступные по алфавиту)
app.MapGet("/api/products/available", async (AppDbContext db) => 
{
    var products = await db.Products
        .Where(x => x.IsAvailable)
        .OrderBy(x => x.Name)
        .ToListAsync();
    return Results.Ok(products);
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");