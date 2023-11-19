using Prometheus;

namespace JobManagementService.Metric;

public static class MetricsRegistry
{
    public static readonly Counter JobApplicationsGetCounter = Metrics
        .CreateCounter("job_applications_get_total", "Counts requests to the GET /jobs/applications/{jobId} endpoint");
    
    public static readonly Counter CloseJobsPostCounter = Metrics
        .CreateCounter("job_close_post_total", "Counts requests to the GET /jobs/applications/{jobId} endpoint");
}