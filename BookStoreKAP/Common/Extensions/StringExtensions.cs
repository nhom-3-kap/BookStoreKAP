using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace BookStoreKAP.Common.Extensions
{
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Chuyển chữ hoa thành chữ thường và thêm dấu gạch dưới giữa các từ
            string snakeCase = Regex.Replace(input, "([a-z])([A-Z])", "$1_$2");

            // Thay khoảng trắng và dấu gạch ngang bằng dấu gạch dưới, đồng thời chuyển toàn bộ chuỗi thành chữ hoa
            snakeCase = Regex.Replace(snakeCase, @"[\s-]+", "_").ToUpperInvariant();

            return snakeCase;
        }
    }
}
