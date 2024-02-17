using mf_apis_web_services_fuel_manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace mf_apis_web_services_fuel_manager.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        //configuração do banco de dados
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        //rota de index para receber todos os usuarios cadastrados
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var model = await _context.Usuarios.ToListAsync();

            return Ok(model);
        }

        //para criar um novo usuario
        [HttpPost]

        public async Task<ActionResult> Create(UsuarioDto model)
        {
            Usuario novo = new Usuario()
            {
                Nome = model.Nome,
                //Criptografando a senha
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Perfil = model.Perfil
            };            
            
            _context.Usuarios.Add(novo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetById", new { id = novo.Id }, novo);
        }

        //recuperação de dados
        [HttpGet("{id}")]

        public async Task<ActionResult> GetById(int id)
        {
            var model = await _context.Usuarios
              
                .FirstOrDefaultAsync(c => c.Id == id);

            if (model == null) return NotFound();

            
            return Ok(model);
        }

        //alteração de dados
        [HttpPut("{id}")]

        public async Task<ActionResult> Update(int id, UsuarioDto model)
        {
            if (id != model.Id) return BadRequest();

            var modeloDb = await _context.Usuarios.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (modeloDb == null) return NotFound();

            modeloDb.Nome = model.Nome;
            modeloDb.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            modeloDb.Perfil = model.Perfil;

            _context.Usuarios.Update(modeloDb);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //deletar dados
        [HttpDelete("{id}")]

        public async Task<ActionResult> Delete(int id)
        {
            var model = await _context.Usuarios.FindAsync(id);
            if (model == null) return NotFound();

            _context.Usuarios.Remove(model);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //rota de autenticação
        [AllowAnonymous]
        [HttpPost("authenticate")]

        public async Task<ActionResult> Authenticate(AuthenticateDto model)
        {
            var usuarioDb = await _context.Usuarios.FindAsync(model.Id);
            if (usuarioDb == null) return NotFound();

            if(usuarioDb == null || !BCrypt.Net.BCrypt.Verify(model.Password, usuarioDb.Password))
                return Unauthorized();

            var jwt = GenerateJwtToken(usuarioDb);

            return Ok(new { jwtToken = jwt});
        }

        private string GenerateJwtToken(Usuario model)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("Ry74cBQva5dThwbwchR9jhbtRFnJxWSZ");
            var claims = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, model.Id.ToString()),
                new Claim(ClaimTypes.Role, model.Perfil.ToString())
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
