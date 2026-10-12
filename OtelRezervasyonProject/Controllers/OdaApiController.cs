using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtelRezervasyonProject.Data;

namespace OtelRezervasyonProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OdaApiController : ControllerBase
    {
        private readonly OtelDbContext _context;
        public OdaApiController(OtelDbContext context) { _context = context; }

        // GET: api/OdaApi -> Tüm odaları JSON formatında listeler
        [HttpGet]
        public async Task<IActionResult> GetOdalar()
        {
            var odalar = await _context.Odalar.Include(o => o.OdaTipi).ToListAsync();
            return Ok(odalar); // REST API dönüşü
        }
    }
}