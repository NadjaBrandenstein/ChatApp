using api.Dto;
using efscaffold.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controller;

[ApiController]
[Route("rooms")]
public class RoomController : ControllerBase
{
    private readonly MyDbContext _ctx;

    public RoomController(MyDbContext ctx)
    {
        _ctx = ctx;
    }
    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await _ctx.Rooms
            .Select(r => r.Roomname)
            .ToListAsync();
        
        return Ok(rooms);
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto dto)
    {
        if (await _ctx.Rooms.AnyAsync(r => r.Roomname == dto.RoomName))
        {
            return BadRequest("Room already exists");
        }

        _ctx.Rooms.Add(new Room { Roomname = dto.RoomName });
        await _ctx.SaveChangesAsync();
        return Ok();
    }
    
}