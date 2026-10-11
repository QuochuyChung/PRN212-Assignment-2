# tools/check-layers.ps1
# Kiểm tra ràng buộc kiến trúc: tầng Web TUYỆT ĐỐI không chạm DAL (xem README, mục Kiến trúc).
# Chạy ở thư mục gốc repo trước khi mở PR (reviewer chạy lại khi duyệt). Thoát mã 1 nếu vi phạm.
#   powershell -NoProfile -File tools/check-layers.ps1
$ErrorActionPreference = 'Stop'
$bad = @()

$web = Get-Content asg2.Web/asg2.Web.csproj -Raw
$bll = Get-Content asg2.BLL/asg2.BLL.csproj -Raw

if ($web -match 'asg2\.DAL') { $bad += 'asg2.Web.csproj tham chieu asg2.DAL' }
if ($bll -notmatch 'ProjectReference Include="[^"]*asg2\.DAL[^"]*"\s+PrivateAssets="compile"') {
    $bad += 'asg2.BLL.csproj thieu PrivateAssets="compile" khi tham chieu asg2.DAL'
}
if ($web -match 'Microsoft\.EntityFrameworkCore(?!\.Design)') { $bad += 'asg2.Web.csproj cai package EF Core ngoai Design' }

Get-ChildItem asg2.Web -Recurse -Include *.cs,*.cshtml |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
    Select-String -Pattern 'asg2\.DAL|DbContext|Microsoft\.EntityFrameworkCore|Repository|\bMigrations?\b' |
    ForEach-Object { $bad += "$($_.Path):$($_.LineNumber): $($_.Line.Trim())" }

if ($bad.Count -gt 0) {
    $bad | ForEach-Object { Write-Host "VI PHAM: $_" -ForegroundColor Red }
    exit 1
}
Write-Host 'OK: tang Web khong cham DAL' -ForegroundColor Green
