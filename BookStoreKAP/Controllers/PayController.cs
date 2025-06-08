using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.DTO;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using BookStoreKAP.Services;
using BookStoreKAP.Common.Constants;

namespace BookStoreKAP.Controllers
{
    public class PayController : Controller
    {
        private readonly BookStoreKAPDBContext _context;
        public PayController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        public IActionResult Index(double totalPrice)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);
            ViewBag.TotalPrice = totalPrice;
            return View(user);
        }

        [HttpPost]
        public IActionResult Payment(PaymentDTO req)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);
            var order = new Order()
            {
                Address = req.Address,
                CustomerID = user.Id,
                PaymentMethod = req.PaymentMethod,
                OrderDate = DateTime.Now,
                Status=1// Cập nhật thời gian hiện tại
            };
            _context.Orders.Add(order);
            _context.SaveChanges();
            var cart = _context.Carts.FirstOrDefault(x => x.UserID == user.Id && x.Status == StatusCart.PENDING);
            var cartItems = _context.CartItems.Include(x => x.Book).Where(x => x.CartID == cart.ID).ToList();
            var orderDetails = new List<OrderDetail>();
            var total = 0.0;
            cartItems.ForEach(cartItem =>
            {
                total += cartItem.Price * cartItem.Quantity;
                orderDetails.Add(new OrderDetail
                {
                    BookID = cartItem.BookID,
                    OrderID = order.ID,
                    Price = cartItem.Price,
                    Quantity = cartItem.Quantity,
                });
                // Cập nhật BuyCount cho Book
                var book = _context.Books.Find(cartItem.BookID);
                if (book != null)
                {
                    book.BuyCount += cartItem.Quantity;
                    _context.Books.Update(book);
                }
            });
            order.Total = total;
            cart.Status = StatusCart.DONE;
            _context.Carts.Update(cart);
            _context.Orders.Update(order);
            _context.OrderDetails.AddRange(orderDetails);
            _context.SaveChanges();
            // Gọi phương thức cập nhật tags sau khi cập nhật BuyCount
            try
            {
                // Giả sử có thể truy cập đến BooksController hoặc tạo một service riêng
                // Ở đây sử dụng cách trực tiếp
                UpdateBookTags();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Tag update failed: {ex.Message}");
                // Không cần rollback giao dịch vì đơn hàng vẫn hoàn tất
            }
            TempData[ToastrConstant.SUCCESS_MSG] = "Payment success";
            return Redirect($"{RouteConstant.HOME}");
        }

        // Phương thức hỗ trợ để cập nhật tags
        private void UpdateBookTags()
        {
            var BEST_SELLER_TAG_ID = new Guid("3E5CAC9B-6E5A-416D-86F8-52F044D6994E");
            var NEW_RELEASE_TAG_ID = new Guid("CA038048-95D2-4BFD-86D8-740FB2ECE1AF");
            // Reset tags trước khi cập nhật
            var allBooks = _context.Books.ToList();
            //foreach (var book in allBooks)
            //{
            //    book.TagID = null;
            //}
            _context.SaveChanges();
            // 1. Xác định sách New Release: sách có CreatedAt trong vòng 1 tháng
            var oneMonthAgo = DateTime.Now.AddMonths(-1);
            var newReleaseBooks = _context.Books
                .Where(b => b.CreatedAt >= oneMonthAgo)
                .ToList();
            foreach (var book in newReleaseBooks)
            {
                book.TagID = NEW_RELEASE_TAG_ID;
            }
            _context.SaveChanges();
            // 2. Xác định Best Seller: sách có BuyCount > 10
            var bestSellerBooks = _context.Books
                .Where(b => b.BuyCount > 10)
                .ToList();
            foreach (var book in bestSellerBooks)
            {
                book.TagID = BEST_SELLER_TAG_ID;
            }
            _context.SaveChanges();
        }

        public IActionResult CreateQRPaymentAPI(double totalPrice)
        {
            // Khởi tạo dịch vụ QRCodeService với thông tin cần thiết
            var qrCodeService = new QrCodeService(BankName.MBBank, "0376976198");
            var qrString = qrCodeService.BuildQRString(totalPrice, "Send to KAPBookStore");
            // Tạo đối tượng QRCodeGenerator
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(qrString, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCoder.Base64QRCode(qrCodeData);
            var base64QR = qrCode.GetGraphic(5);
            // Trả về base64 string
            return Ok(new ResponseAPI<string>() { Success = true, Message = "Generator QR Code Successfully", Data = base64QR });
        }

        public IActionResult Purchase(string search, int page = 1, int pageSize = 10)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName == this.User.Identity.Name);
            if (user == null)
            {
                TempData[ToastrConstant.ERROR_MSG] = "User not found";
                return RedirectToAction("Index", "Home");
            }
            var query = from od in _context.OrderDetails
                        join o in _context.Orders on od.OrderID equals o.ID
                        join b in _context.Books on od.BookID equals b.ID
                        where o.CustomerID == user.Id // Chỉ lấy sách mà user đã mua
                        select new PurchaseViewModel
                        {
                            ID = od.ID, // Thêm ID vào để dùng cho Edit và Delete
                            Title = b.Title,
                            Author = b.Author,
                            Price = od.Price,
                            Quantity = od.Quantity,
                            UserName = user.UserName,
                            BoughtDate = o.OrderDate,
                            Thumbnail = b.Thumbnail // Added Thumbnail property
                        };
            // Tìm kiếm theo tên sách hoặc tác giả
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Title.Contains(search) || x.Author.Contains(search));
            }
            // Phân trang
            int totalItems = query.Count();
            var purchases = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            return View(purchases);
        }
        // THÊM CÁC CHỨC NĂNG MỚI



    }
}