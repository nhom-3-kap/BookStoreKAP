namespace BookStoreKAP.Common.Constants
{
    public class RouteConstant
    {
        public const string HOME = "/";

        //Admin space
        public const string ADMIN = $"/{AreasConstant.ADMIN}";
        public const string ADMIN_BOOKS = $"/{AreasConstant.ADMIN}/Books";
        public const string ADMIN_BOOKS_CREATE = $"/{AreasConstant.ADMIN}/Books/Create";
        public const string ADMIN_CATEGORIES = $"/{AreasConstant.ADMIN}/CategoriesManager";
        public const string ADMIN_CATEGORIES_CREATE = $"/{AreasConstant.ADMIN}/CategoriesManager/Create";
    }
}
