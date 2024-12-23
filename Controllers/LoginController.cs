﻿using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SFT.Model;
using SFT.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using Microsoft.AspNetCore.Authorization;
using SFT.Model.NonDBModel;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SFT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly ILoggerService _logger;

        public LoginController(IConfiguration configuration, AppDbContext context, ILoggerService logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }


        private string GenerateToken(UserAuth user)
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.User_ID.ToString()), // Assuming UserID is the unique identifier
                new Claim("AutoGenPass", user.AutoGenPass.ToString()), // Convert bool to string
            };

            var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials
        );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Authorize]
        [HttpPost("Extend")]
        public IActionResult Extend()
        {
            IActionResult response = Unauthorized();

            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var userauth = _context.UserAuthentication.FirstOrDefault(i => i.User_ID == userId);
                if (userauth != null)
                {
                    var token = GenerateToken(userauth);

                    _logger.LogEvent($"User-Login Extended", "Login", userauth.User_ID);
                    response = Ok(new { token = token, userauth.User_ID, userauth.AutoGenPass });

                }
            }
            return response;
        }

        //Login api 
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] UserLogin model)
        {
            var userAuth = (from user in _context.Users
                            join ua in _context.UserAuthentication on user.User_ID equals ua.User_ID
                            where user.EmailAddress == model.Email
                            select new { ua, user.Status }).FirstOrDefault();

            if (userAuth == null)
            {
                return NotFound("User not found");
            }

            if (!userAuth.Status)
            {
                return Unauthorized("User is inactive");
            }

            string hashedPassword = Sha256Hasher.ComputeSHA256Hash(model.Password);
            Console.WriteLine(hashedPassword);

            if (hashedPassword != userAuth.ua.Password)
            {
                return Unauthorized("Invalid password");
            }

            var token = GenerateToken(userAuth.ua);

            _logger.LogEvent($"User Logged-in", "Login", userAuth.ua.User_ID);
            return Ok(new { token = token, userAuth.ua.User_ID, userAuth.ua.AutoGenPass });
        }


        [HttpPut("Forgotpassword")]
        public IActionResult ResetPassword([FromBody] LoginRequest user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var users = _context.Users.FirstOrDefault(u => u.EmailAddress == user.Email);

            if (users == null)
            {
                return NotFound("User not found");
            }

            var userauth = _context.UserAuthentication.FirstOrDefault(i => i.User_ID == users.User_ID);

            if (userauth == null)
            {
                return NotFound("User Authentication Data Not Found");
            }

            string newPassword = PasswordGen.GeneratePassword();

            string hashedPassword = Sha256Hasher.ComputeSHA256Hash(newPassword);

            userauth.Password = hashedPassword;

            userauth.AutoGenPass = true;

            _context.SaveChanges();

            string emailBody = $@"
                <div style=""text-align: center; background-color: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); border: 2px solid black; min-width: 200px; max-width: 300px; width: 100%; margin: 50px auto;"">
                    <h2 style=""color: blue;"">New Login Credentials <hr /></h2>
                    <p>
                        <strong>Username:</strong><br /> {users.EmailAddress}
                    </p>
                    <p>
                        <strong>Password:</strong><br /> {newPassword}
                    </p>
                    <p style=""color: #F00;"">
                        Please change the password immediately after login.
                    </p>
                    <a href=""#"" style=""display: inline-block; padding: 10px 20px; background-color: #007BFF; color: #fff; text-decoration: none; border-radius: 5px; margin-top: 15px;"">Login Here</a>
                </div>";
            string result = new EmailService(_context, _logger, _configuration).SendEmail(users.EmailAddress, "Reset-Password", emailBody);
            /*var emailservice = new EmailService(_configuration);
            var result = emailservice.SendEmail(users.EmailAddress, "Reset-Password", emailBody);*/
            _logger.LogEvent("Password-Reset", "Login", users.User_ID);
            return Ok(new { result });


        }
        [Authorize]
        [HttpPut("Changepassword/{id}")]
        public IActionResult ChangePassword(int id, Model.NonDBModel.ChangePassword cred)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            string oldHashPass = Sha256Hasher.ComputeSHA256Hash(cred.OldPassword);
            var userauth = _context.UserAuthentication.FirstOrDefault(i => i.User_ID == id);

            if (userauth == null)
            {
                return NotFound("User Authentication Data Not Found");
            }
            if (userauth.Password != oldHashPass)
            {
                return BadRequest("Existing Password Invalid");
            }

            string newPassword = cred.NewPassword;

            string hashedPassword = Sha256Hasher.ComputeSHA256Hash(newPassword);

            userauth.Password = hashedPassword;

            userauth.AutoGenPass = false;

            _context.SaveChanges();
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                // Now you have the user ID
                _logger.LogEvent("Password-Changed", "Login", userId);
            }

            return Ok(new { newPassword, hashedPassword });


        }
    }
}
