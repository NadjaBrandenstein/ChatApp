using api.Dto;
using api.Dto.Request;
using api.Dto.Response;
using efscaffold.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StateleSSE.AspNetCore;

namespace api.Controller;

[ApiController]
[Route("chat")]
public class ChatController : ControllerBase
{
    private readonly ISseBackplane _backplane;
    private readonly MyDbContext _ctx;

    public ChatController(ISseBackplane backplane, MyDbContext ctx)
    {
        _backplane = backplane;
        _ctx = ctx;
    }
    
    [Authorize]
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] ChatMessageDto dto)
    {
        var username = User.Identity!.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized("User not found");
        }
        
        var user = await _ctx.Logins
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            return BadRequest("User does not exist");
        }

        var room = await _ctx.Rooms
            .FirstOrDefaultAsync(r => r.Roomname == dto.room);

        if (room == null)
        {
            return BadRequest("Room does not exist");
        }

        var message = new Message
        {
            Content = dto.message,
            Sentat = DateTime.UtcNow,
            Senderuserid = user.Userid,
            Roomid = room.Roomid,
            Recipientuserid = null
        };
        
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"{claim.Type} = {claim.Value}");
        }

        _ctx.Messages.Add(message);
        await _ctx.SaveChangesAsync();
        
        await _backplane.Clients.SendToGroupAsync(
            dto.room,
            new ChatResponse(username, dto.message)
        );

        return Ok();
    }
    
    [HttpPost("join")]
    public async Task Join([FromBody] JoinDto dto)
    {
        await _backplane.Groups.AddToGroupAsync(dto.connectionId, dto.room);
        await _backplane.Clients.SendToGroupAsync(dto.room, new JoinResponse("Someone has entered the chat!"));
    }

    [HttpPost("leave")]
    public async Task Leave(string roomId, string connectionId)
    {
        await _backplane.Groups.RemoveFromGroupAsync(connectionId, roomId);
    }

    [HttpPost("typing")]
    public async Task Typing([FromBody] TypingRequest request)
    {
        await _backplane.Clients.SendToGroupAsync(
            request.Room,
            new TypingResponse("Someone", request.IsTyping)
        );
    }
    
    [HttpPost("poke")]
    public async Task Poke(string connectionId)
    {
        await _backplane.Clients.SendToClientAsync(connectionId, new PokeResponse("You have been poked!"));
    }
    
}