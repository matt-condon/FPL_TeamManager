namespace FplManager.Infrastructure.Models
{
    public class AccountModel
    {
        public int FplTeamId { get; set; }
        public AuthenticationModel AuthModel { get; set; }
    }

    public class AuthenticationModel
    {
        public string login { get; set; }
        public string password { get; set; }
        public string app { get; set; }
        public string redirect_uri { get; set; }
    }
}
