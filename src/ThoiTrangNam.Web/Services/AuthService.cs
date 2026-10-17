using Microsoft.AspNetCore.Identity;
using ThoiTrangNam.Web.Models;
using ThoiTrangNam.Web.Repositories;

namespace ThoiTrangNam.Web.Services;

public class LoginResult
{
    public bool ThanhCong { get; init; }
    public string? Loi { get; init; }
    public TaiKhoan? TaiKhoan { get; init; }
}

public interface IAuthService
{
    Task<LoginResult> DangNhapAsync(string email, string matKhau, CancellationToken ct = default);
    /// <summary>Đăng ký khách hàng mới. Lỗi nghiệp vụ (trùng email, trùng số điện thoại) ném NghiepVuException.</summary>
    Task<int> DangKyAsync(RegisterViewModel vm, CancellationToken ct = default);
}

/// <summary>Đăng nhập, đăng ký. Mật khẩu băm bằng PBKDF2 (PasswordHasher của ASP.NET), không lưu mật khẩu thô.</summary>
public class AuthService : IAuthService
{
    private static readonly PasswordHasher<TaiKhoan> Hasher = new();
    private const string SaiThongTin = "Email hoặc mật khẩu không đúng.";

    private readonly ITaiKhoanRepository _repo;
    public AuthService(ITaiKhoanRepository repo) { _repo = repo; }

    public async Task<LoginResult> DangNhapAsync(string email, string matKhau, CancellationToken ct = default)
    {
        var tk = await _repo.LayTheoEmailAsync(email.Trim(), ct);
        if (tk == null) return Loi(SaiThongTin);

        if (tk.TrangThai == "Bị khóa")
            return Loi("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");

        if (tk.DangKhoa)
            return Loi($"Đăng nhập sai quá nhiều lần. Vui lòng thử lại sau khoảng {tk.SoPhutKhoaConLai} phút.");

        if (tk.KhoaDaHetHan)
        {
            await _repo.DatLaiDangNhapSaiAsync(tk.MaTK, ct);
            tk.SoLanDangNhapSai = 0;
        }

        if (!KiemTraMatKhau(tk, matKhau))
        {
            await _repo.GhiNhanDangNhapSaiAsync(tk.MaTK, ct);
            return Loi(SaiThongTin);
        }

        if (tk.SoLanDangNhapSai > 0) await _repo.DatLaiDangNhapSaiAsync(tk.MaTK, ct);
        return new LoginResult { ThanhCong = true, TaiKhoan = tk };
    }

    public async Task<int> DangKyAsync(RegisterViewModel vm, CancellationToken ct = default)
    {
        var hash = Hasher.HashPassword(new TaiKhoan(), vm.MatKhau);
        return await _repo.TaoKhachHangAsync(vm.HoTen, vm.Email, hash, vm.SoDienThoai, ct);
    }

    private static bool KiemTraMatKhau(TaiKhoan tk, string matKhau)
    {
        try
        {
            var kq = Hasher.VerifyHashedPassword(tk, tk.MatKhauHash, matKhau);
            return kq != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            // Chuỗi trong CSDL không phải mật khẩu đã băm (ví dụ dữ liệu mẫu chưa được xử lý)
            return false;
        }
    }

    private static LoginResult Loi(string msg) => new() { ThanhCong = false, Loi = msg };
}
