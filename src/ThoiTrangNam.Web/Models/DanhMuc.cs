using System.ComponentModel.DataAnnotations;

namespace ThoiTrangNam.Web.Models;

/// <summary>Entity ứng với bảng DANH_MUC.</summary>
public class DanhMuc
{
    public int MaDM { get; set; }

    [Required(ErrorMessage = "Nhập tên danh mục")]
    [StringLength(50, ErrorMessage = "Tên danh mục tối đa 50 ký tự")]
    public string TenDM { get; set; } = "";

    [StringLength(255, ErrorMessage = "Mô tả tối đa 255 ký tự")]
    public string? MoTa { get; set; }

    /// <summary>true = hiển thị cho khách, false = đã ẩn.</summary>
    public bool TrangThai { get; set; } = true;

    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public int? NguoiTao { get; set; }
    public int? NguoiCapNhat { get; set; }
}

public class DanhMucAdminItem
{
    public int MaDM { get; set; }
    public string TenDM { get; set; } = "";
    public string? MoTa { get; set; }
    public bool TrangThai { get; set; }
    public int SoSanPham { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public string? NguoiCapNhatTen { get; set; }
}

public enum XoaDanhMucKetQua { DaXoa, KhongTimThay, CoSanPham }
