using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("messages")]
public class MessageController : ControllerBase
{
    private readonly MyDbContext _ctx;

    public MessageController(MyDbContext ctx)
    {
        _ctx = ctx;
    }
    
    [HttpGet("messages/{roomName}")]
    public async Task<ActionResult> GetLastMessages(string roomName)
    {
        var messages = await _ctx.Messages.Where(m => m.Room.Roomname == roomName)
            .Where(m => m.Recipientuserid == null)
            .OrderByDescending(m => m.Sentat)
            .Take(50)
            .Select(m => new
            {
                user = m.Senderuser.Username,
                message = m.Content,
                sentAt = m.Sentat
            })
            .ToListAsync();

        return Ok(messages);
    }
}