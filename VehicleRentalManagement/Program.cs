using Microsoft.AspNetCore.Authentication.Cookies;
using VehicleRentalManagement.DataAccess; // DatabaseConnection i�in

var builder = WebApplication.CreateBuilder(args);

// MVC servisleri ekle
builder.Services.AddControllersWithViews();

// DatabaseConnection servisini ekle
builder.Services.AddSingleton<DatabaseConnection>();

// Oturum (Session) deste�i ekle (iste�e ba�l�)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cookie tabanl� kimlik do�rulama
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";        // Oturum a�ma sayfas�
        options.LogoutPath = "/Account/Logout";      // Oturum kapatma sayfas�
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisiz eri�im y�nlendirmesi
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Hata y�netimi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Kimlik do�rulama ve yetkilendirme middleware�leri
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
