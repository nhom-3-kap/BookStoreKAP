using Newtonsoft.Json;

namespace BookStoreKAP.Models.DTO
{
    public class ReqCreateBook
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
    }
}
