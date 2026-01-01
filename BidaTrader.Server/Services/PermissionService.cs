using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.EntityFrameworkCore;


namespace BidaTrader.Server.Services
{
    public class PermissionService
    {
        private readonly AppDbContext _context;
        public PermissionService(AppDbContext context) { 
            _context = context;
        }

        public async Task<List<Permission>> GetAllAsync()
        {
            return await _context.Permissions.OrderBy(p => p.Code).ToListAsync();
        }

        public async Task<(bool IsSuccess, string Message, Permission? Data)> CreateAsync(Permission dto)
        {
            if (await _context.Permissions.AnyAsync(p => p.Code == dto.Code))
                return (false, "Mã quyền đã tồn tại", null);

            _context.Permissions.Add(dto);
            await _context.SaveChangesAsync();
            return (true, "Tạo thành công", dto);
        }

        public async Task<(bool IsSuccess, string Message)> UpdateAsync(Permission dto)
        {
            var perm = await _context.Permissions.FindAsync(dto.Id);
            if (perm == null) return (false, "Không tìm thấy quyền");

            perm.Name = dto.Name;
            perm.Description = dto.Description;

            await _context.SaveChangesAsync();
            return (true, "Cập nhật thành công");
        }

        public async Task<(bool IsSuccess, string Message)> DeleteAsync(int id)
        {
            var perm = await _context.Permissions.FindAsync(id);
            if (perm == null) return (false, "Không tìm thấy quyền");

            bool isInUse = await _context.RolePermissions.AnyAsync(rp => rp.PermissionId == id);
            if (isInUse) return (false, "Quyền đang được sử dụng trong Role. Hãy gỡ bỏ trước.");

            _context.Permissions.Remove(perm);
            await _context.SaveChangesAsync();
            return (true, "Xóa thành công");
        }
    }
}