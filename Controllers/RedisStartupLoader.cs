using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using MOZ_UPGRADE.Interfaces;

public class RedisStartupLoader : IHostedService
{
    private readonly IDatabase _redis;
    private readonly IServiceProvider _serviceProvider;

    public RedisStartupLoader(
        IConnectionMultiplexer connection,
        IServiceProvider serviceProvider)
    {
        _redis = connection.GetDatabase();
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var user = await unitOfWork.userRepository
                    .GetByEmail("emmanukiptoo98@gmail.com");

                if (user != null)
                {
                    await _redis.StringSetAsync("global", user.FullName);
                }
            }

        }, cancellationToken);

        return Task.CompletedTask; // VERY IMPORTANT
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}