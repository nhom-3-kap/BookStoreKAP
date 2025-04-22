using BookStoreKAP.Models.Entities;
using System.Collections.Generic;

namespace BookStoreKAP.Services
{
    public interface ITagService
    {
        IEnumerable<Tag> GetAllTags();
    }
}