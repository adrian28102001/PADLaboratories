namespace JobManagementService.Saga;

public interface ISagaCoordinator
{
    Task CloseJobSaga(int jobId, bool shouldFail);
}