using Microsoft.AspNetCore.Mvc;
using ThoiTrangNam.Web.Repositories;

namespace ThoiTrangNam.Web.ViewComponents;

/// <summary>Số lượng hàng trong giỏ, hiện trên thanh menu. Lỗi (ví dụ mất kết nối CSDL) thì hiện 0, không làm hỏng trang.</summary>
public class CartBadgeViewComponent : ViewComponent
{
    private readonly ICartRepository _repo;
    public CartBadgeViewComponent(ICartRepository repo) { _repo = repo; }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var v = HttpContext.User.FindFirst("MaTK")?.Value;
        if (!int.TryParse(v, out var maTK)) return View(0);
        try
        {
            return View(await _repo.DemSoLuongAsync(maTK));
        }
        catch
        {
            return View(0);
        }
    }
}
