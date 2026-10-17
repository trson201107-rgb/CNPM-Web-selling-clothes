using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using ThoiTrangNam.Web.Data;
using ThoiTrangNam.Web.Models;

namespace ThoiTrangNam.Web.Services;

/// <summary>
/// Dữ liệu mẫu trong script SQL có mật khẩu giả ('hash_demo_01'...). Khi ứng dụng khởi động,
/// lớp này đổi các tài khoản đó sang mật khẩu thật đã băm: Demo@123.
/// Chỉ đụng tới các dòng còn mật khẩu giả. Tắt bằng cấu hình Seed:ResetDemoPasswords = false.
/// </summary>
public class DemoAccountSeeder : IHostedService
{
    public const string MatKhauDemo = "Demo@123";

    private readonly IDbConnectionFactory _factory;
    private readonly IConfiguration _config;
    private readonly ILogger<DemoAccountSeeder> _logger;

    public DemoAccountSeeder(IDbConnectionFactory factory, IConfiguration config, ILogger<DemoAccountSeeder> logger)
    {
        _factory = factory;
        _config = config;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_config.GetValue<bool>("Seed:ResetDemoPasswords", true)) return;
        try
        {
            var ids = new List<int>();
            await using (var conn = await _factory.OpenConnectionAsync(cancellationToken))
            {
                await using var cmd = new SqlCommand(
                    "SELECT MaTK FROM TAI_KHOAN WHERE MatKhauHash LIKE 'hash[_]demo[_]%'", conn);
                await using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await rd.ReadAsync(cancellationToken)) ids.Add(rd.Int("MaTK"));
            }
            if (ids.Count == 0) return;

            var hasher = new PasswordHasher<TaiKhoan>();
            await using (var conn = await _factory.OpenConnectionAsync(cancellationToken))
            {
                foreach (var id in ids)
                {
                    await using var cmd = new SqlCommand("UPDATE TAI_KHOAN SET MatKhauHash = @h WHERE MaTK = @id", conn);
                    cmd.Parameters.AddWithValue("@h", hasher.HashPassword(new TaiKhoan(), MatKhauDemo));
                    cmd.Parameters.AddWithValue("@id", id);
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }
            }
            _logger.LogInformation("Đã đặt mật khẩu mẫu cho {Count} tài khoản demo.", ids.Count);
        }
        catch (Exception ex)
        {
            // Không để việc này làm ứng dụng không khởi động được (ví dụ chưa chạy script SQL).
            _logger.LogWarning(ex, "Không đặt được mật khẩu mẫu cho tài khoản demo.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
