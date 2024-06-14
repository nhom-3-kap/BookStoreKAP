
using Owin;
using BookStoreKAP.Database;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Owin.Security.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<BookStoreKAPDBContext>(
    options =>
    {
        options.UseSqlServer(connectionString);
    }
);

builder.Services.AddIdentity<User, IdentityRole>(
        options =>
        {
            options.Password.RequiredUniqueChars = 0;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
        }
    )
    .AddEntityFrameworkStores<BookStoreKAPDBContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie().AddGoogle(options =>
{
    options.ClientId = configuration["Authentication:Google:ClientId"] ?? "702342811459-cbagsaprmfi0s687j6hjgllqfsi5m7rc.apps.googleusercontent.com";
    options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? "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";
    options.CallbackPath = "/signin-google";
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = new PathString("/Login");
    options.LogoutPath = "/Logout";
    options.AccessDeniedPath = "/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddMvc();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/Admin") && context.Request.Path == "/Admin")
    {
        context.Response.Redirect("/Admin/Home");
        return;
    }
    await next();
});

app.MapControllerRoute(
    name: "MyArea",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
