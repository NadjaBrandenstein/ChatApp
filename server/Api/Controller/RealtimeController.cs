using System.Text.Json;
using api.Dto;
using api.Service;
using efscaffold.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StateleSSE.AspNetCore;


[ApiController]
[Route("")]
public class RealtimeController(
    ISseBackplane backplane,
    MessageService msgService,
    MyDbContext ctx,
    JwtService jwtService) : ControllerBase
{
    private readonly MyDbContext _ctx = ctx;
    private readonly JwtService _jwtService = jwtService;

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



    [HttpPost("join")]
    public async Task Join([FromBody] JoinDto dto)
    {
        await backplane.Groups.AddToGroupAsync(dto.connectionId, dto.room);
        await backplane.Clients.SendToGroupAsync(dto.room, new JoinResponse("Someone has entered the chat!"));
    }


    [Authorize]
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] ChatMessageDto dto)
    {
        var username = User.Identity!.Name;
        await msgService.SendMessage(new CreateMessageDto
        {
            RoomName = dto.room,
            Username = username!,
            Content = dto.message
        });
        
        await backplane.Clients.SendToGroupAsync(
            dto.room,
            new ChatResponse(username!,dto.message)
        );

        return Ok();
    }

    [HttpPost("poke")]
    public async Task Poke(string connectionId)
    {
        await backplane.Clients.SendToClientAsync(connectionId, new PokeResponse("You have been poked!"));
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

    [Authorize]
    [HttpPost("rooms")]
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


    public record PokeResponse(string Message) : BaseResponseDto;

    public record JoinResponse(string Message) : BaseResponseDto;

    public record ChatResponse(string user, string message) : BaseResponseDto;

    public record TypingResponse(string User, bool IsTyping) : BaseResponseDto;

    public record TypingRequest(string Room, bool IsTyping);

    public record ChatMessageDto(string room, string message);

    public record JoinDto(string connectionId, string room);

}