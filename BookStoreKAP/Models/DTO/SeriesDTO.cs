using System.ComponentModel.DataAnnotations;

namespace BookStoreKAP.Models.DTO
{
    public class ReqCreateSeries
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int Volumns { get; set; }
    }
}
