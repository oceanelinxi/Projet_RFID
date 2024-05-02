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
    public class MethodesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MethodesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Methodes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Methode>>> GetMethode()
        {
          if (_context.Methode == null)
          {
              return NotFound();
          }
            return await _context.Methode.ToListAsync();
        }

        // GET: api/Methodes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Methode>> GetMethode(int id)
        {
          if (_context.Methode == null)
          {
              return NotFound();
          }
            var methode = await _context.Methode.FindAsync(id);

            if (methode == null)
            {
                return NotFound();
            }

            return methode;
        }

        // PUT: api/Methodes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMethode(int id, Methode methode)
        {
            if (id != methode.Id)
            {
                return BadRequest();
            }

            _context.Entry(methode).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MethodeExists(id))
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

        // POST: api/Methodes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Methode>> PostMethode(Methode methode)
        {
          if (_context.Methode == null)
          {
              return Problem("Entity set 'ApplicationDbContext.Methode'  is null.");
          }
            _context.Methode.Add(methode);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMethode", new { id = methode.Id }, methode);
        }

        // DELETE: api/Methodes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMethode(int id)
        {
            if (_context.Methode == null)
            {
                return NotFound();
            }
            var methode = await _context.Methode.FindAsync(id);
            if (methode == null)
            {
                return NotFound();
            }

            _context.Methode.Remove(methode);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MethodeExists(int id)
        {
            return (_context.Methode?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
