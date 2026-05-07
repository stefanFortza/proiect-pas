using System.Threading.Tasks;
using Refit;

namespace ProiectSimulareLimbaj.Api;

public interface ISimulationApi
{
    [Get("/health")]
    Task<HealthResponse> CheckHealthAsync();

    [Get("/api/v1/simulation/step")]
    Task<SimulationResponse> GetNextStepAsync();
}
