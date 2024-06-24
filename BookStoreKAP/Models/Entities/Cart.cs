using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreKAP.Models.Entities
{
	public class Cart : BaseEntity
	{
		[ForeignKey(nameof(UserID))]
		public Guid UserID { get; set; }
		public User User { get; set; }
		public StatusCart Status { get; set; }

		public ICollection<CartItem> CartItems { get; set; }
	}
	public enum StatusCart
	{
		PENDING,
		DONE,
	}
}
