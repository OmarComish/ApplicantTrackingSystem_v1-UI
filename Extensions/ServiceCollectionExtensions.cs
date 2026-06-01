// ─────────────────────────────────────────────────────────────────────────────
// ServiceCollectionExtensions.cs  (registration helper — add to Program.cs)
// ─────────────────────────────────────────────────────────────────────────────
namespace ATS.API.Services.BackgroundServices
{
    public static class ApplicantRankingServiceExtensions
    {
        /// <summary>
        /// Registers the applicant-ranking background service and its options.
        /// Call from Program.cs:  builder.Services.AddApplicantRankingService(builder.Configuration);
        /// </summary>
        public static IServiceCollection AddApplicantRankingService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<ApplicantRankingOptions>(
                configuration.GetSection(ApplicantRankingOptions.SectionName));

            services.AddHostedService<ApplicantRankingBackgroundService>();

            return services;
        }
    }
}