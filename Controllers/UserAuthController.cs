using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFT.Model;
using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SFT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserAuthsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserAuths
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserAuth>>> GetUserAuthentication()
        {
            if (_context.UserAuthentication == null)
            {
                return NotFound();
            }
            return await _context.UserAuthentication.ToListAsync();
        }

        // GET: api/UserAuths/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserAuth>> GetUserAuth(int id)
        {
            if (_context.UserAuthentication == null)
            {
                return NotFound();
            }
            var userAuth = await _context.UserAuthentication.FindAsync(id);

            if (userAuth == null)
            {
                return NotFound();
            }

            return userAuth;
        }

        // PUT: api/UserAuths/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserAuth(int id, UserAuth userAuth)
        {
            if (id != userAuth.User_Auth_Id)
            {
                return BadRequest();
            }

            _context.Entry(userAuth).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserAuthExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // DELETE: api/UserAuths/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAuth(int id)
        {
            if (_context.UserAuthentication == null)
            {
                return NotFound();
            }
            var userAuth = await _context.UserAuthentication.FindAsync(id);
            if (userAuth == null)
            {
                return NotFound();
            }

            _context.UserAuthentication.Remove(userAuth);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserAuthExists(int id)
        {
            return (_context.UserAuthentication?.Any(e => e.User_Auth_Id == id)).GetValueOrDefault();
        }
    }
}
