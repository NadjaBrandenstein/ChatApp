namespace api.Dto.Response;

public record ChatResponse(string User, string Message) : BaseResponseDto;
