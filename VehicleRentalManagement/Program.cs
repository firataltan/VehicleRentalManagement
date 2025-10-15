using Microsoft.AspNetCore.Authentication.Cookies;
using VehicleRentalManagement.DataAccess; // DatabaseConnection için

var builder = WebApplication.CreateBuilder(args);

// MVC servisleri ekle
builder.Services.AddControllersWithViews();

// DatabaseConnection servisini ekle
builder.Services.AddSingleton<DatabaseConnection>();

// Oturum (Session) desteði ekle (isteðe baðlý)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cookie tabanlý kimlik doðrulama
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";        // Oturum açma sayfasý
        options.LogoutPath = "/Account/Logout";      // Oturum kapatma sayfasý
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisiz eriþim yönlendirmesi
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Hata yönetimi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Kimlik doðrulama ve yetkilendirme middleware’leri
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
