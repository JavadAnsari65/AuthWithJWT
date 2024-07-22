using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using sample3.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace sample3.Controller
{
    [Route("[Action]")]
    [ApiController]
    public class AuthWithRefereshTokenController : ControllerBase
    {
        private readonly Context _context;
        private string key = "this is my custom Secret key for authnetication";

        public AuthWithRefereshTokenController(Context context)
        {
            _context = context;
        }


        [HttpPost]
        public IActionResult Register2([FromBody] DtoUser user)
        {
            var userExists = _context.Users.Any(x => x.UserName == user.UserName);
            if (userExists)
            {
                return BadRequest("User already exists");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password + key + user.UserName);

            var users = new User
            {
                UserName = user.UserName,
                Password = user.Password,
                Token = "token"
            };

            _context.Users.Add(users);
            _context.SaveChanges();

            return Ok("User created");
        }


        [HttpPost]
        public IActionResult Login2([FromBody] DtoUser user)
        {
            var userExists = _context.Users.Any(x => x.UserName == user.UserName);
            if (!userExists)
            {
                return BadRequest("User does not exist");
            }

            var userFromDb = _context.Users.FirstOrDefault(x => x.UserName == user.UserName);

            if (!BCrypt.Net.BCrypt.Verify(user.Password + key + user.UserName, userFromDb.Password))
            {
                return BadRequest("Password is incorrect");
            }

            var userId = userFromDb.Id;
            var userRoles = _context.UserRoles.Where(x => x.UserId == userId).ToList();

            var permisons = new List<string>();
            foreach (var userRole in userRoles)
            {
                var role = _context.Roles.FirstOrDefault(x => x.Id == userRole.RoleId);
                var rolePermisons = _context.RolePermissions.Where(x => x.RoleId == role.Id).ToList();
                foreach (var rolePermison in rolePermisons)
                {
                    var permison = _context.Permissions.FirstOrDefault(x => x.Id == rolePermison.PermissionId);
                    permisons.Add(permison.Name);
                }
            }

            var tokenModel = GenerateTokens2(userFromDb, permisons);

            return Ok(tokenModel);
        }


        [HttpPost]
        public IActionResult Refresh([FromBody] RefreshTokenModel refreshTokenModel)
        {
            var principal = GetPrincipalFromExpiredToken(refreshTokenModel.RefreshToken);
            if (principal == null)
            {
                return Unauthorized();
            }

            var username = principal.Identity.Name;
            var userFromDb = _context.Users.FirstOrDefault(x => x.UserName == username);
            if (userFromDb == null)
            {
                return Unauthorized();
            }

            var userId = userFromDb.Id;
            var userRoles = _context.UserRoles.Where(x => x.UserId == userId).ToList();

            var permisons = new List<string>();
            foreach (var userRole in userRoles)
            {
                var role = _context.Roles.FirstOrDefault(x => x.Id == userRole.RoleId);
                var rolePermisons = _context.RolePermissions.Where(x => x.RoleId == role.Id).ToList();
                foreach (var rolePermison in rolePermisons)
                {
                    var permison = _context.Permissions.FirstOrDefault(x => x.Id == rolePermison.PermissionId);
                    permisons.Add(permison.Name);
                }
            }

            var newTokenModel = GenerateTokens2(userFromDb, permisons);
            return Ok(newTokenModel);
        }


        private TokenModel GenerateTokens2(User user, List<string> permisons)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("this is my custom Secret key for authnetication");

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
        };

            claims.AddRange(permisons.Select(permison => new Claim("permisons", permison)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var accessToken = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = GenerateRefreshToken();

            return new TokenModel
            {
                AccessToken = tokenHandler.WriteToken(accessToken),
                RefreshToken = refreshToken
            };
        }


        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }


        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is my custom Secret key for authnetication")),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}



//public class AuthController : Controller
//{
//    private readonly Context _context;
//    private string key = "this is my custom Secret key for authnetication";

//    public AuthController(Context context)
//    {
//        _context = context;
//    }


//    [HttpPost]
//    public IActionResult Register([FromBody] DtoUser user)
//    {
//        var userExists = _context.Users.Any(x => x.UserName == user.UserName);
//        if (userExists)
//        {
//            return BadRequest("User already exists");
//        }

//        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password + key + user.UserName);

//        var users = new User
//        {
//            UserName = user.UserName,
//            Password = user.Password,
//            Token = "token"
//        };

//        _context.Users.Add(users);
//        _context.SaveChanges();

//        return Ok("User created");
//    }


//    [HttpPost]
//    public IActionResult Login([FromBody] DtoUser user)
//    {
//        var userExists = _context.Users.Any(x => x.UserName == user.UserName);
//        if (!userExists)
//        {
//            return BadRequest("User does not exist");
//        }

//        var userFromDb = _context.Users.FirstOrDefault(x => x.UserName == user.UserName);

//        if (!BCrypt.Net.BCrypt.Verify(user.Password + key + user.UserName, userFromDb.Password))
//        {
//            return BadRequest("Password is incorrect");
//        }

//        var userId = userFromDb.Id;
//        var userRoles = _context.UserRoles.Where(x => x.UserId == userId).ToList();

//        var permisons = new List<string>();
//        foreach (var userRole in userRoles)
//        {
//            var role = _context.Roles.FirstOrDefault(x => x.Id == userRole.RoleId);
//            var rolePermisons = _context.RolePermissions.Where(x => x.RoleId == role.Id).ToList();
//            foreach (var rolePermison in rolePermisons)
//            {
//                var permison = _context.Permissions.FirstOrDefault(x => x.Id == rolePermison.PermissionId);
//                permisons.Add(permison.Name);
//            }
//        }

//        var tokenModel = GenerateTokens(userFromDb, permisons);

//        return Ok(tokenModel);
//    }


//    [HttpPost]
//    public IActionResult Refresh([FromBody] RefreshTokenModel refreshTokenModel)
//    {
//        var storedRefreshToken = _context.RefreshTokens.SingleOrDefault(x => x.Token == refreshTokenModel.RefreshToken);
//        if (storedRefreshToken == null || storedRefreshToken.ExpiryDate < DateTime.UtcNow)
//        {
//            return Unauthorized();
//        }

//        var userFromDb = _context.Users.SingleOrDefault(x => x.Id == storedRefreshToken.UserId);
//        if (userFromDb == null)
//        {
//            return Unauthorized();
//        }

//        var userId = userFromDb.Id;
//        var userRoles = _context.UserRoles.Where(x => x.UserId == userId).ToList();

//        var permisons = new List<string>();
//        foreach (var userRole in userRoles)
//        {
//            var role = _context.Roles.FirstOrDefault(x => x.Id == userRole.RoleId);
//            var rolePermisons = _context.RolePermissions.Where(x => x.RoleId == role.Id).ToList();
//            foreach (var rolePermison in rolePermisons)
//            {
//                var permison = _context.Permissions.FirstOrDefault(x => x.Id == rolePermison.PermissionId);
//                permisons.Add(permison.Name);
//            }
//        }

//        var newTokenModel = GenerateTokens(userFromDb, permisons);

//        // Invalidate the old refresh token and store the new one
//        _context.RefreshTokens.Remove(storedRefreshToken);
//        _context.RefreshTokens.Add(new RefreshToken
//        {
//            Token = newTokenModel.RefreshToken,
//            UserId = userFromDb.Id,
//            ExpiryDate = DateTime.UtcNow.AddDays(7)
//        });
//        _context.SaveChanges();

//        return Ok(newTokenModel);
//    }


//    private TokenModel GenerateTokens(User user, List<string> permisons)
//    {
//        var tokenHandler = new JwtSecurityTokenHandler();
//        var key = Encoding.ASCII.GetBytes("this is my custom Secret key for authnetication");

//        var claims = new List<Claim>
//        {
//            new Claim(ClaimTypes.Name, user.UserName),
//        };

//        claims.AddRange(permisons.Select(permison => new Claim("permisons", permison)));

//        var tokenDescriptor = new SecurityTokenDescriptor
//        {
//            Subject = new ClaimsIdentity(claims),
//            Expires = DateTime.UtcNow.AddMinutes(15),
//            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//        };

//        var accessToken = tokenHandler.CreateToken(tokenDescriptor);
//        var refreshToken = GenerateRefreshToken();

//        return new TokenModel
//        {
//            AccessToken = tokenHandler.WriteToken(accessToken),
//            RefreshToken = refreshToken
//        };
//    }


//    private string GenerateRefreshToken()
//    {
//        var randomNumber = new byte[32];
//        using (var rng = RandomNumberGenerator.Create())
//        {
//            rng.GetBytes(randomNumber);
//            return Convert.ToBase64String(randomNumber);
//        }
//    }
//}