using mf_apis_web_services_fuel_manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace mf_apis_web_services_fuel_manager.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VeiculosController : ControllerBase
    {
        //configuração do banco de dados
        private readonly AppDbContext _context;

        public VeiculosController(AppDbContext context)
        {
            _context = context;
        }

        //rota de index para receber todos os carros cadastrados
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var model = await _context.Veiculos.ToListAsync();

            return Ok(model);
        }

        //para criar um novo veiculo
        [HttpPost]

        public async Task<ActionResult> Create(Veiculo model)
        {
            if(model.AnoFabricacao <= 0 || model.AnoModelo <= 0)
            {
                return BadRequest(new { message = "Ano de Fabricação e ano do Modelo são obrigatório e devem ser maiores que zero" });
            }

            _context.Veiculos.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetById", new {id = model.Id}, model);
        }

        //recuperação de dados
        [HttpGet("{id}")]

        public async Task<ActionResult> GetById(int id)
        {
            var model = await _context.Veiculos
                .Include(t => t.Usuarios).ThenInclude(t=>t.Usuario)
                .Include(t => t.Consumos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (model == null) return NotFound();

            GerarLinks(model);
            return Ok(model);
        }

        //alteração de dados
        [HttpPut("{id}")]

        public async Task<ActionResult> Update(int id, Veiculo model)
        {
            if (id != model.Id) return BadRequest();

            var modeloDb = await _context.Veiculos.AsNoTracking()
                .FirstOrDefaultAsync (c => c.Id == id);

            if (modeloDb == null) return NotFound();

            _context.Veiculos.Update(model);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //deletar dados
        [HttpDelete("{id}")]

        public async Task<ActionResult> Delete(int id)
        {
            var model = await _context.Veiculos.FindAsync(id);
            if(model == null) return NotFound();

            _context.Veiculos.Remove(model);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //metodos para preencher os links
        private void GerarLinks(Veiculo model)
        {
            model.Links.Add(new LinkDto(model.Id, Url.ActionLink() , rel: "self", metodo: "GET"));
            model.Links.Add(new LinkDto(model.Id, Url.ActionLink(), rel: "update", metodo: "PUT"));
            model.Links.Add(new LinkDto(model.Id, Url.ActionLink(), rel: "delete", metodo: "Delete"));
        }

        //rotas para adicionar um usuario a um determinado veiculo
        [HttpPost("{id}/usuarios")]
        public async Task<ActionResult> AddUsuario(int id, VeiculoUsuario model)
        {
            if (id != model.VeiculoId) return BadRequest();

            _context.VeiculoUsuarios.Add(model);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetById", new { id = model.VeiculoId }, model);
        }

        [HttpDelete("{id}/usuarios/{usuarioId}")]
        public async Task<ActionResult> DeleteUsuario(int id, int usuarioId)
        {
            var model = await _context.VeiculoUsuarios
                .Where(c => c.VeiculoId == id && c.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (model == null) return NotFound();

            _context.VeiculoUsuarios.Remove(model);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
