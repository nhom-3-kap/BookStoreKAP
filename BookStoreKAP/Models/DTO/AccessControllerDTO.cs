using BookStoreKAP.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace BookStoreKAP.Models.DTO
{
    public class ReqCreateAccessController
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public List<Domain> Domains { get; set; }
    }
}
