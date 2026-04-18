using System.Text.Json.Serialization;

namespace exam.Models

{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]

        public string Msg { get; set; }

        [JsonPropertyName("movie")]
        public MovieDTO Movie { get; set; } = new MovieDTO();
    }
}
