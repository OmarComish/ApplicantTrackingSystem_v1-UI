using ATS.API.Services.BackgroundServices;

namespace ATS.API.Services.BackgroundServices
{
    /// <summary>
    /// Strongly-typed settings for <see cref="ApplicantRankingBackgroundService"/>.
    /// Bind these from appsettings.json under the key "ApplicantRanking".
    /// </summary>
    public class ApplicantRankingOptions
    {
        public const string SectionName = "ApplicantRanking";

        /// <summary>
        /// How often (in minutes) the background job runs. Default: 60.
        /// </summary>
        public double IntervalMinutes { get; set; } =5;

        /// <summary>
        /// Seconds to wait after app startup before the first run. Default: 30.
        /// Useful to let migrations / warm-up finish before the first ranking pass.
        /// </summary>
        public int InitialDelaySeconds { get; set; } = 15;
    }
}





// ─────────────────────────────────────────────────────────────────────────────
// appsettings.json  — add this block
// ─────────────────────────────────────────────────────────────────────────────
/*
{
  "ApplicantRanking": {
    "IntervalMinutes": 60,
    "InitialDelaySeconds": 30
  }
}

Environment-specific overrides — e.g. appsettings.Development.json:
{
  "ApplicantRanking": {
    "IntervalMinutes": 5,
    "InitialDelaySeconds": 5
  }
}

Or via environment variables (useful in Docker / CI):
  ApplicantRanking__IntervalMinutes=30
  ApplicantRanking__InitialDelaySeconds=10
*/


// ─────────────────────────────────────────────────────────────────────────────
// Program.cs  — minimal wiring example
// ─────────────────────────────────────────────────────────────────────────────
/*
var builder = WebApplication.CreateBuilder(args);

// ... your existing registrations ...

// Register scoped services the background job depends on
builder.Services.AddScoped<IShortlistingService, ShortlistingService>();
builder.Services.AddScoped<IRankingService, RankingService>();

// Register the background service
builder.Services.AddApplicantRankingService(builder.Configuration);

var app = builder.Build();
// ...
app.Run();
*/
