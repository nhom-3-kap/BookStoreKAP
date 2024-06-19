using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace BookStoreKAP.Areas.Api.Models
{
    public class Chart
    {
        [JsonPropertyName("labels")]
        public List<string> Labels { get; set; } = new List<string>();
        [JsonPropertyName("datasets")]
        public List<DataChart> DataChart { get; set; } = new List<DataChart>();
    }

    public class DataChart
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = "";
        [JsonPropertyName("data")]
        public List<double>? Data { get; set; } = new List<double>();
    }
}
