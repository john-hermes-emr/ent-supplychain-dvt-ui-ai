namespace DVT.Api.Extensions
{
    public static class GetUserInfoFromHttpContextExtension
    {
        public static string GetUserEmailFromHttpContext(this HttpContext context)
        {
            var userEmail = context.User.Claims.Where(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Select(x => x.Value).FirstOrDefault();

            Console.WriteLine(context.User.Identity.Name);
            return userEmail == null ? "test@emerson.com" : userEmail.ToLower();
        }
    }
}
