using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;

namespace BidaTrader.Client.Auth
{
    public static class JwtParser
    {
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1]; // Lấy phần payload
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
                // Trích xuất các claims mà chúng ta đã định nghĩa ở API
                if (keyValuePairs.TryGetValue("role", out var roles) || keyValuePairs.TryGetValue(ClaimTypes.Role, out roles))
                {
                    if (roles is JsonElement rolesElem && rolesElem.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in rolesElem.EnumerateArray())
                        {
                            // QUAN TRỌNG: Khi thêm vào List<Claim>, PHẢI dùng ClaimTypes.Role
                            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                        }
                    }
                    else
                    {
                        // QUAN TRỌNG: Khi thêm vào List<Claim>, PHẢI dùng ClaimTypes.Role
                        claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
                    }
                }

                if (keyValuePairs.TryGetValue(ClaimTypes.Email, out var email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, email.ToString()));
                }

                if (keyValuePairs.TryGetValue("sub", out var sub)) // "sub" là ID người dùng
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, sub.ToString()));
                }
            }

            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}