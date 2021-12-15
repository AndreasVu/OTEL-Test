using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OTEL_Test
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;

        public Worker(ILogger<Worker> logger)
        {
            this.logger = logger;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var activitySource = new ActivitySource(typeof(Worker).ToString());
            var meter = new Meter("MyApplicationMetrics");
            var requestCounter = meter.CreateCounter<int>("compute_requests");

            while (!stoppingToken.IsCancellationRequested)
            {
                requestCounter.Add(1);
                using (var activity = activitySource.StartActivity("DoWork"))
                {
                    activity?.SetTag("foo", 1);
                    activity?.SetTag("bar", "Hello, World!");
                    activity?.SetTag("baz", new int[] { 1, 2, 3 });
                }

                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
