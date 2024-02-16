﻿using mf_apis_web_services_fuel_manager.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace mf_apis_web_services_fuel_manager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsumosController : ControllerBase
    {
            //configuração do banco de dados
            private readonly AppDbContext _context;

            public ConsumosController(AppDbContext context)
            {
                _context = context;
            }

            //rota de index para receber todos os carros cadastrados
            [HttpGet]
            public async Task<ActionResult> GetAll()
            {
                var model = await _context.Consumos.ToListAsync();

                return Ok(model);
            }

            //para criar um novo veiculo
            [HttpPost]

            public async Task<ActionResult> Create(Consumo model)
            {

                _context.Consumos.Add(model);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetById", new { id = model.Id }, model);
            }

            //recuperação de dados
            [HttpGet("{id}")]

            public async Task<ActionResult> GetById(int id)
            {
                var model = await _context.Consumos
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (model == null) return NotFound();

                return Ok(model);
            }

            //alteração de dados
            [HttpPut("{id}")]

            public async Task<ActionResult> Update(int id, Consumo model)
            {
                if (id != model.Id) return BadRequest();

                var modeloDb = await _context.Consumos.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (modeloDb == null) return NotFound();

                _context.Consumos.Update(model);
                await _context.SaveChangesAsync();

                return NoContent();
            }

            //deletar dados
            [HttpDelete("{id}")]

            public async Task<ActionResult> Delete(int id)
            {
                var model = await _context.Consumos.FindAsync(id);
                if (model == null) return NotFound();

                _context.Consumos.Remove(model);
                await _context.SaveChangesAsync();

                return NoContent();
            }
    }
}
