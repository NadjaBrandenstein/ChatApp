using System.Text.Json;
using api.Dto;
using efscaffold.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StateleSSE.AspNetCore;


[ApiController]
[Route("")]
public class RealtimeController : ControllerBase
{
    private readonly ISseBackplane _backplane;
    private readonly MyDbContext _ctx;
    private readonly JwtService _jwtService;

    public RealtimeController(ISseBackplane backplane, MyDbContext ctx, JwtService jwtService)
    {
        _backplane = backplane;
        _ctx = ctx;
        _jwtService = jwtService;
    }

    [HttpGet("connect")]
    public async Task Connect([FromQuery] string token)
    {
        var username = _jwtService.ValidateToken(token);
        
        if (username == null)
        {
            Response.StatusCode = 401;
            return;
        }
        
        await using var sse = await HttpContext.OpenSseStreamAsync();
        await using var connection = _backplane.CreateConnection();

        await sse.WriteAsync("connected", JsonSerializer.Serialize(new { connection.ConnectionId, username },
            new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));

        await foreach (var evt in connection.ReadAllAsync(HttpContext.RequestAborted))
            await sse.WriteAsync(evt.Group ?? "message", evt.Data);
    }
}