namespace api.Dto;

public class CreateMessageDto
{
    public string Username { get; set; } = null!;
    public string RoomName { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int? RecipientUserId { get; set; } // null = public
}