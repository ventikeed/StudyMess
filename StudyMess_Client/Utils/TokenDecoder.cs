using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace StudyMess_Client.Utils
{
    public static class TokenDecoder
    {
        public static Dictionary<string, string>? DecodeClaims(string? token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var claims = new Dictionary<string, string>();
            foreach (var claim in jwt.Claims)
            {
                claims[claim.Type] = claim.Value;
            }
            return claims;
        }

        public static int? GetUserId(string? token)
        {
            var claims = DecodeClaims(token);
            if (claims != null && claims.TryGetValue(ClaimTypes.NameIdentifier, out var idStr) && int.TryParse(idStr, out var id))
                return id;
            return null;
        }

        public static string? GetUsername(string? token)
        {
            var claims = DecodeClaims(token);
            if (claims != null && claims.TryGetValue(ClaimTypes.Name, out var username))
                return username;
            return null;
        }

        public static string? GetEmail(string? token)
        {
            var claims = DecodeClaims(token);
            if (claims != null && claims.TryGetValue(ClaimTypes.Email, out var email))
                return email;
            return null;
        }

        public static string? GetRole(string? token)
        {
            var claims = DecodeClaims(token);
            if (claims != null && claims.TryGetValue(ClaimTypes.Role, out var role))
                return role;
            return null;
        }

        public static string? GetGroup(string? token)
        {
            var claims = DecodeClaims(token);
            if (claims != null && claims.TryGetValue("GroupId", out var groupId))
                return groupId;
            return null;
        }
        public static string? GetAvatar(string? token)
        {
            var claims = DecodeClaims(token);
            if (claims != null && claims.TryGetValue("Avatar", out var avatar))
                return avatar;
            return null;
        }
        public static string? GetFullName(string? token)
        {
            var claims = DecodeClaims(token);
            if (claims != null && claims.TryGetValue("FullName", out var FullName))
                return FullName;
            return null;
        }
    }
}