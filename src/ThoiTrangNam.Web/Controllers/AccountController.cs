using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ThoiTrangNam.Web.Models;
using ThoiTrangNam.Web.Services;

namespace ThoiTrangNam.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _auth;
    public AccountController(IAuthService auth) { _auth = auth; }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var kq = await _auth.DangNhapAsync(vm.Email, vm.MatKhau, ct);
        if (!kq.ThanhCong || kq.TaiKhoan == null)
        {
            ModelState.AddModelError("", kq.Loi ?? "Đăng nhập không thành công.");
            vm.MatKhau = "";
            return View(vm);
        }

        await DangNhapCookieAsync(kq.TaiKhoan, vm.GhiNho);
        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl)) return LocalRedirect(vm.ReturnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);
        try
        {
            await _auth.DangKyAsync(vm, ct);
        }
        catch (NghiepVuException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(vm);
        }
        TempData["Msg"] = "Đăng ký thành công. Hãy đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private async Task DangNhapCookieAsync(TaiKhoan tk, bool ghiNho)
    {
        var claims = new List<Claim>
        {
            new("MaTK", tk.MaTK.ToString()),
            new(ClaimTypes.Name, tk.HoTen),
            new(ClaimTypes.Email, tk.Email),
            new(ClaimTypes.Role, tk.TenVaiTro),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = ghiNho });
    }
}
