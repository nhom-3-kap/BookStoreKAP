﻿
using Owin;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Owin.Security.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using BookStoreKAP.Data;
using BookStoreKAP.Middleware;
using BookStoreKAP.Areas.Admin.Controllers;

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

builder.Services.AddIdentity<User, Role>(
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
    options.CallbackPath = new PathString("/signin-google");
}).AddFacebook(options =>
{
    options.AppId = configuration["Authentication:Facebook:AppId"] ?? "1453111265578357";
    options.AppSecret = configuration["Authentication:Facebook:AppSecret"] ?? "f187eed2b0b76a2c49049c43eb5d80e8";
    options.CallbackPath = new PathString("/signin-facebook");
    options.Scope.Add("email");
    options.Fields.Add("email");
});


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = new PathString("/Auth/Login");
    options.LogoutPath = new PathString("/Auth/Logout");
    options.AccessDeniedPath = new PathString("/Auth/AccessDenied");
});

builder.Services.AddControllersWithViews();
builder.Services.AddMvc();

#region DI
builder.Services.AddScoped<RoleManager<Role>>();
builder.Services.AddScoped<UserManager<User>>();
builder.Services.AddScoped<BookStoreKAPDBContext>();
builder.Services.AddScoped<RolesController>();
#endregion

var app = builder.Build();

app.UseStatusCodePagesWithRedirects("/Home/Error?statusCode={0}");

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

// Đăng ký middleware kiểm tra quyền truy cập
app.UseMiddleware<AccessControlMiddleware>();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/Admin") && context.Request.Path == "/Admin")
    {
        context.Response.Redirect("/Admin/HomeAdmin");
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
using (var scope = app.Services.CreateScope())
{
    var rolesController = scope.ServiceProvider.GetRequiredService<RolesController>();
    rolesController.RefreshListController();
    rolesController.RefreshListPermissions();
}
app.Run();
