using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThoiTrangNam.Web.Repositories;

namespace ThoiTrangNam.Web.Controllers;

/// <summary>Trang tổng quan quản trị.</summary>
[Authorize(Roles = "Admin,Staff")]
[Route("Admin")]
public class AdminController : Controller
{
    private readonly ISanPhamQueryRepository _query;
    public AdminController(ISanPhamQueryRepository query) { _query = query; }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _query.LayThongKeAsync(5, ct));
}
