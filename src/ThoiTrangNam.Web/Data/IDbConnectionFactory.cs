using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ThoiTrangNam.Web.Data
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
        Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
        Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
    }

    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private readonly string? _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("ThoiTrangNam")
                              ?? _configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

        public async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    }
}