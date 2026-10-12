using MShrink.Models;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace MShrink.Services
{
    public class ColorConfig
    {
        [JsonPropertyName("scheme")]
        public ColorScheme Scheme { get; set; }
        [JsonPropertyName("color")]
        public Color? Color { get; set; }
    }
}
