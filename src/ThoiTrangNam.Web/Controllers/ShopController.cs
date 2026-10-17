using Microsoft.AspNetCore.Mvc;
using ThoiTrangNam.Web.Models;
using ThoiTrangNam.Web.Repositories;

namespace ThoiTrangNam.Web.Controllers;

/// <summary>Trang cửa hàng: tìm kiếm, lọc, sắp xếp, phân trang.</summary>
public class ShopController : Controller
{
    private const int SoSanPhamMoiTrang = 12;

    private readonly ISanPhamQueryRepository _query;
    private readonly IDanhMucRepository _danhMuc;

    public ShopController(ISanPhamQueryRepository query, IDanhMucRepository danhMuc)
    {
        _query = query;
        _danhMuc = danhMuc;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ProductSearchCriteria criteria, CancellationToken ct)
    {
        criteria.Page = Math.Max(1, criteria.Page);
        criteria.PageSize = SoSanPhamMoiTrang;
        if (criteria.GiaTu < 0) criteria.GiaTu = null;
        if (criteria.GiaDen < 0) criteria.GiaDen = null;
        if (criteria.GiaTu.HasValue && criteria.GiaDen.HasValue && criteria.GiaTu > criteria.GiaDen)
            (criteria.GiaTu, criteria.GiaDen) = (criteria.GiaDen, criteria.GiaTu);

        var vm = new ShopIndexViewModel
        {
            Criteria = criteria,
            Result = await _query.TimKiemAsync(criteria, ct),
            DanhMucs = await _danhMuc.LayHienThiAsync(ct),
        };
        criteria.Page = vm.Result.Page;
        return View(vm);
    }
}
