using System.Text.RegularExpressions;
using asg2.BLL.Common;
using asg2.BLL.Dtos;
using asg2.BLL.Mapping;
using asg2.BLL.Realtime;
using asg2.DAL.Entities;
using asg2.DAL.Repositories;
using asg2.DAL.Security;

namespace asg2.BLL.Services
{
    internal sealed class AccountService : IAccountService
    {
        private const int MinPasswordLength = 6;
        private const int MaxPasswordLength = 100;

        private static readonly Regex UsernamePattern = new(@"^[A-Za-z0-9_.\-]{3,50}$", RegexOptions.Compiled);

        private readonly IAccountRepository _accounts;
        private readonly IRealtimeNotifier _notifier;

        public AccountService(IAccountRepository accounts, IRealtimeNotifier notifier)
        {
            _accounts = accounts;
            _notifier = notifier;
        }

        public async Task<IReadOnlyList<AccountDto>> GetStudentsAsync(string? search = null, bool includeInactive = false)
        {
            var list = await _accounts.GetByRoleAsync(AccountRole.Student, search, includeInactive);
            return list.Select(a => a.ToDto()).ToList();
        }

        public async Task<AccountDto?> GetByIdAsync(int id)
        {
            var account = await _accounts.GetByIdAsync(id);
            return account?.ToDto();
        }

        public async Task<ServiceResult<AccountDto>> CreateAsync(AccountSaveDto dto)
        {
            var errors = Validate(dto, requirePassword: true);
            if (errors.Count > 0)
                return ServiceResult<AccountDto>.Failure(errors);

            var username = dto.Username.Trim();
            if (await _accounts.UsernameExistsAsync(username))
                return ServiceResult<AccountDto>.Failure(nameof(dto.Username), "Tên đăng nhập đã tồn tại.");

            var account = new Account
            {
                Username = username,
                FullName = dto.FullName.Trim(),
                PasswordHash = AccountPasswordHasher.Hash(dto.Password!),
                Role = AccountRole.Student,
                IsActive = dto.IsActive
            };
            await _accounts.AddAsync(account);

            var result = account.ToDto();
            await _notifier.SendToAdminsAsync(HubEvents.AccountCreated, result);
            return ServiceResult<AccountDto>.Success(result);
        }

        public async Task<ServiceResult<AccountDto>> UpdateAsync(int actorId, int id, AccountSaveDto dto)
        {
            var account = await _accounts.GetByIdAsync(id);
            if (account is null)
                return ServiceResult<AccountDto>.Failure(string.Empty, "Không tìm thấy tài khoản.");
            if (account.Role != AccountRole.Student)
                return ServiceResult<AccountDto>.Failure(string.Empty, "Không thể thao tác trên tài khoản quản trị.");

            var errors = Validate(dto, requirePassword: false);
            if (errors.Count > 0)
                return ServiceResult<AccountDto>.Failure(errors);

            if (actorId == id && !dto.IsActive)
                return ServiceResult<AccountDto>.Failure(nameof(dto.IsActive), "Bạn không thể tự khóa tài khoản của mình.");

            var username = dto.Username.Trim();
            if (await _accounts.UsernameExistsAsync(username, id))
                return ServiceResult<AccountDto>.Failure(nameof(dto.Username), "Tên đăng nhập đã tồn tại.");

            var wasActive = account.IsActive;

            account.Username = username;
            account.FullName = dto.FullName.Trim();
            account.IsActive = dto.IsActive;
            if (!string.IsNullOrEmpty(dto.Password))
                account.PasswordHash = AccountPasswordHasher.Hash(dto.Password);

            await _accounts.UpdateAsync(account);

            var result = account.ToDto();
            await _notifier.SendToAdminsAsync(HubEvents.AccountUpdated, result);

            // Vừa bị khóa: đá khỏi hệ thống ngay (cookie cũng sẽ bị vô hiệu ở request kế tiếp).
            if (wasActive && !account.IsActive)
                await _notifier.SendToUserAsync(account.Id, HubEvents.ForceLogout, null);

            return ServiceResult<AccountDto>.Success(result);
        }

        public async Task<ServiceResult> DeleteAsync(int actorId, int id)
        {
            var account = await _accounts.GetByIdAsync(id);
            if (account is null)
                return ServiceResult.Failure(string.Empty, "Không tìm thấy tài khoản.");
            if (account.Role != AccountRole.Student)
                return ServiceResult.Failure(string.Empty, "Không thể thao tác trên tài khoản quản trị.");
            if (actorId == id)
                return ServiceResult.Failure(string.Empty, "Bạn không thể tự xóa tài khoản của mình.");

            if (!await _accounts.TryDeleteAsync(account))
                return ServiceResult.Failure(string.Empty,
                    "Tài khoản đã được sử dụng (ví dụ đã có lịch thi) nên không thể xóa. Hãy khóa tài khoản thay vì xóa.");

            await _notifier.SendToAdminsAsync(HubEvents.AccountDeleted, id);
            await _notifier.SendToUserAsync(id, HubEvents.ForceLogout, null);
            return ServiceResult.Success();
        }

        private static Dictionary<string, string[]> Validate(AccountSaveDto dto, bool requirePassword)
        {
            var errors = new Dictionary<string, string[]>();

            var username = (dto.Username ?? string.Empty).Trim();
            if (username.Length == 0)
                errors[nameof(dto.Username)] = new[] { "Tên đăng nhập là bắt buộc." };
            else if (!UsernamePattern.IsMatch(username))
                errors[nameof(dto.Username)] = new[] { "Tên đăng nhập 3-50 ký tự, chỉ gồm chữ, số, dấu _ . -" };

            var fullName = (dto.FullName ?? string.Empty).Trim();
            if (fullName.Length == 0)
                errors[nameof(dto.FullName)] = new[] { "Họ tên là bắt buộc." };
            else if (fullName.Length > 100)
                errors[nameof(dto.FullName)] = new[] { "Họ tên tối đa 100 ký tự." };

            var hasPassword = !string.IsNullOrEmpty(dto.Password);
            if (requirePassword && !hasPassword)
                errors[nameof(dto.Password)] = new[] { "Mật khẩu là bắt buộc." };
            else if (hasPassword && (dto.Password!.Length < MinPasswordLength || dto.Password.Length > MaxPasswordLength))
                errors[nameof(dto.Password)] = new[] { $"Mật khẩu từ {MinPasswordLength} đến {MaxPasswordLength} ký tự." };

            return errors;
        }
    }
}
