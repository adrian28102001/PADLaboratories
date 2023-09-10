namespace ApplicationManagementService.Configurations;

public class SemaphoreConfiguration
{
    public static readonly SemaphoreSlim ConcurrencySemaphore = new(10, 10);
}