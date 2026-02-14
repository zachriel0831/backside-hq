using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HQBackSite.Utils
{
    public static class JwtUtil
    {
        private const string SecurityKey = "d87ba1388bc736061efe0298e2f1265f7075934fab48dee3195e159c6ba35e69";

        public static string Generate(Dictionary<string, string> data, DateTime expires)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // 建立儲存的內容
            var claims = new List<Claim>();
            foreach (string c in data.Keys)
            {
                claims.Add(new Claim(c, data[c]));
            }

            // 建立 JwtToken 內容
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecurityKey)),
                    SecurityAlgorithms.HmacSha256Signature
                    )
            };

            // 建立 JwtToken
            var jwtToken = tokenHandler.CreateToken(tokenDescriptor);

            // 序列化 JwtToken
            var jwtTokenString = tokenHandler.WriteToken(jwtToken);

            return jwtTokenString;
        }

        public static  Dictionary<string, string> Decrypt(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // 驗證JwtToken
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecurityKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtResult = (JwtSecurityToken)validatedToken;

            var result = new Dictionary<string, string>();
            foreach (var item in jwtResult.Claims)
            {
                result.Add(item.Type, item.Value);
            }

            return result;
        }
    }
}