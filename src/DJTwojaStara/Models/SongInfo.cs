namespace DJTwojaStara.Services;

public record SongInfo
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string CoverUrl { get; set; }
    public float Length { get; set; }
    public string Url { get; set; }
    public string Id { get; set; }
}