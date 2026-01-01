using System;
using System.Collections.Generic;
using System.Text;

namespace BidaTrader.Shared.DTOs
{
        public class RoleDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Code { get; set; }
            public string? Description { get; set; }
        }

        public class RoleWithPermissionsDto : RoleDto
        {
            public List<int> AssignedPermissionIds { get; set; } = new();
        }

        public class UpdateRolePermissionsDto
        {
            public int RoleId { get; set; }
            public List<int> PermissionIds { get; set; } = new();
        }
    
}
