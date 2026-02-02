using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StateleSSE.AspNetCore;


[ApiController]
[Route("")]
public class RealtimeController(ISseBackplane backplane) : ControllerBase
{
    [HttpGet("connect")]
    public async Task Connect()
    {
        await using var sse = await HttpContext.OpenSseStreamAsync();
        await using var connection = backplane.CreateConnection();

        await sse.WriteAsync("connected", JsonSerializer.Serialize(new { connection.ConnectionId },
            new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));

        await foreach (var evt in connection.ReadAllAsync(HttpContext.RequestAborted))
            await sse.WriteAsync(evt.Group ?? "message", evt.Data);
    }

    [HttpPost("join")]
    public async Task Join([FromBody] JoinDto dto)
    {
        await backplane.Groups.AddToGroupAsync(dto.connectionId, dto.room);
        await backplane.Clients.SendToGroupAsync(dto.room, new JoinResponse("Someone has entered the chat!"));
    }

    [HttpPost("send")]
    public async Task Send([FromBody] ChatMessageDto dto)
    {
        await backplane.Clients.SendToGroupAsync(dto.room, new ChatResponse(dto.user, dto.message));
    }
    
    // [HttpPost("poke")]
    // public async Task Poke(string room)
    //     => await backplane.Clients.SendToGroupAsync(room, "poked");

    [HttpPost("poke")]
    public async Task Poke(string connectionId)
    {
        await backplane.Clients.SendToClientAsync(connectionId, new PokeRespone("You have been poked!"));
    }

    [HttpPost("leave")]
    public async Task Leave(string roomId, string connectionId)
    {
        await backplane.Groups.RemoveFromGroupAsync(connectionId, roomId);
    }

    [HttpPost("typing")]
    public async Task Typing([FromBody] TypingRequest request)
    {
        await backplane.Clients.SendToGroupAsync(
            request.Room,
            new TypingResponse("Someone", request.IsTyping)
            );
    }

    public record PokeRespone(string Message) : BaseResponseDto;
    public record JoinResponse(string Message) : BaseResponseDto;
    public record ChatResponse(string user, string message) : BaseResponseDto;
    public record TypingResponse(string User, bool IsTyping) : BaseResponseDto;

    public record TypingRequest(string Room, bool IsTyping);
    public record ChatMessageDto(string room, string message, string user);
    public record JoinDto(string connectionId, string room);
    
}