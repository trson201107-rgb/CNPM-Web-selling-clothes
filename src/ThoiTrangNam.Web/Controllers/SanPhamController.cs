using Microsoft.AspNetCore.Mvc;
using ThoiTrangNam.Web.Repositories;

namespace ThoiTrangNam.Web.Controllers;

/// <summary>Trang chi tiết sản phẩm cho khách hàng. Admin/Staff xem được cả sản phẩm đã ẩn.</summary>
public class SanPhamController : Controller
{
    private readonly ISanPhamRepository _repo;
    public SanPhamController(ISanPhamRepository repo) { _repo = repo; }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        bool xemTatCa = User.IsInRole("Admin") || User.IsInRole("Staff");
        var vm = await _repo.LayChiTietAsync(id, chiDangBan: !xemTatCa, ct);
        if (vm == null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }
        vm.LienQuan = await _repo.LaySanPhamLienQuanAsync(id, 4, ct);
        return View(vm);
    }
}
