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
    public class ParametresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ParametresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Parametres
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Parametre>>> GetParametre()
        {
          if (_context.Parametre == null)
          {
              return NotFound();
          }
            return await _context.Parametre.ToListAsync();
        }

        // GET: api/Parametres/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Parametre>> GetParametre(int id)
        {
          if (_context.Parametre == null)
          {
              return NotFound();
          }
            var parametre = await _context.Parametre.FindAsync(id);

            if (parametre == null)
            {
                return NotFound();
            }

            return parametre;
        }

        // PUT: api/Parametres/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutParametre(int id, Parametre parametre)
        {
            if (id != parametre.ParametreID)
            {
                return BadRequest();
            }

            _context.Entry(parametre).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParametreExists(id))
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

        // POST: api/Parametres
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Parametre>> PostParametre(Parametre parametre)
        {
          if (_context.Parametre == null)
          {
              return Problem("Entity set 'ApplicationDbContext.Parametre'  is null.");
          }
            _context.Parametre.Add(parametre);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetParametre", new { id = parametre.ParametreID }, parametre);
        }

        // DELETE: api/Parametres/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParametre(int id)
        {
            if (_context.Parametre == null)
            {
                return NotFound();
            }
            var parametre = await _context.Parametre.FindAsync(id);
            if (parametre == null)
            {
                return NotFound();
            }

            _context.Parametre.Remove(parametre);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ParametreExists(int id)
        {
            return (_context.Parametre?.Any(e => e.ParametreID == id)).GetValueOrDefault();
        }
    }
}
