using System.Threading.Tasks;
using Refit;

namespace ProiectSimulareLimbaj.Api;

public interface ISimulationApi
{
    [Get("/health")]
    Task<HealthResponse> CheckHealthAsync();

    [Post("/generate")]
    Task<GenerateResponse> GenerateAsync([Body] GenerateRequest request);
}
