using BidaTrader.Shared.DTOs;
using BidaTrader.Shared.Models;
using BidaTrader.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BidaTrader.Server.Services
{
    public class RoleService
    {
        private readonly AppDbContext _context;

        public RoleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoleWithPermissionsDto>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Select(r => new RoleWithPermissionsDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Description = r.Description,
                    AssignedPermissionIds = r.RolePermissions.Select(rp => rp.PermissionId).ToList()
                })
                .ToListAsync();
        }

        public async Task<(bool IsSuccess, string Message, Role? Data)> CreateRoleAsync(RoleDto dto)
        {
            if (await _context.Roles.AnyAsync(r => r.Name == dto.Name))
                return (false, "Tên vai trò đã tồn tại", null);

            var role = new Role
            {
                Name = dto.Name,
                Code = string.IsNullOrWhiteSpace(dto.Code) ? dto.Name.ToUpper().Replace(" ", "_") : dto.Code,
                Description = dto.Description
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return (true, "Tạo thành công", role);
        }

        public async Task<(bool IsSuccess, string Message)> UpdateRoleAsync(RoleDto dto)
        {
            var role = await _context.Roles.FindAsync(dto.Id);
            if (role == null) return (false, "Không tìm thấy vai trò");

            // Không cho sửa Code của các Role hệ thống
            if ((role.Code == "ADMIN" || role.Code == "CUSTOMER") && role.Code != dto.Code)
                return (false, "Không thể thay đổi Mã của vai trò hệ thống.");

            role.Name = dto.Name;
            role.Code = dto.Code;
            role.Description = dto.Description;

            await _context.SaveChangesAsync();
            return (true, "Cập nhật thành công");
        }

        public async Task<(bool IsSuccess, string Message)> DeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return (false, "Không tìm thấy vai trò");

            bool isInUse = await _context.AccountRoles.AnyAsync(ar => ar.RoleId == id);
            if (isInUse) return (false, "Không thể xóa vai trò đang được gán cho người dùng.");

            if (role.Code == "ADMIN" || role.Code == "CUSTOMER" || role.Code == "STORE_OWNER")
                return (false, "Không thể xóa vai trò mặc định của hệ thống.");

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return (true, "Xóa thành công");
        }

        // --- Logic gán quyền (thuộc về Role) ---
        public async Task<bool> UpdateRolePermissionsAsync(UpdateRolePermissionsDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var oldPermissions = await _context.RolePermissions.Where(rp => rp.RoleId == dto.RoleId).ToListAsync();
                _context.RolePermissions.RemoveRange(oldPermissions);

                if (dto.PermissionIds != null && dto.PermissionIds.Any())
                {
                    var newPermissions = dto.PermissionIds.Select(permId => new RolePermission
                    {
                        RoleId = dto.RoleId,
                        PermissionId = permId
                    });
                    await _context.RolePermissions.AddRangeAsync(newPermissions);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}