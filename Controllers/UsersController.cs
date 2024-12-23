﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFT;
using SFT.Model;
using SFT.Services;



namespace SFT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILoggerService _logger;

        public UsersController(AppDbContext context, IConfiguration configuration, ILoggerService logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // GET: api/Users
        [Authorize]
        [HttpGet("GetUsers")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {

            if (_context.Users == null)
            {
                return NotFound();
            }
            return await _context.Users.ToListAsync();
        }
        // GET: api/Users/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.User_ID)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    // Now you have the user ID
                    _logger.LogEvent($"User {user.User_ID} Updated ", "User", userId);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Error occurred while updating User", "DbUpdateConcurrencyException", "UsersController");
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Users.Add(user);
            // Generate password

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserExists(user.User_ID))
                {
                    return Conflict();
                }
                else
                {
                    _logger.LogError("Error occurred while Adding User", "DbUpdateException", "UsersController");
                    throw;
                }
            }

            int generatedUserId = user.User_ID;

            string generatedPassword = PasswordGen.GeneratePassword();

            string hashedPassword = Sha256Hasher.ComputeSHA256Hash(generatedPassword);

            UserAuth userAuth = new UserAuth
            {
                User_ID = generatedUserId,
                Password = hashedPassword,
                AutoGenPass = true
            };
            _context.UserAuthentication.Add(userAuth);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserExists(generatedUserId))
                {
                    return Conflict();
                }
                else
                {
                    _logger.LogError("Error occurred while Adding UserAuthentication", "DbUpdateException", "CoursesController");
                    throw;
                }
            }
            string emailBody = $@"
                <div style=""text-align: center; background-color: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); border: 2px solid black; min-width: 200px; max-width: 300px; width: 100%; margin: 50px auto;"">
                    <h2 style=""color: blue;"">Login Credentials <hr /></h2>
                     <p>
                        <strong>Username:</strong><br /> {user.EmailAddress}
                    </p>
                    <p>
                        <strong>Password:</strong><br /> {generatedPassword}
                    </p>
                    <p style=""color: #F00;"">
                        Please change the password immediately after login.
                    </p>
                    <a href=""http://keygen.chandrakala.co.in/"" style=""display: inline-block; padding: 10px 20px; background-color: #007BFF; color: #fff; text-decoration: none; border-radius: 5px; margin-top: 15px;"">Login Here</a>
                </div>";

            var result = new EmailService(_context, _logger, _configuration).SendEmail(user.EmailAddress, "Welcome to CUPL!", emailBody);
            /*var emailservice = new EmailService(_configuration);
            var result = emailservice.SendEmail(user.EmailAddress, "Welcome to CUPL!", emailBody);*/
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                // Now you have the user ID
                _logger.LogEvent($"User: {user.User_ID} Added ", "User", userId);
            }
            return Ok(new { user.User_ID, user, result });
        }

        // DELETE: api/Users/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var userauth = await _context.UserAuthentication.FirstOrDefaultAsync(u => u.User_ID == user.User_ID);

            _context.Users.Remove(user);
            _context.UserAuthentication.Remove(userauth);

            await _context.SaveChangesAsync();
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                // Now you have the user ID
                _logger.LogEvent($"User: {user.User_ID} Deleted ", "User", userId);
            }

            return NoContent();
        }

        [Authorize]
        [HttpPost("upload/{userId}")]
        public IActionResult UploadImage(int userId)
        {
            try
            {
                // Ensure the user with the provided userId exists
                if (UserExists(userId))
                {
                    var file = Request.Form.Files[0];

                    if (file.Length > 0)
                    {
                        // Extract the file extension from the original filename
                        var fileExtension = Path.GetExtension(file.FileName);

                        // Generate the custom filename based on the user ID and original file extension
                        var customFileName = $"{userId}_profilepic{fileExtension}";

                        var filePath = Path.Combine("wwwroot/images", customFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        // Update the user's profile picture path in the database
                        var user = _context.Users.FirstOrDefault(u => u.User_ID == userId);
                        if (user != null)
                        {
                            user.ProfilePicturePath = $"images/{customFileName}";
                            _context.SaveChanges();
                        }

                        return Ok(new { message = "Image uploaded successfully", filePath });
                    }
                    else
                    {
                        return BadRequest("No file uploaded");
                    }
                }
                else
                {
                    return BadRequest("Invalid user");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while Uploading image", $"{ex.Message}", "UsersController");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.User_ID == id);
        }
    }
}