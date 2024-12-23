﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFT.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SFT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LogsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/EventLogs
        [Route("Events")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventLog>>> GetEventLogs()
        {
            return await _context.EventLogs.ToListAsync();
        }

        [Route("Errors")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ErrorLog>>> GetErrorLogs()
        {
            return await _context.ErrorLogs.ToListAsync();
        }

        // GET: api/EventLogs/5
        [HttpGet("Events/Category")]
        public async Task<ActionResult<IQueryable<EventLog>>> GetEventLogbyCategory(string name)
        {
            // Query the database for EventLogs by name
            var eventLogs = await _context.EventLogs.Where(e => e.Category == name).ToListAsync();

            // Execute the query asynchronously

            if (eventLogs == null || eventLogs.Count == 0)
            {
                return NotFound();
            }

            return Ok(eventLogs.AsQueryable());
        }

        [HttpGet("Error/OccuranceSpace")]
        public async Task<ActionResult<IQueryable<ErrorLog>>> GetErrorLogbyOccuranceSpace(string name)
        {
            // Query the database for EventLogs by name
            var errorLogs = await _context.ErrorLogs.Where(e => e.OccuranceSpace == name).ToListAsync();

            // Execute the query asynchronously

            if (errorLogs == null || errorLogs.Count == 0)
            {
                return NotFound();
            }

            return Ok(errorLogs.AsQueryable());
        }

        [HttpGet("Events/{ID}")]
        public async Task<ActionResult<IQueryable<EventLog>>> GetEventLogbyUserID(int ID)
        {
            // Query the database for EventLogs by name
            var eventLogs = await _context.EventLogs.Where(e => e.EventTriggeredBy == ID).ToListAsync();


            if (eventLogs == null || eventLogs.Count == 0)
            {
                return NotFound();
            }

            return Ok(eventLogs.AsQueryable());
        }

        [HttpGet("Events/User/{ID}/Count/{count}")]
        public async Task<ActionResult<IEnumerable<EventLog>>> GetEventLogsByUserIDAndCount(int ID, int count)
        {
            // Query the database for EventLogs by ID, order by timestamp, and take the specified number of logs
            var eventLogs = await _context.EventLogs
                .Where(e => e.EventTriggeredBy == ID)
                .OrderByDescending(e => e.LoggedAT)
                .Take(count)
                .ToListAsync();

            if (eventLogs == null || eventLogs.Count == 0)
            {
                return NotFound();
            }

            return Ok(eventLogs);
        }


        [HttpGet("Events/{ID}/{category}")]
        public async Task<ActionResult<IQueryable<EventLog>>> GetEventLogbyUserIDCategory(int ID, string category)
        {
            // Query the database for EventLogs by name
            var eventLogs = await _context.EventLogs.Where(e => e.EventTriggeredBy == ID && e.Category == category).ToListAsync();

            if (eventLogs == null || eventLogs.Count == 0)
            {
                return NotFound();
            }

            return Ok(eventLogs.AsQueryable());
        }
        // DELETE: api/EventLogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEventLog(int id)
        {
            var eventLog = await _context.EventLogs.FindAsync(id);
            if (eventLog == null)
            {
                return NotFound();
            }

            _context.EventLogs.Remove(eventLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
