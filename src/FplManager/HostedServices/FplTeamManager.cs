using FplManager.Application;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace FplManager.HostedServices
{
    public class FplTeamManager : IHostedService
    {
        readonly IApplication _app;
        readonly ILogger<FplTeamManager> _logger;
        public FplTeamManager(ILogger<FplTeamManager> logger, IApplication app)
        {
            _app = app;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _app.Run();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("Application has ended");
            return Task.CompletedTask;
        }
    }
}
