using BookStoreKAP.Areas.Api.Models;
using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using BookStoreKAP.Filters;
using BookStoreKAP.Models;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BookStoreKAP.Areas.Api.Controllers
{
    [Area(AreasConstant.API)]
    public class ChartsController : Controller
    {
        private readonly BookStoreKAPDBContext _context;
        public ChartsController(BookStoreKAPDBContext context)
        {
            _context = context;
        }

        [PermissionFilter(Name = "GetChartBookAmount")]
        [HttpGet]
        public IActionResult GetChartBookAmount()
        {
            try
            {
                var labels = _context.Books.Select(x => x.Title).ToList();
                var labelDataChart = "Số lượng sách";
                var dataChart = _context.Books.Select(x => x.Quantity).ToList().ConvertAll(x => double.Parse(x.ToString()));
                var data = new Chart() { Labels = labels, DataChart = new List<DataChart>() { new() { Label = labelDataChart, Data = dataChart } } };
                return Ok(new ResponseAPI<Chart>() { Success = true, Message = "Get Chart Success✅", Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseAPI<string>() { Success = false, Message = ex.Message });
            }
        }

        private string GenerateTitleSuffix(IQueryCollection query)
        {
            // Kiểm tra khoảng thời gian tùy chọn trước
            if (query.ContainsKey("fromDate") && query.ContainsKey("toDate") &&
                DateTime.TryParse(query["fromDate"], out var fromDate) &&
                DateTime.TryParse(query["toDate"], out var toDate))
            {
                return $" từ {fromDate:dd/MM/yyyy} đến {toDate:dd/MM/yyyy}";
            }

            if (query.ContainsKey("specificDate") && DateTime.TryParse(query["specificDate"], out var parsedDate))
            {
                return $" ngày {parsedDate:dd/MM/yyyy}";
            }

            if (query.ContainsKey("periodType") && query.ContainsKey("year"))
            {
                var periodType = query["periodType"].ToString().ToLower();
                var year = int.Parse(query["year"]);
                int specific = 0;
                var hasSpecific = query.ContainsKey("specificPeriod") && int.TryParse(query["specificPeriod"], out specific);
                return periodType switch
                {
                    "day" => hasSpecific ? $" trong {specific}/{year}" : $" trong năm {year}",
                    "week" => hasSpecific ? $" tuần {specific}/{year}" : $" các tuần trong năm {year}",
                    "month" => hasSpecific ? $" tháng {specific}/{year}" : $" các tháng trong năm {year}",
                    "quarter" => hasSpecific ? $" quý {specific}/{year}" : $" các quý trong năm {year}",
                    "year" => hasSpecific ? $" năm {specific}" : $" các năm từ {year - 4} đến {year}",
                    _ => ""
                };
            }
            return "";
        }

        // Phương thức lọc orders theo các tham số thời gian - Cập nhật với hỗ trợ khoảng thời gian
        private IQueryable<Order> FilterOrdersByTimeParams(IQueryable<Order> orders,
            int? year, string periodType, int? specificPeriod, DateTime? specificDate,
            DateTime? fromDate, DateTime? toDate)
        {
            // Ưu tiên khoảng thời gian tùy chọn
            if ((fromDate.HasValue && toDate.HasValue) || periodType == "custom")
            {
                return orders.Where(o => o.CreatedAt.Date >= fromDate.Value.Date && o.CreatedAt.Date <= toDate.Value.Date);
            }

            if (specificDate.HasValue)
            {
                return orders.Where(o => o.CreatedAt.Date == specificDate.Value.Date);
            }

            if (!year.HasValue || string.IsNullOrEmpty(periodType))
            {
                return orders;
            }

            switch (periodType.ToLower())
            {
                case "day":
                    if (specificPeriod.HasValue) // Tháng cụ thể trong năm
                    {
                        return orders.Where(o => o.CreatedAt.Year == year && o.CreatedAt.Month == specificPeriod.Value);
                    }
                    return orders.Where(o => o.CreatedAt.Year == year);
                case "week":
                    if (specificPeriod.HasValue) // Tuần cụ thể trong năm
                    {
                        var jan1 = new DateTime(year.Value, 1, 1);
                        var daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
                        var firstMonday = jan1.AddDays(daysOffset);
                        var weekStart = firstMonday.AddDays((specificPeriod.Value - 1) * 7);
                        var weekEnd = weekStart.AddDays(6);
                        return orders.Where(o => o.CreatedAt.Date >= weekStart && o.CreatedAt.Date <= weekEnd);
                    }
                    return orders.Where(o => o.CreatedAt.Year == year);
                case "month":
                    if (specificPeriod.HasValue) // Tháng cụ thể trong năm
                    {
                        return orders.Where(o => o.CreatedAt.Year == year && o.CreatedAt.Month == specificPeriod.Value);
                    }
                    return orders.Where(o => o.CreatedAt.Year == year);
                case "quarter":
                    if (specificPeriod.HasValue) // Quý cụ thể trong năm
                    {
                        var startMonth = (specificPeriod.Value - 1) * 3 + 1;
                        var endMonth = specificPeriod.Value * 3;
                        return orders.Where(o => o.CreatedAt.Year == year &&
                                                  o.CreatedAt.Month >= startMonth &&
                                                  o.CreatedAt.Month <= endMonth);
                    }
                    return orders.Where(o => o.CreatedAt.Year == year);
                case "year":
                    if (specificPeriod.HasValue) // Năm cụ thể
                    {
                        return orders.Where(o => o.CreatedAt.Year == specificPeriod.Value);
                    }
                    // Lấy 5 năm gần nhất tính từ year
                    int startYear = year.Value - 4;
                    return orders.Where(o => o.CreatedAt.Year >= startYear && o.CreatedAt.Year <= year);
                default:
                    return orders.Where(o => o.CreatedAt.Year == year);
            }
        }
        private string GenerateDateRangeTitle(DateTime fromDate, DateTime toDate)
        {
            return $" từ {fromDate:dd/MM/yyyy} đến {toDate:dd/MM/yyyy}";
        }
        [PermissionFilter(Name = "GetChartBookSales")]
        [HttpGet]
        public async Task<IActionResult> GetChartBookSales([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string saleType)
        {
            try
            {
                string titleSuffix = GenerateDateRangeTitle(fromDate, toDate);
                Chart data = new Chart();

                // Bước 1: Lọc các đơn hàng đã hoàn thành trong khoảng thời gian để lấy ID
                var orderIds = await _context.Orders
                    .Where(o => o.Status == 1 && o.CreatedAt.Date >= fromDate.Date && o.CreatedAt.Date <= toDate.Date)
                    .Select(o => o.ID)
                    .ToListAsync();

                // Bước 2: Xử lý trường hợp không có đơn hàng nào
                if (!orderIds.Any())
                {
                    return Ok(new ResponseAPI<Chart>()
                    {
                        Success = true,
                        Message = "Không có dữ liệu sách bán trong khoảng thời gian đã chọn.",
                        Data = new Chart
                        {
                            Labels = new List<string>(),
                            DataChart = new List<DataChart>()
                        }
                    });
                }

                // Bước 3: Truy vấn chi tiết đơn hàng dựa trên các ID đã lọc
                var relevantOrderDetails = _context.OrderDetails.Where(od => orderIds.Contains(od.OrderID));

                // Bước 4: Nhóm dữ liệu dựa trên loại thống kê (saleType)
                switch (saleType.ToLower())
                {
                    case "genre":
                        var genreStats = await relevantOrderDetails
                            .Include(od => od.Book)
                            .ThenInclude(b => b.BookGenres)
                            .ThenInclude(bg => bg.Genre)
                            .SelectMany(od => od.Book.BookGenres.Select(bg => new {
                                GenreName = bg.Genre.Name,
                                Quantity = od.Quantity
                            }))
                            .GroupBy(x => x.GenreName)
                            .Select(g => new {
                                Name = g.Key,
                                SoldCount = g.Sum(x => x.Quantity)
                            })
                            .OrderByDescending(x => x.SoldCount)
                            .ToListAsync();

                        data.Labels = genreStats.Select(g => g.Name).ToList();
                        data.DataChart = new List<DataChart> {
                    new() {
                        Label = $"Sách đã bán theo thể loại{titleSuffix}",
                        Data = genreStats.Select(g => (double)g.SoldCount).ToList()
                    }
                };
                        break;

                    case "series":
                        var seriesStats = await relevantOrderDetails
                            .Include(od => od.Book)
                            .ThenInclude(b => b.Series)
                            .Where(od => od.Book.SeriesID != null) // Chỉ lấy sách có series
                            .GroupBy(od => od.Book.Series.Name)
                            .Select(g => new {
                                Name = g.Key,
                                SoldCount = g.Sum(od => od.Quantity)
                            })
                            .OrderByDescending(x => x.SoldCount)
                            .ToListAsync();

                        data.Labels = seriesStats.Select(s => s.Name).ToList();
                        data.DataChart = new List<DataChart> {
                    new() {
                        Label = $"Sách đã bán theo series{titleSuffix}",
                        Data = seriesStats.Select(s => (double)s.SoldCount).ToList()
                    }
                };
                        break;

                    case "buycount":
                        var bookStats = await relevantOrderDetails
                            .Include(od => od.Book)
                            .GroupBy(od => od.Book.Title)
                            .Select(g => new {
                                Name = g.Key,
                                SoldCount = g.Sum(od => od.Quantity)
                            })
                            .OrderByDescending(x => x.SoldCount)
                            .Take(20) // Có thể giới hạn để biểu đồ không quá lớn
                            .ToListAsync();

                        data.Labels = bookStats.Select(b => b.Name).ToList();
                        data.DataChart = new List<DataChart> {
                    new() {
                        Label = $"Sách bán chạy nhất{titleSuffix}",
                        Data = bookStats.Select(b => (double)b.SoldCount).ToList()
                    }
                };
                        break;

                    default:
                        return BadRequest(new ResponseAPI<string>() { Success = false, Message = "Loại thống kê không hợp lệ." });
                }

                // Bước 5: Kiểm tra lại nếu kết quả rỗng (ví dụ: có đơn hàng nhưng sách không thuộc thể loại/series nào)
                if (data.Labels.Count == 0)
                {
                    return Ok(new ResponseAPI<Chart>()
                    {
                        Success = true,
                        Message = "Không tìm thấy dữ liệu phù hợp với loại thống kê đã chọn.",
                        Data = new Chart { Labels = new List<string>(), DataChart = new List<DataChart>() }
                    });
                }

                return Ok(new ResponseAPI<Chart>() { Success = true, Message = "Lấy dữ liệu biểu đồ thành công.✅", Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseAPI<string>() { Success = false, Message = ex.Message });
            }
        }

        [PermissionFilter(Name = "GetChartTotalPrice")]
        [HttpGet]
        public async Task<IActionResult> GetChartTotalPrice([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string periodType)
        {
            try
            {
                // Lọc các đơn hàng đã hoàn thành trong khoảng thời gian đã chọn
                var orders = await _context.Orders
                    .Where(o => o.Status == 1 && o.CreatedAt.Date >= fromDate.Date && o.CreatedAt.Date <= toDate.Date)
                    .ToListAsync(); // Tải dữ liệu vào bộ nhớ để xử lý nhóm dễ dàng hơn

                if (!orders.Any())
                {
                    // Trả về thông báo không có dữ liệu nếu không tìm thấy đơn hàng nào
                    return Ok(new ResponseAPI<Chart>()
                    {
                        Success = true,
                        Message = "Không có dữ liệu trong khoảng thời gian đã chọn.",
                        Data = new Chart { Labels = new List<string>(), DataChart = new List<DataChart>() }
                    });
                }

                var chartData = new Chart();
                var dataPoints = new List<KeyValuePair<string, double>>();

                // Nhóm dữ liệu dựa trên periodType
                switch (periodType.ToLower())
                {
                    case "day":
                        dataPoints = orders
                            .GroupBy(o => o.CreatedAt.Date)
                            .Select(g => new KeyValuePair<string, double>(g.Key.ToString("dd/MM/yyyy"), g.Sum(o => o.Total)))
                            .OrderBy(kvp => DateTime.ParseExact(kvp.Key, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                            .ToList();
                        break;

                    case "week":
                        dataPoints = orders
                           .GroupBy(o => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(o.CreatedAt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday))
                           .Select(g => {
                               var firstDayOfWeek = g.First().CreatedAt; // Lấy ngày đại diện
                               return new KeyValuePair<string, double>($"Tuần {g.Key}/{firstDayOfWeek.Year}", g.Sum(o => o.Total));
                           })
                           .OrderBy(kvp => kvp.Key)
                           .ToList();
                        break;

                    case "month":
                        dataPoints = orders
                            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                            .Select(g => new KeyValuePair<string, double>($"Tháng {g.Key.Month}/{g.Key.Year}", g.Sum(o => o.Total)))
                            .OrderBy(kvp => kvp.Key.Split('/')[1]).ThenBy(kvp => kvp.Key.Split('/')[0].Split(' ')[1])
                            .ToList();
                        break;

                    case "quarter":
                        dataPoints = orders
                            .GroupBy(o => new { o.CreatedAt.Year, Quarter = (o.CreatedAt.Month - 1) / 3 + 1 })
                            .Select(g => new KeyValuePair<string, double>($"Quý {g.Key.Quarter}/{g.Key.Year}", g.Sum(o => o.Total)))
                            .OrderBy(kvp => kvp.Key.Split('/')[1]).ThenBy(kvp => kvp.Key.Split('/')[0].Split(' ')[1])
                            .ToList();
                        break;

                    case "year":
                        dataPoints = orders
                            .GroupBy(o => o.CreatedAt.Year)
                            .Select(g => new KeyValuePair<string, double>(g.Key.ToString(), g.Sum(o => o.Total)))
                            .OrderBy(kvp => kvp.Key)
                            .ToList();
                        break;
                }

                chartData.Labels = dataPoints.Select(dp => dp.Key).ToList();
                chartData.DataChart = new List<DataChart>
            {
                new DataChart
                {
                    Label = $"Tổng doanh thu từ {fromDate:dd/MM/yyyy} đến {toDate:dd/MM/yyyy}",
                    Data = dataPoints.Select(dp => dp.Value).ToList()
                }
            };

                return Ok(new ResponseAPI<Chart>() { Success = true, Message = "Get Chart Success✅", Data = chartData });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseAPI<string>() { Success = false, Message = ex.Message });
            }
        }
    }
}

