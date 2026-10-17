using System.Globalization;
using System.Text.RegularExpressions;

namespace ThoiTrangNam.Web.Helpers;

public static class Money
{
    /// <summary>Định dạng tiền Việt: 199.000 ₫ (không phụ thuộc bộ ngôn ngữ cài trên máy chủ).</summary>
    public static string Vnd(decimal v) =>
        v.ToString("N0", CultureInfo.InvariantCulture).Replace(',', '.') + " ₫";
}

public static class Css
{
    private static readonly Regex HexRe = new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    /// <summary>Chỉ nhận mã màu hex hợp lệ (#RRGGBB), còn lại trả về màu xám an toàn.</summary>
    public static string Hex(string? v) => v != null && HexRe.IsMatch(v) ? v : "#cccccc";
}
