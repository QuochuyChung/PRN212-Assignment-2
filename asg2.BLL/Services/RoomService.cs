using asg2.BLL.Common;
using asg2.BLL.Dtos;
using asg2.BLL.Mapping;
using asg2.BLL.Realtime;
using asg2.DAL.Entities;
using asg2.DAL.Repositories;

namespace asg2.BLL.Services
{
    internal sealed class RoomService : IRoomService
    {
        private readonly IRoomRepository _rooms;
        private readonly IRealtimeNotifier _notifier;

        public RoomService(IRoomRepository rooms, IRealtimeNotifier notifier)
        {
            _rooms = rooms;
            _notifier = notifier;
        }

        public async Task<IReadOnlyList<RoomDto>> GetAllAsync()
        {
            var list = await _rooms.GetAllAsync();
            return list.Select(r => r.ToDto()).ToList();
        }

        public async Task<IReadOnlyList<RoomDto>> GetActiveAsync()
        {
            var list = await _rooms.GetActiveAsync();
            return list.Select(r => r.ToDto()).ToList();
        }

        public async Task<RoomDto?> GetByIdAsync(int id)
        {
            var room = await _rooms.GetByIdAsync(id);
            return room?.ToDto();
        }

        public async Task<ServiceResult<RoomDto>> CreateAsync(RoomSaveDto dto)
        {
            var errors = Validate(dto);
            if (errors.Count > 0)
                return ServiceResult<RoomDto>.Failure(errors);

            var name = dto.Name.Trim();
            if (await _rooms.NameExistsAsync(name))
                return ServiceResult<RoomDto>.Failure(nameof(dto.Name), "Tên phòng đã tồn tại.");

            var room = new Room
            {
                Name = name,
                Building = NormalizeBuilding(dto.Building),
                IsActive = dto.IsActive
            };
            await _rooms.AddAsync(room);

            var result = room.ToDto();
            await _notifier.SendToAdminsAsync(HubEvents.RoomCreated, result);
            return ServiceResult<RoomDto>.Success(result);
        }

        public async Task<ServiceResult<RoomDto>> UpdateAsync(int id, RoomSaveDto dto)
        {
            var room = await _rooms.GetByIdAsync(id);
            if (room is null)
                return ServiceResult<RoomDto>.Failure(string.Empty, "Không tìm thấy phòng thi.");

            var errors = Validate(dto);
            if (errors.Count > 0)
                return ServiceResult<RoomDto>.Failure(errors);

            var name = dto.Name.Trim();
            if (await _rooms.NameExistsAsync(name, id))
                return ServiceResult<RoomDto>.Failure(nameof(dto.Name), "Tên phòng đã tồn tại.");

            room.Name = name;
            room.Building = NormalizeBuilding(dto.Building);
            room.IsActive = dto.IsActive;
            await _rooms.UpdateAsync(room);

            var result = room.ToDto();
            await _notifier.SendToAdminsAsync(HubEvents.RoomUpdated, result);
            return ServiceResult<RoomDto>.Success(result);
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var room = await _rooms.GetByIdAsync(id);
            if (room is null)
                return ServiceResult.Failure(string.Empty, "Không tìm thấy phòng thi.");

            if (await _rooms.IsInUseAsync(id))
                return ServiceResult.Failure(string.Empty,
                    "Phòng thi đang được sử dụng bởi các phiên thi nên không thể xóa.");

            if (!await _rooms.TryDeleteAsync(room))
                return ServiceResult.Failure(string.Empty,
                    "Phòng thi đang được sử dụng nên không thể xóa.");

            await _notifier.SendToAdminsAsync(HubEvents.RoomDeleted, id);
            return ServiceResult.Success();
        }

        private static string? NormalizeBuilding(string? building)
        {
            var value = (building ?? string.Empty).Trim();
            return value.Length == 0 ? null : value;
        }

        private static Dictionary<string, string[]> Validate(RoomSaveDto dto)
        {
            var errors = new Dictionary<string, string[]>();

            var name = (dto.Name ?? string.Empty).Trim();
            if (name.Length == 0)
                errors[nameof(dto.Name)] = new[] { "Tên phòng là bắt buộc." };
            else if (name.Length > 50)
                errors[nameof(dto.Name)] = new[] { "Tên phòng tối đa 50 ký tự." };

            var building = (dto.Building ?? string.Empty).Trim();
            if (building.Length > 50)
                errors[nameof(dto.Building)] = new[] { "Tên tòa nhà tối đa 50 ký tự." };

            return errors;
        }
    }
}
