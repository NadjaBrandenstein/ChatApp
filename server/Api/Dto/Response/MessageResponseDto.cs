namespace api.Dto.Response;

public class MessageResponseDto
{
    public string Content { get; set; } = null!;
    public string Sender { get; set; } = null!;
    public DateTime Sentat { get; set; }
}