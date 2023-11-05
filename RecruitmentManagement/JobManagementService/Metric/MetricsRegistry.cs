using Prometheus;

namespace JobManagementService.Metric;

public static class MetricsRegistry
{
    public static readonly Counter JobApplicationsGetCounter = Metrics
        .CreateCounter("job_applications_get_total", "Counts requests to the GET /jobs/applications/{jobId} endpoint");
}