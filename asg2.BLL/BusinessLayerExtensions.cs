using asg2.BLL.Realtime;
using asg2.DAL.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace asg2.BLL
{
    /// <summary>
    /// Hai điểm duy nhất tầng Web dùng để "nói chuyện" với phần còn lại của hệ thống.
    /// Bên trong mới gọi DAL; Web không bao giờ thấy DAL (xem README, mục Kiến trúc).
    /// </summary>
    public static class BusinessLayerExtensions
    {
        /// <summary>Khóa cấu hình bật/tắt việc migrate + seed lúc khởi động (mặc định tắt vì DB là server dùng chung).</summary>
        public const string InitializeOnStartupKey = "Database:InitializeOnStartup";

        /// <summary>
        /// Đăng ký DAL + toàn bộ service. Service tự được đăng ký: class tên *Service có interface I*Service
        /// trong assembly này (nên service để <c>internal</c>, chỉ interface và DTO là public).
        /// </summary>
        public static IServiceCollection AddBusinessLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDataAccess(configuration);

            // Bản mặc định không làm gì; Web đăng ký SignalRNotifier (P2) sẽ thay thế.
            services.TryAddSingleton<IRealtimeNotifier, NullRealtimeNotifier>();

            var implementations = typeof(BusinessLayerExtensions).Assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
                            && t.Name.EndsWith("Service", StringComparison.Ordinal));

            foreach (var impl in implementations)
            {
                var contract = impl.GetInterfaces().FirstOrDefault(i => i.Name == "I" + impl.Name);
                if (contract is not null)
                    services.AddScoped(contract, impl);
            }

            return services;
        }

        /// <summary>
        /// Áp migration và seed dữ liệu mẫu, CHỈ khi cấu hình <c>Database:InitializeOnStartup = true</c>.
        /// DB của nhóm là server dùng chung nên mặc định không tự ghi lên DB khi chạy app; người giữ migration
        /// (leader) bật cờ này một lần (hoặc đặt biến môi trường <c>Database__InitializeOnStartup=true</c>).
        /// Trả về true nếu đã chạy.
        /// </summary>
        public static async Task<bool> InitializeDatabaseAsync(this IServiceProvider services)
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            if (!bool.TryParse(configuration[InitializeOnStartupKey], out var enabled) || !enabled)
                return false;

            await DatabaseInitializer.InitializeAsync(services);
            return true;
        }
    }
}
