using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThoiTrangNam.Web.Models;
using ThoiTrangNam.Web.Repositories;

namespace ThoiTrangNam.Web.Controllers;

/// <summary>Giỏ hàng cơ bản: xem, thêm, sửa số lượng, xóa. Thanh toán thuộc Sprint 3.</summary>
[Authorize]
public class CartController : Controller
{
    private readonly ICartRepository _cart;
    public CartController(ICartRepository cart) { _cart = cart; }

    private int MaTK => int.TryParse(User.FindFirst("MaTK")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _cart.LayGioHangAsync(MaTK, ct));

    [HttpPost]
    public async Task<IActionResult> Add(int maBT, int soLuong = 1, string? returnUrl = null, CancellationToken ct = default)
    {
        var kq = await _cart.ThemAsync(MaTK, maBT, soLuong, ct);
        if (kq == CartResult.ThanhCong) TempData["Msg"] = "Đã thêm vào giỏ hàng.";
        else TempData["Err"] = ThongBaoLoi(kq);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Update(int maBT, int soLuong, CancellationToken ct)
    {
        var kq = await _cart.CapNhatSoLuongAsync(MaTK, maBT, soLuong, ct);
        if (kq != CartResult.ThanhCong) TempData["Err"] = ThongBaoLoi(kq);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int maBT, CancellationToken ct)
    {
        await _cart.XoaAsync(MaTK, maBT, ct);
        TempData["Msg"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
        return RedirectToAction(nameof(Index));
    }

    private static string ThongBaoLoi(CartResult kq) => kq switch
    {
        CartResult.HetHang => "Biến thể này đã hết hàng.",
        CartResult.VuotTon => "Số lượng vượt quá tồn kho hiện có.",
        _ => "Không tìm thấy sản phẩm hoặc sản phẩm đã ngừng bán.",
    };
}
