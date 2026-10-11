using Microsoft.EntityFrameworkCore;

namespace asg2.DAL.Data
{
    /// <summary>
    /// Không khai báo DbSet ở đây để mỗi người thêm entity mà không phải sửa file này.
    /// Cách thêm entity mới: tạo class trong Entities/ và một IEntityTypeConfiguration&lt;T&gt;
    /// trong Data/Configurations/ (tự được nạp bởi ApplyConfigurationsFromAssembly).
    /// Repository dùng db.Set&lt;T&gt;().
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
