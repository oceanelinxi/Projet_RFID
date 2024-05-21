using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MLnew.Data;
using MLnew.Models;

namespace MLnew.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectionHistoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ConnectionHistoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ConnectionHistories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConnectionHistory>>> GetConnectionHistory()
        {
          if (_context.ConnectionHistory == null)
          {
              return NotFound();
          }
            return await _context.ConnectionHistory.ToListAsync();
        }

        // GET: api/ConnectionHistories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ConnectionHistory>> GetConnectionHistory(int id)
        {
          if (_context.ConnectionHistory == null)
          {
              return NotFound();
          }
            var connectionHistory = await _context.ConnectionHistory.FindAsync(id);

            if (connectionHistory == null)
            {
                return NotFound();
            }

            return connectionHistory;
        }

        // PUT: api/ConnectionHistories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutConnectionHistory(int id, ConnectionHistory connectionHistory)
        {
            if (id != connectionHistory.Id)
            {
                return BadRequest();
            }

            _context.Entry(connectionHistory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConnectionHistoryExists(id))
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

        // POST: api/ConnectionHistories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ConnectionHistory>> PostConnectionHistory(ConnectionHistory connectionHistory)
        {
          if (_context.ConnectionHistory == null)
          {
              return Problem("Entity set 'ApplicationDbContext.ConnectionHistory'  is null.");
          }
            _context.ConnectionHistory.Add(connectionHistory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetConnectionHistory", new { id = connectionHistory.Id }, connectionHistory);
        }

        // DELETE: api/ConnectionHistories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConnectionHistory(int id)
        {
            if (_context.ConnectionHistory == null)
            {
                return NotFound();
            }
            var connectionHistory = await _context.ConnectionHistory.FindAsync(id);
            if (connectionHistory == null)
            {
                return NotFound();
            }

            _context.ConnectionHistory.Remove(connectionHistory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConnectionHistoryExists(int id)
        {
            return (_context.ConnectionHistory?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
