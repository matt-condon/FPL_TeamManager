using FplManager.Infrastructure.Models;

namespace FplManager.Configuration
{
    public class AppConfig
    {
        public AccountModel[] AccountModel { get; set; }
        public string LogInUrl { get; set; }
    }
}
