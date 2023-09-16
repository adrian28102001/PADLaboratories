namespace ApplicationManagementService.Configurations;

public static class SemaphoreConfiguration
{
    public static readonly SemaphoreSlim ConcurrencySemaphore = new(10, 10);
}