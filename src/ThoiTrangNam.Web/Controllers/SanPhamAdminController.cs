using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThoiTrangNam.Web.Models;
using ThoiTrangNam.Web.Repositories;
using ThoiTrangNam.Web.Services;

namespace ThoiTrangNam.Web.Controllers;

/// <summary>Quản lý sản phẩm: thêm, sửa, ẩn/hiện, xóa hẳn. Dành cho Admin và Staff.</summary>
[Authorize(Roles = "Admin,Staff")]
[Route("Admin/SanPham")]
public class SanPhamAdminController : Controller
{
    private readonly ISanPhamRepository _repo;
    private readonly ICurrentUser _user;
    private readonly ILogger<SanPhamAdminController> _logger;

    public SanPhamAdminController(ISanPhamRepository repo, ICurrentUser user, ILogger<SanPhamAdminController> logger)
    {
        _repo = repo;
        _user = user;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, string? trangThai, CancellationToken ct)
    {
        ViewData["q"] = q;
        ViewData["trangThai"] = trangThai;
        var ds = await _repo.LayDanhSachAdminAsync(q, trangThai, ct);
        return View(ds);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var vm = new SanPhamFormViewModel();
        vm.BienThes.Add(new BienTheFormRow());
        await NapLuaChonAsync(vm, ct);
        return View(vm);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(SanPhamFormViewModel vm, CancellationToken ct)
    {
        KiemTraBienTheTrongForm(vm);
        if (ModelState.IsValid)
        {
            try
            {
                var id = await _repo.ThemAsync(vm, _user.MaTK, ct);
                TempData["Msg"] = $"Đã thêm sản phẩm \"{vm.TenSP}\" (mã {id}).";
                return RedirectToAction(nameof(Index));
            }
            catch (NghiepVuException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        await NapLuaChonAsync(vm, ct);
        return View(vm);
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var vm = await _repo.LayChoFormAsync(id, ct);
        if (vm == null) return KhongTimThay();
        await NapLuaChonAsync(vm, ct);
        return View(vm);
    }

    [HttpPost("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, SanPhamFormViewModel vm, CancellationToken ct)
    {
        vm.MaSP = id;
        KiemTraBienTheTrongForm(vm);
        if (ModelState.IsValid)
        {
            try
            {
                var ok = await _repo.CapNhatAsync(vm, _user.MaTK, ct);
                if (!ok) return KhongTimThay();
                TempData["Msg"] = $"Đã cập nhật sản phẩm \"{vm.TenSP}\".";
                return RedirectToAction(nameof(Index));
            }
            catch (NghiepVuException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        await NapLuaChonAsync(vm, ct);
        return View(vm);
    }

    [HttpPost("Hide/{id:int}")]
    public async Task<IActionResult> Hide(int id, CancellationToken ct)
    {
        var ok = await _repo.DoiTrangThaiAsync(id, TrangThaiSanPham.NgungBan, _user.MaTK, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Đã ẩn sản phẩm. Khách hàng sẽ không còn thấy sản phẩm này." : "Không tìm thấy sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Show/{id:int}")]
    public async Task<IActionResult> Show(int id, CancellationToken ct)
    {
        var ok = await _repo.DoiTrangThaiAsync(id, TrangThaiSanPham.DangBan, _user.MaTK, ct);
        TempData[ok ? "Msg" : "Err"] = ok ? "Đã hiển thị lại sản phẩm." : "Không tìm thấy sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Xóa hẳn: chỉ Admin, và chỉ khi không còn đơn hàng hoặc đánh giá tham chiếu.</summary>
    [HttpPost("Delete/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var kq = await _repo.XoaHanAsync(id, ct);
        switch (kq)
        {
            case XoaSanPhamKetQua.DaXoa:
                TempData["Msg"] = "Đã xóa hẳn sản phẩm khỏi hệ thống.";
                break;
            case XoaSanPhamKetQua.CoThamChieu:
                TempData["Err"] = "Không thể xóa hẳn vì sản phẩm đã có đơn hàng hoặc đánh giá. Hãy dùng chức năng Ẩn.";
                break;
            default:
                TempData["Err"] = "Không tìm thấy sản phẩm.";
                break;
        }
        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------------
    private IActionResult KhongTimThay()
    {
        TempData["Err"] = "Không tìm thấy sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    private async Task NapLuaChonAsync(SanPhamFormViewModel vm, CancellationToken ct)
    {
        vm.DanhMucs = await _repo.LayDanhMucAsync(ct);
        vm.Maus = await _repo.LayMauSacAsync(ct);
        vm.Sizes = await _repo.LayKichCoAsync(ct);
    }

    /// <summary>Bỏ qua kiểm tra của các dòng biến thể đã đánh dấu xóa; yêu cầu còn ít nhất một dòng.</summary>
    private void KiemTraBienTheTrongForm(SanPhamFormViewModel vm)
    {
        for (int i = 0; i < vm.BienThes.Count; i++)
        {
            if (!vm.BienThes[i].Xoa) continue;
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith($"BienThes[{i}].")).ToList())
                ModelState[key]!.Errors.Clear();
        }
        if (!vm.BienThes.Any(b => !b.Xoa))
            ModelState.AddModelError("", "Sản phẩm phải có ít nhất một biến thể (màu + size).");
    }
}
