using Prometheus;

namespace ApplicationManagementService.Metric;

public static class MetricsRegistry
{
    public static readonly Counter ApplicationsGetCounter = Metrics
        .CreateCounter("applications_get_total", "Counts requests to the GET /applications endpoint");

    public static readonly Counter ApplicationGetByIdCounter = Metrics
        .CreateCounter("application_get_by_id_total", "Counts requests to the GET /applications/{id} endpoint");

    public static readonly Counter ApplicationGetByJobIdCounter = Metrics
        .CreateCounter("application_get_by_job_id_total", "Counts requests to the GET /job/{jobId} endpoint");

    public static readonly Counter ApplicationPostCounter = Metrics
        .CreateCounter("applications_post_total", "Counts requests to the POST /applications endpoint");

    public static readonly Counter ApplicationPutCounter = Metrics
        .CreateCounter("applications_put_total", "Counts requests to the PUT /applications/{id} endpoint");

    public static readonly Counter ApplicationDeleteCounter = Metrics
        .CreateCounter("applications_delete_total", "Counts requests to the DELETE /applications/{id} endpoint");
}