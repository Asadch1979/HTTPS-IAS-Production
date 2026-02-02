using System.Text.Json.Serialization;

namespace AIS.Models
{
    public class ChecklistDetailsRowDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("sName")]
        public string SName { get; set; }

        [JsonPropertyName("vId")]
        public int VId { get; set; }

        [JsonPropertyName("heading")]
        public string Heading { get; set; }
    }
}
