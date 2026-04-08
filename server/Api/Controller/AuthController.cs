using api.Dto;
using efscaffold.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StateleSSE.AspNetCore;

namespace api.Controller;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly MyDbContext _ctx;
    private readonly JwtService _jwtService;

    public AuthController(MyDbContext ctx, JwtService jwtService)
    {
        _ctx = ctx;
        _jwtService = jwtService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register(AuthDto dto)
    {
        if (await _ctx.Logins.AnyAsync(x => x.Username == dto.Username))
            return BadRequest("User exists");

        var hasher = new PasswordHasher<Login>();

        var user = new Login
        {
            Username = dto.Username,
            Password = hasher.HashPassword(null!, dto.Password),
            Roleid = 1
        };

        user.Password = hasher.HashPassword(user, dto.Password);

        _ctx.Logins.Add(user);
        await _ctx.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthDto dto)
    {
        var user = await _ctx.Logins
            .FirstOrDefaultAsync(x => x.Username == dto.Username);

        if (user == null)
            return Unauthorized();

        var hasher = new PasswordHasher<Login>();

        var result = hasher.VerifyHashedPassword(user,
            user.Password,
            dto.Password);

        if (result == PasswordVerificationResult.Failed)
            return Unauthorized();

        var token = _jwtService.GenerateToken(user.Username);

        return Ok(new { token });
    }
    
}