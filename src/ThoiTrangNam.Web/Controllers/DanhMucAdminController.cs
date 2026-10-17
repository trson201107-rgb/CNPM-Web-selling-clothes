using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThoiTrangNam.Web.Models;
using ThoiTrangNam.Web.Repositories;
using ThoiTrangNam.Web.Services;

namespace ThoiTrangNam.Web.Controllers;

/// <summary>Quản lý danh mục: thêm, sửa, ẩn/hiện, xóa (chỉ khi không còn sản phẩm).</summary>
[Authorize(Roles = "Admin,Staff")]
[Route("Admin/DanhMuc")]
public class DanhMucAdminController : Controller
{
    private readonly IDanhMucRepository _repo;
    private readonly ICurrentUser _user;

    public DanhMucAdminController(IDanhMucRepository repo, ICurrentUser user)
    {
        _repo = repo;
        _user = user;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _repo.LayTatCaAdminAsync(ct));

    [HttpGet("Create")]
    public IActionResult Create() => View(new DanhMuc());

    [HttpPost("Create")]
    public async Task<IActionResult> Create(DanhMuc dm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dm);
        try
        {
            await _repo.ThemAsync(dm, _user.MaTK, ct);
            TempData["Msg"] = $"Đã thêm danh mục \"{dm.TenDM}\".";
            return RedirectToAction(nameof(Index));
        }
        catch (NghiepVuException ex)
        {
            ModelState.AddModelError(nameof(DanhMuc.TenDM), ex.Message);
            return View(dm);
        }
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var dm = await _repo.LayTheoIdAsync(id, ct);
        if (dm == null) return KhongTimThay();
        return View(dm);
    }

    [HttpPost("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, DanhMuc dm, CancellationToken ct)
    {
        dm.MaDM = id;
        if (!ModelState.IsValid) return View(dm);
        try
        {
            var ok = await _repo.CapNhatAsync(dm, _user.MaTK, ct);
            if (!ok) return KhongTimThay();
            TempData["Msg"] = $"Đã cập nhật danh mục \"{dm.TenDM}\".";
            return RedirectToAction(nameof(Index));
        }
        catch (NghiepVuException ex)
        {
            ModelState.AddModelError(nameof(DanhMuc.TenDM), ex.Message);
            return View(dm);
        }
    }

    [HttpPost("Hide/{id:int}")]
    public async Task<IActionResult> Hide(int id, CancellationToken ct)
    {
        var ok = await _repo.DoiTrangThaiAsync(id, false, _user.MaTK, ct);
        TempData[ok ? "Msg" : "Err"] = ok
            ? "Đã ẩn danh mục. Các sản phẩm thuộc danh mục này không còn hiển thị cho khách."
            : "Không tìm thấy danh mục.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Show/{id:int}")]
    public async Task<IActionResult> Show(int id, CancellationToken ct)
    {
        var ok = await _repo.DoiTrangThaiAsync(id, true, _user.MaTK, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Đã hiển thị lại danh mục." : "Không tìm thấy danh mục.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var kq = await _repo.XoaAsync(id, ct);
        switch (kq)
        {
            case XoaDanhMucKetQua.DaXoa:
                TempData["Msg"] = "Đã xóa danh mục.";
                break;
            case XoaDanhMucKetQua.CoSanPham:
                TempData["Err"] = "Không thể xóa vì danh mục vẫn còn sản phẩm. Hãy chuyển hoặc xóa sản phẩm trước, hoặc dùng chức năng Ẩn.";
                break;
            default:
                TempData["Err"] = "Không tìm thấy danh mục.";
                break;
        }
        return RedirectToAction(nameof(Index));
    }

    private IActionResult KhongTimThay()
    {
        TempData["Err"] = "Không tìm thấy danh mục.";
        return RedirectToAction(nameof(Index));
    }
}
