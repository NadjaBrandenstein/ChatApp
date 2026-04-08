namespace api.Dto.Response;

public record TypingResponse(string User, bool IsTyping) : BaseResponseDto;
