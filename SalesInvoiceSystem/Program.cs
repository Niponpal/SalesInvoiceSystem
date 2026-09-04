
using Microsoft.EntityFrameworkCore;
using SalesInvoiceSystem.Data;
using SalesInvoiceSystem.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// EF Core DbContext
builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer( builder.Configuration.GetConnectionString("DefaultConnection")));

// Dapper DbConnectionFactory
builder.Services.AddScoped<DbConnectionFactory>();

// Product Repository
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=sale}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

