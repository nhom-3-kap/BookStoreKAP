namespace BookStoreKAP.Common.Constants
{
    public class RouteConstant
    {
        #region Static
        public const string HOME = "/";
        public const string LOCATION = "/Location";
        public const string ABOUT_US = "/AboutUs";
        public const string BLOG = "/Blog";
        public const string CONTACT_US = "/ContactUs";
        public const string PAY = "/Pay";
        public const string PRODUCT_DETAIL = "/ProductDetails";
        public const string LIST = "/List";
        public const string BOOK_REQUEST = "/BookRequest";
        public const string CART = "/Cart";
        #endregion

        #region Admin Routes
        public const string ADMIN = $"/{AreasConstant.ADMIN}";

        public const string ADMIN_SERIES = $"/{AreasConstant.ADMIN}/Series";
        public const string ADMIN_SERIES_CREATE = $"/{AreasConstant.ADMIN}/Series/Create";
        public const string ADMIN_SERIES_MODIFY = $"/{AreasConstant.ADMIN}/Series/Modify";

        public const string ADMIN_GENRE = $"/{AreasConstant.ADMIN}/Genre";
        public const string ADMIN_GENRE_CREATE = $"/{AreasConstant.ADMIN}/Genre/Create";
        public const string ADMIN_GENRE_MODIFY = $"/{AreasConstant.ADMIN}/Genre/Modify";

        public const string ADMIN_TAGS = $"/{AreasConstant.ADMIN}/Tags";
        public const string ADMIN_TAGS_CREATE = $"/{AreasConstant.ADMIN}/Tags/Create";
        public const string ADMIN_TAGS_MODIFY = $"/{AreasConstant.ADMIN}/Tags/Modify";

        public const string ADMIN_BOOKS = $"/{AreasConstant.ADMIN}/Books";
        public const string ADMIN_BOOKS_CREATE = $"/{AreasConstant.ADMIN}/Books/Create";
        public const string ADMIN_BOOKS_MODIFY = $"/{AreasConstant.ADMIN}/Books/Modify";

        public const string ADMIN_USERS = $"/{AreasConstant.ADMIN}/Users";
        public const string ADMIN_USERS_CREATE = $"/{AreasConstant.ADMIN}/Users/Create";
        public const string ADMIN_USERS_MODIFY = $"/{AreasConstant.ADMIN}/Users/Modify";
        #endregion

        #region Auth Routes
        public const string LOGIN = $"/{AreasConstant.AUTH}/Login";
        public const string REGISTER = $"/{AreasConstant.AUTH}/Register";
        public const string EXTERNALLOGIN = $"/{AreasConstant.AUTH}/ExternalLogin";
        public const string FORGOT_PASSWORD = $"/{AreasConstant.AUTH}/ForgotPassword";
        public const string LOGOUT = $"/{AreasConstant.AUTH}/Logout";
        #endregion

    }
}
