using Microsoft.AspNetCore.Authentication.Cookies;
using VehicleRentalManagement.DataAccess; // DatabaseConnection i�in

using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Culture ayarları - Türkçe locale için
var cultureInfo = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// MVC servisleri ekle
builder.Services.AddControllersWithViews(options =>
{
    // Model binding için culture ayarları
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
        _ => "Bu alan gereklidir.");
});

// Decimal model binder ekle
builder.Services.AddMvc(options =>
{
    options.ModelBinderProviders.Insert(0, new VehicleRentalManagement.Models.DecimalModelBinderProvider());
});

// Request localization ayarları
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("tr-TR") };
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("tr-TR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

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

// Request localization middleware
app.UseRequestLocalization();

// Kimlik do�rulama ve yetkilendirme middleware�leri
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
