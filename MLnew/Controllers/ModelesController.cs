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
    public class ModelesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ModelesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Modeles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Modele>>> GetModele()
        {
          if (_context.Modele == null)
          {
              return NotFound();
          }
            return await _context.Modele.ToListAsync();
        }

        // GET: api/Modeles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Modele>> GetModele(int id)
        {
          if (_context.Modele == null)
          {
              return NotFound();
          }
            var modele = await _context.Modele.FindAsync(id);

            if (modele == null)
            {
                return NotFound();
            }

            return modele;
        }

        // PUT: api/Modeles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutModele(int id, Modele modele)
        {
            if (id != modele.ModeleID)
            {
                return BadRequest();
            }

            _context.Entry(modele).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ModeleExists(id))
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

        // POST: api/Modeles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Modele>> PostModele(Modele modele)
        {
          if (_context.Modele == null)
          {
              return Problem("Entity set 'ApplicationDbContext.Modele'  is null.");
          }
            _context.Modele.Add(modele);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetModele", new { id = modele.ModeleID }, modele);
        }

        // DELETE: api/Modeles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModele(int id)
        {
            if (_context.Modele == null)
            {
                return NotFound();
            }
            var modele = await _context.Modele.FindAsync(id);
            if (modele == null)
            {
                return NotFound();
            }

            _context.Modele.Remove(modele);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ModeleExists(int id)
        {
            return (_context.Modele?.Any(e => e.ModeleID == id)).GetValueOrDefault();
        }
    }
}
