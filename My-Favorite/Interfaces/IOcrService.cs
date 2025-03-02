public interface IOcrService
{
    Task<string> ExtractText(Stream fileStream);
}
