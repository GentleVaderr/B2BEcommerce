using Business.Abstract;
using Business.Concrete;
using DataAccess.Abstract;
using DataAccess.Concrete;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Dependincy Injection
builder.Services.AddScoped<IOrderService, OrderManager>();
builder.Services.AddScoped<IProductService, ProductManager>();
builder.Services.AddScoped<IUserService, UserManager>();

builder.Services.AddScoped<IOrderDal, EfOrderDal>();
builder.Services.AddScoped<IProductDal, EfProductDal>();
builder.Services.AddScoped<IUserDal, EfUserDal>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
