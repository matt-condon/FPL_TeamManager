using FplManager.Infrastructure.Models;
using System.Collections.Generic;

namespace FplManager.Configuration
{
    public class AppConfig
    {
        public IEnumerable<AccountModel> AccountModel { get; set; }
        public string LogInUrl { get; set; }
    }
}
