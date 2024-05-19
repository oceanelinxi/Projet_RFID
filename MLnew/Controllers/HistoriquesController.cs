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
    public class HistoriquesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HistoriquesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Historiques
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Historique>>> GetHistorique()
        {
          if (_context.Historique == null)
          {
              return NotFound();
          }
            return await _context.Historique.ToListAsync();
        }

        // GET: api/Historiques/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Historique>> GetHistorique(int id)
        {
          if (_context.Historique == null)
          {
              return NotFound();
          }
            var historique = await _context.Historique.FindAsync(id);

            if (historique == null)
            {
                return NotFound();
            }

            return historique;
        }

        // PUT: api/Historiques/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHistorique(int id, Historique historique)
        {
            if (id != historique.HistoriqueID)
            {
                return BadRequest();
            }

            _context.Entry(historique).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HistoriqueExists(id))
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

        // POST: api/Historiques
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Historique>> PostHistorique(Historique historique)
        {
          if (_context.Historique == null)
          {
              return Problem("Entity set 'ApplicationDbContext.Historique'  is null.");
          }
            _context.Historique.Add(historique);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetHistorique", new { id = historique.HistoriqueID }, historique);
        }

        // DELETE: api/Historiques/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHistorique(int id)
        {
            if (_context.Historique == null)
            {
                return NotFound();
            }
            var historique = await _context.Historique.FindAsync(id);
            if (historique == null)
            {
                return NotFound();
            }

            _context.Historique.Remove(historique);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool HistoriqueExists(int id)
        {
            return (_context.Historique?.Any(e => e.HistoriqueID == id)).GetValueOrDefault();
        }
    }
}
