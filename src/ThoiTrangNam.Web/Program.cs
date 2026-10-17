using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ThoiTrangNam.Web.Data;
using ThoiTrangNam.Web.Middleware;
using ThoiTrangNam.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Các nền tảng host (Render, Railway...) cấp cổng qua biến môi trường PORT.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddControllersWithViews(options =>
{
    // Không tự coi mọi thuộc tính string không-null là bắt buộc; ta khai báo [Required] rõ ràng.
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    // Mọi POST đều phải có token chống giả mạo yêu cầu (CSRF).
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/AccessDenied";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        o.Cookie.Name = "TTN.Auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// Kết nối CSDL (singleton: chỉ giữ chuỗi kết nối, mỗi lần gọi tạo SqlConnection mới có pooling)
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<DbHealthService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Tự đăng ký: lớp XxxRepository / XxxService có interface IXxxRepository / IXxxService,
// và các lớp IHostedService. Thành viên thêm lớp mới không cần sửa Program.cs.
foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && !t.IsAbstract))
{
    if (type.Name.EndsWith("Repository") || type.Name.EndsWith("Service"))
    {
        var itf = type.GetInterfaces().FirstOrDefault(i => i.Name == "I" + type.Name);
        if (itf != null) builder.Services.AddScoped(itf, type);
    }
    else if (typeof(IHostedService).IsAssignableFrom(type))
    {
        builder.Services.AddSingleton(typeof(IHostedService), type);
    }
}
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
var app = builder.Build();

// Luôn dùng trang lỗi thân thiện (kể cả khi phát triển); chi tiết lỗi nằm trong log.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Hiển thị lỗi chi tiết màu đỏ của .NET
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<DevBypassAuthMiddleware>();
app.UseAuthorization();

app.MapGet("/healthz", () => Results.Text("ok"));
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
