namespace OrleansPlusEventStoreDb.Demo.Models;

[GenerateSerializer]
public record ProcessingResult(bool IsSuccess, string? ErrorMessage = null)
{
    public static ProcessingResult Fail(string message) => new(false, message);

    public static ProcessingResult Success() => new(true);
}