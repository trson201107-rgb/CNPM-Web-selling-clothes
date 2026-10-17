using Microsoft.AspNetCore.Mvc;
using ThoiTrangNam.Web.Models;
using ThoiTrangNam.Web.Repositories;

namespace ThoiTrangNam.Web.Controllers;

public class HomeController : Controller
{
    private readonly ISanPhamQueryRepository _query;
    private readonly IDanhMucRepository _danhMuc;

    public HomeController(ISanPhamQueryRepository query, IDanhMucRepository danhMuc)
    {
        _query = query;
        _danhMuc = danhMuc;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = new HomeViewModel
        {
            Hot = await _query.LayNoiBatAsync(8, ct),
            New = await _query.LayMoiAsync(8, ct),
            DanhMucs = await _danhMuc.LayHienThiAsync(ct),
        };
        return View(vm);
    }

    // Trang lỗi chung: không hiển thị stack trace cho người dùng.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
