using api.Dto;
using api.Service;
using Microsoft.AspNetCore.Mvc;

namespace api.Controller;

[ApiController]
[Route("api/[controller]")]
public class MessagesController(MessageService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] CreateMessageDto dto)
    {
        await service.SendMessage(dto);
        return Ok();
    }

    [HttpGet("{roomName}")]
    public async Task<ActionResult> GetLastMessages(string roomName)
    {
        var result = await service.GetLastMessages(roomName);
        return Ok(result);
    }
    
}