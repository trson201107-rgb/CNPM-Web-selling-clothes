using Microsoft.Data.SqlClient;
using ThoiTrangNam.Web.Models;

namespace ThoiTrangNam.Web.Data;

public static class SqlText
{
    /// <summary>Giá trị tham số: null thành DBNull.</summary>
    public static object Val(object? v) => v ?? DBNull.Value;

    /// <summary>Thoát ký tự đặc biệt của LIKE để người dùng gõ % _ [ không làm sai kết quả.</summary>
    public static string EscapeLike(string s) =>
        s.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

    /// <summary>Ánh xạ một dòng kết quả thành ProductListItem. Cần đủ cột: MaSP, TenSP, GiaBan, AnhDaiDien, MaDM, TenDM, LaNoiBat, NgayTao, TongTon.</summary>
    public static ProductListItem MapProductListItem(SqlDataReader r) => new()
    {
        MaSP = r.Int("MaSP"),
        TenSP = r.Str("TenSP"),
        GiaBan = r.Dec("GiaBan"),
        AnhDaiDien = r.StrN("AnhDaiDien"),
        MaDM = r.Int("MaDM"),
        TenDM = r.Str("TenDM"),
        LaNoiBat = r.Bool("LaNoiBat"),
        NgayTao = r.Dt("NgayTao"),
        TongTon = r.Int("TongTon"),
    };
}
