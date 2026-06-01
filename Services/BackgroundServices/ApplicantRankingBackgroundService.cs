using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace ATS.API.Services.BackgroundServices
{
    /// <summary>
    /// Background service that autonomously ranks new applicants at a configurable interval.
    /// Targets IShortlistingService.AutoRankApplicants().
    /// </summary>
    public class ApplicantRankingBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ApplicantRankingBackgroundService> _logger;
        private readonly ApplicantRankingOptions _options;

        public ApplicantRankingBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ApplicantRankingBackgroundService> logger,
            IOptions<ApplicantRankingOptions> options)
        {
            _scopeFactory  = scopeFactory;
            _logger        = logger;
            _options       = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Applicant Ranking Background Service started. " +
                "Interval: {Interval} minutes, Initial delay: {Delay} seconds.",
                _options.IntervalMinutes,
                _options.InitialDelaySeconds);

            // Optional startup delay — lets the rest of the app initialise first.
            if (_options.InitialDelaySeconds > 0)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(_options.InitialDelaySeconds),
                    stoppingToken);
            }

            using var timer = new PeriodicTimer(
                TimeSpan.FromMinutes(_options.IntervalMinutes));

            // Run immediately on first tick, then repeat on the timer.
            do
            {
                await RankApplicantsSafelyAsync(stoppingToken);
            }
            while (!stoppingToken.IsCancellationRequested
                   && await timer.WaitForNextTickAsync(stoppingToken));

            _logger.LogInformation("Applicant Ranking Background Service is stopping.");
        }

        private async Task RankApplicantsSafelyAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "[{Time}] Running AutoRankApplicants...", DateTime.UtcNow);

            // IShortlistingService is scoped (EF Core context underneath),
            // so we must resolve it inside a fresh DI scope each time.
            await using var scope = _scopeFactory.CreateAsyncScope();

            var shortlistingService =
                scope.ServiceProvider.GetRequiredService<IShortlistingService>();

            try
            {
                await shortlistingService.AutoRankApplicants();

                _logger.LogInformation("[{Time}] AutoRankApplicants completed successfully.", DateTime.UtcNow);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown — not an error.
                _logger.LogInformation("AutoRankApplicants was cancelled due to host shutdown.");
            }
            catch (Exception ex)
            {
                // Log and swallow so the service keeps running on the next tick.
                _logger.LogError(ex,
                    "[{Time}] AutoRankApplicants failed. It will retry at the next interval.",
                    DateTime.UtcNow);
            }
        }
    }
}
