using System.ComponentModel.DataAnnotations;

namespace ThoiTrangNam.Web.Models;

public class TaiKhoan
{
    public int MaTK { get; set; }
    public string HoTen { get; set; } = "";
    public string Email { get; set; } = "";
    public string MatKhauHash { get; set; } = "";
    public string SoDienThoai { get; set; } = "";
    public string TrangThai { get; set; } = "";
    public int SoLanDangNhapSai { get; set; }
    public int MaVaiTro { get; set; }
    public string TenVaiTro { get; set; } = "";
    public bool DangKhoa { get; set; }
    public bool KhoaDaHetHan { get; set; }
    public int SoPhutKhoaConLai { get; set; }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Nhập mật khẩu")]
    public string MatKhau { get; set; } = "";

    public bool GhiNho { get; set; }
    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Nhập họ tên")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
    public string HoTen { get; set; } = "";

    [Required(ErrorMessage = "Nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Nhập số điện thoại")]
    [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại gồm 10 hoặc 11 chữ số, bắt đầu bằng 0")]
    public string SoDienThoai { get; set; } = "";

    [Required(ErrorMessage = "Nhập mật khẩu")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    [StringLength(100, ErrorMessage = "Mật khẩu tối đa 100 ký tự")]
    public string MatKhau { get; set; } = "";

    [Compare(nameof(MatKhau), ErrorMessage = "Mật khẩu nhập lại không khớp")]
    public string XacNhanMatKhau { get; set; } = "";
}
