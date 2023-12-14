// CuentasController.cs

using LoginBackend.Models;
using LoginBackend20233.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace LoginBackend20233.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ApplicationDbContext context;

        public CuentasController(UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            SignInManager<IdentityUser> signInManager, ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.context = context;
        }

        [HttpPost("registrar")]
        public async Task<ActionResult<RespuestaAutenticacion>> Registrar(CredencialesUsuario credencialesUsuario)
        {
            var usuario = new IdentityUser
            {
                UserName = credencialesUsuario.Email,
                Email = credencialesUsuario.Email
            };
            var resultado = await userManager.CreateAsync(usuario, credencialesUsuario.Password);
            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }
            return BadRequest("Error al registrar el usuario: " + string.Join(", ", resultado.Errors));
        }

        private async Task<RespuestaAutenticacion> ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            var claims = new List<Claim>()
            {
                new Claim("email", credencialesUsuario.Email),
            };

            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);
            var claimsRoles = await userManager.GetClaimsAsync(usuario);

            claims.AddRange(claimsRoles);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["LlaveJWT"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddDays(1);

            return new RespuestaAutenticacion();
            
        }

        [HttpGet("RenovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutenticacion>> Renovar()
        {
            var emailClaim = HttpContext.User?.Claims?.FirstOrDefault(x => x.Type == "email");
            if (emailClaim != null)
            {
                var credencialesUsuario = new CredencialesUsuario()
                {
                    Email = emailClaim.Value
                };
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest("No se pudo obtener la información del usuario para renovar el token");
            }

        }

        [HttpPost("Login")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login(CredencialesUsuario credencialesUsuario)
        {
            var resultado = await signInManager.PasswordSignInAsync(credencialesUsuario.Email,
                credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest("Credenciales de inicio de sesión incorrectas");
            }
        }


        //--------------------------------------------------------------------------------------------------------//
        [HttpGet("ObtenerUserId/{email}")]
        public async Task<ActionResult<string>> ObtenerUserId(string email)
        {
            try
            {
                var usuario = await userManager.FindByEmailAsync(email);

                if (usuario != null)
                {
                    return Ok(new { UserId = usuario.Id });
                }
                else
                {
                    return NotFound(new { error = "Usuario no encontrado" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor", detalles = ex.Message });
            }
        }


        [HttpGet("Favoritos/{userId}")]
        public IActionResult ObtenerFavoritos(string userId)
        {
            try
            {
                var Favoritos = context.Favoritos
                    .Where(p => p.UserId == userId)
                    .ToList();

                return Ok(Favoritos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor", detalles = ex.Message });
            }

        }

        [HttpPost("Favoritos")]
        public async Task<ActionResult> AgregarFavoritoAsync([FromBody] Favoritos favorito)
        {
            try
            {
                if (favorito == null)
                {
                    return BadRequest(new { error = "Datos inválidos" });
                }

                var Existente = context.Favoritos
                    .FirstOrDefault(p => p.UserId == favorito.UserId && p.idMeal == favorito.idMeal);

                if (Existente != null)
                {
                    return BadRequest(new { error = "Ya está en la lista de favoritos" });
                }

                context.Favoritos.Add(favorito);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor", detalles = ex.Message });
            }
        }

        [HttpDelete("Favoritos")]
        public async Task<ActionResult> EliminarFavoritoAsync([FromBody] Favoritos favorito)
        {
            try
            {
                if (favorito == null)
                {
                    return BadRequest(new { error = "Datos inválidos" });
                }

                var Existente = context.Favoritos
                    .FirstOrDefault(p => p.UserId == favorito.UserId && p.idMeal == favorito.idMeal);

                if (Existente == null)
                {
                    return NotFound(new { error = "No se encuentra en la lista de favoritos" });
                }

                context.Favoritos.Remove(Existente);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor", detalles = ex.Message });
            }
        }
        

    }
}

