namespace api.Dto.Response;

public abstract record BaseResponseDto
{
    public string EventType { get; init; }

    protected BaseResponseDto()
    {
        EventType = GetType().Name;
    }
}