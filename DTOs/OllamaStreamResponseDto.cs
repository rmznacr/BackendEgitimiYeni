using System.Text.Json.Serialization;

namespace BackendEgitimiYeni.DTOs;

public class OllamaStreamResponseDto
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;

    [JsonPropertyName("done")]
    public bool Done { get; set; }
}