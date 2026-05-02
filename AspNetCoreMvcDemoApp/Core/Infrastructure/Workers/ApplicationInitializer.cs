using AspNetCoreMvcDemoApp.Core.Data.Abstractions;
using AspNetCoreMvcDemoApp.Core.Infrastructure.Caching.Abstractions;

namespace AspNetCoreMvcDemoApp.Core.Infrastructure.Workers
{
    public class ApplicationInitializer(IServiceProvider serviceProvider, ILogger<ApplicationInitializer> logger) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var sp = scope.ServiceProvider;

            try
            {
                var db = sp.GetRequiredService<IDataService>();
                var userCache = sp.GetRequiredService<IActiveUserCache>();

                var activeIds = await db.Users.GetActiveIdsAsync();
                userCache.Update(activeIds);

                logger.LogInformation("User Cache warmed up with {Count} users.", activeIds.Count());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while warming up the User Cache!");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
