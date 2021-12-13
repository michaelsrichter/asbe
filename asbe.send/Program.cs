using asbe.send;
using Microsoft.ApplicationInsights.WorkerService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        var aiOptions = new ApplicationInsightsServiceOptions();
        aiOptions.EnableAdaptiveSampling = false;
        services.AddHostedService<Worker>();
        services.AddApplicationInsightsTelemetryWorkerService(aiOptions);
    })
    .Build();

await host.RunAsync();
