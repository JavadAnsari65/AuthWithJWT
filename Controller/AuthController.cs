using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

//route ["Action"] is the name of the method

[Route("[Action]")]

//apicontroller
[ApiController]

public class AuthController : Controller
{

    private readonly Context _context;

    //haradcode key 
    private string key = "this is my custom Secret key for authnetication";

    public AuthController(Context context)
    {
        _context = context;
    }

    //Register user in database input username and password
    [HttpPost]
    public IActionResult Register([FromBody] DtoUser user)
    {

        //check if user exists
        var userExists = _context.Users.Any(x => x.UserName == user.UserName);
        if (userExists)
        {
            return BadRequest("User already exists");
        }

        //Hash password with BCrypt
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password+key+user.UserName);

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

    //Login user input username and password
    [HttpPost]
    public IActionResult Login([FromBody] DtoUser user)
    {
        //check if user exists
        var userExists = _context.Users.Any(x => x.UserName == user.UserName);
        if (!userExists)
        {
            return BadRequest("User does not exist");
        }

        //get user from database
        var userFromDb = _context.Users.FirstOrDefault(x => x.UserName == user.UserName);

        //check if password is correct
        if (!BCrypt.Net.BCrypt.Verify(user.Password+key+user.UserName, userFromDb.Password))
        {
            return BadRequest("Password is incorrect");
        }

        //userid
        var userId = userFromDb.Id;
        //list Role id
        var userRoles = _context.UserRoles.Where(x => x.UserId == userId).ToList();

        //get list all permisons by role
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

        //generate token
        var token = GenerateToken(userFromDb,permisons);

        return Ok(token);
    }


    //generate token
    private string GenerateToken(User user , List<string> permisons)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("this is my custom Secret key for authnetication");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            
        };

        //claim permisons
        claims.AddRange(permisons.Select(permison => new Claim("permisons", permison)));




        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
        
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
