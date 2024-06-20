using BookStoreKAP.Areas.Api.Models;
using BookStoreKAP.Common.Constants;
using BookStoreKAP.Data;
using BookStoreKAP.Models;
using BookStoreKAP.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public IActionResult GetChartCountViewBook()
        {
            try
            {
                var labels = _context.Books.Select(x => x.Title).ToList();
                var labelDataChart = "Số lượt xem";
                var dataChart = _context.Books.Select(x => x.ViewCount).ToList().ConvertAll(x => double.Parse(x.ToString()));

                var data = new Chart() { Labels = labels, DataChart = new List<DataChart>() { new() { Label = labelDataChart, Data = dataChart } } };

                return Ok(new ResponseAPI<Chart>() { Success = true, Message = "Get Chart Success✅", Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseAPI<string>() { Success = false, Message = ex.Message });
            }
        }

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

        [HttpGet]
        public async Task<IActionResult> GetChartTotalPriceForMQY([FromQuery] int year)
        {
            try
            {
                // Define the labels
                List<string> labels = new() { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Q1", "Q2", "Q3", "Q4", year.ToString() };

                // Monthly data
                var monthlyData = await _context.Orders
                                                .Where(o => o.Status == StatusType.APPROVED && o.OrderDate.Year == year)
                                                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                                                .Select(g => new
                                                {
                                                    Period = g.Key.Month, // Chỉ lấy tháng
                                                    Total = g.Sum(o => o.Total)
                                                })
                                                .ToListAsync();
                // Quarterly data
                var quarterlyData = await _context.Orders
                                                .Where(o => o.Status == StatusType.APPROVED && o.OrderDate.Year == year)
                                                .GroupBy(o => new { o.OrderDate.Year, Quarter = (o.OrderDate.Month - 1) / 3 + 1 })
                                                .Select(g => new
                                                {
                                                    Period = "Q" + g.Key.Quarter, // Chỉ lấy quý
                                                    Total = g.Sum(o => o.Total)
                                                })
                                                .ToListAsync();

                // Yearly data
                var yearlyData = await _context.Orders
                                                .Where(o => o.Status == StatusType.APPROVED && o.OrderDate.Year == year)
                                                .GroupBy(o => o.OrderDate.Year)
                                                .Select(g => new
                                                {
                                                    Year = g.Key.ToString(), // Chỉ lấy năm
                                                    Total = g.Sum(o => o.Total)
                                                })
                                                .ToListAsync();

                var dataChartM = labels.Select(label => monthlyData.FirstOrDefault(md => labels[md.Period - 1] == label)?.Total ?? 0.0).ToList();
                var dataChartQ = labels.Select(label => quarterlyData.FirstOrDefault(qd => qd.Period == label)?.Total ?? 0.0).ToList();
                var dataChartY = labels.Select(label => yearlyData.FirstOrDefault(yd => yd.Year.ToString() == label)?.Total ?? 0.0).ToList();
                var data = new Chart()
                {
                    Labels = labels,
                    DataChart = new List<DataChart>()
                    {
                        new()
                        {
                            Label = "monthly",
                            Data = dataChartM },
                        new()
                        {
                            Label = "quarterly",
                            Data = dataChartQ
                        },
                        new()
                        {
                            Label = "yearly",
                            Data = dataChartY
                        }
                    }
                };

                return Ok(new ResponseAPI<Chart> { Success = true, Message = "Get Chart Success✅", Data = data });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseAPI<string> { Success = false, Message = ex.Message });
            }
        }
    }
}
