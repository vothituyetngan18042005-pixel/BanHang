using BanHang.Data;
using BanHang.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//
// ================= DATABASE =================
//

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

//
// ================= MVC =================
//

builder.Services.AddControllersWithViews();

//
// ================= COOKIE AUTH =================
//

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)

    .AddCookie(options =>
    {
        // Trang login
        options.LoginPath = "/Account/Login";

        // Trang bị từ chối quyền
        options.AccessDeniedPath = "/Account/AccessDenied";

        // Thời gian sống cookie
        options.ExpireTimeSpan = TimeSpan.FromDays(30);

        // Tự gia hạn cookie khi còn hoạt động
        options.SlidingExpiration = true;

        // Tên cookie
        options.Cookie.Name = "ZoTreeStore_Auth_Cookie";

        // Chống truy cập JS
        options.Cookie.HttpOnly = true;

        // HTTPS only
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.SameAsRequest;
    });

var app = builder.Build();

//
// ================= ERROR =================
//

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//
// ================= MIDDLEWARE =================
//

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

//
// QUAN TRỌNG
//

app.UseAuthentication();

app.UseAuthorization();

//
// ================= ROUTE =================
//

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

