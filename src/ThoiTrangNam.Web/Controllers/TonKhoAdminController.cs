using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThoiTrangNam.Web.Repositories;

namespace ThoiTrangNam.Web.Controllers;

/// <summary>Cảnh báo sản phẩm sắp hết hàng và hết hàng (theo từng biến thể).</summary>
[Authorize(Roles = "Admin,Staff")]
[Route("Admin/TonKho")]
public class TonKhoAdminController : Controller
{
    private readonly ISanPhamQueryRepository _query;
    public TonKhoAdminController(ISanPhamQueryRepository query) { _query = query; }

    [HttpGet("")]
    public async Task<IActionResult> Index(int nguong = 5, CancellationToken ct = default)
        => View(await _query.LayCanhBaoTonKhoAsync(nguong, ct));
}
