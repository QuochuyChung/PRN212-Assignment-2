using asg2.BLL;
using asg2.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Tầng Web chỉ biết BLL: AddBusinessLayer đăng ký toàn bộ phần còn lại (kể cả truy cập dữ liệu).
builder.Services.AddBusinessLayer(builder.Configuration);
builder.Services.AddAppAuthentication();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", AuthPolicies.AdminOnly);
    options.Conventions.AuthorizeFolder("/Student", AuthPolicies.StudentOnly);
});

var app = builder.Build();

// DB là server dùng chung nên mặc định không tự migrate/seed (xem README, mục Database).
if (!await app.Services.InitializeDatabaseAsync())
{
    app.Logger.LogInformation(
        "Bỏ qua migrate/seed lúc khởi động (Database:InitializeOnStartup = false).");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
