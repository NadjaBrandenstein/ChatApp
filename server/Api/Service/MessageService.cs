using api.Dto;
using api.Dto.Response;
using efscaffold.Entities;
using Microsoft.EntityFrameworkCore;
using StateleSSE.AspNetCore;
using Infrastructure.Postgres.Scaffolding;

namespace api.Service;

public class MessageService(MyDbContext ctx, ISseBackplane backplane)
{
    public async Task SendMessage(CreateMessageDto dto)
    {
        var user = await ctx.Logins.FirstOrDefaultAsync(u => u.Username == dto.Username)
            ?? throw new Exception("User not found");

        var room = await ctx.Rooms.FirstOrDefaultAsync(r => r.Roomname == dto.RoomName)
            ?? throw new Exception("Room not found");
        
        var message = new Message
        {
            Content = dto.Content,
            Sentat = DateTime.UtcNow,
            Senderuserid = user.Userid,
            Roomid = room.Roomid,
            Recipientuserid = dto.RecipientUserId // null = public
        };

        ctx.Messages.Add(message);
        await ctx.SaveChangesAsync();

        await backplane.Clients.SendToGroupAsync(dto.RoomName, new { message = dto.Content, sender = dto.Username });
    }

    public async Task<List<MessageResponseDto>> GetLastMessages(string roomName)
    {
        return await ctx.Messages
            .Where(m => m.Room.Roomname == roomName)
            .Where(m => m.Recipientuserid == null)
            .OrderByDescending(m => m.Sentat)
            .Take(5)
            .Select(m => new MessageResponseDto
            {
                Content = m.Content,
                Sender = m.Senderuser.Username,
                Sentat = m.Sentat,
            })
            .ToListAsync();
    }
    
}