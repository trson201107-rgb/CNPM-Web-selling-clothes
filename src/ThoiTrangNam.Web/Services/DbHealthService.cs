using ThoiTrangNam.Web.Data;

namespace ThoiTrangNam.Web.Services
{
    public class DbHealthService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DbHealthService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
    }
}
