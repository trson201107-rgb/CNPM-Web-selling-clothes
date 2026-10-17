using Microsoft.Data.SqlClient;

namespace ThoiTrangNam.Web.Data;

/// <summary>Đọc cột theo tên, xử lý NULL gọn hơn. Dùng chung cho mọi Repository.</summary>
public static class DbReaderExtensions
{
    public static string Str(this SqlDataReader r, string col)
    {
        var i = r.GetOrdinal(col);
        return r.IsDBNull(i) ? "" : r.GetString(i);
    }
    public static string? StrN(this SqlDataReader r, string col)
    {
        var i = r.GetOrdinal(col);
        return r.IsDBNull(i) ? null : r.GetString(i);
    }
    public static int Int(this SqlDataReader r, string col) => r.GetInt32(r.GetOrdinal(col));
    public static int? IntN(this SqlDataReader r, string col)
    {
        var i = r.GetOrdinal(col);
        return r.IsDBNull(i) ? null : r.GetInt32(i);
    }
    public static decimal Dec(this SqlDataReader r, string col) => r.GetDecimal(r.GetOrdinal(col));
    public static bool Bool(this SqlDataReader r, string col) => r.GetBoolean(r.GetOrdinal(col));
    public static DateTime Dt(this SqlDataReader r, string col) => r.GetDateTime(r.GetOrdinal(col));
    public static DateTime? DtN(this SqlDataReader r, string col)
    {
        var i = r.GetOrdinal(col);
        return r.IsDBNull(i) ? null : r.GetDateTime(i);
    }
}
