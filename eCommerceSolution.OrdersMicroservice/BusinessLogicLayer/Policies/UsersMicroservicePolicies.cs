using System.Net;
using System.Text;
using System.Text.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class UsersMicroservicePolicies : IUsersMicroservicePolicies
{
    private readonly ILogger<UsersMicroservicePolicies> _logger;
    private readonly IPollyPolicies _pollyPolicies;

    public UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger, IPollyPolicies pollyPolicies)
    {
        _logger = logger;
        _pollyPolicies = pollyPolicies;
    }

    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy =
            Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .FallbackAsync(async (context) =>
            {
                _logger.LogWarning("Fallback triggered: The request failed, returning dummy data");
                UserDTO user = new UserDTO(PersonName: "Temporarily Unavailable (fallback)", Email: "Temporarily Unavailable (fallback)", 
                    Gender: "Temporarily Unavailable (fallback)", UserID: Guid.Empty);

                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json")
                };
            });
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        var retryPolicy = _pollyPolicies.GetRetryPolicy(5); 
        var circuitBreakerPolicy = _pollyPolicies.GetCircuitBreakerPolicy(3, TimeSpan.FromMinutes(2));
        var timeoutPolicy = _pollyPolicies.GetTimeoutPolicy(TimeSpan.FromSeconds(3));

        AsyncPolicyWrap<HttpResponseMessage> wrappedPolicy = Policy.WrapAsync(GetFallbackPolicy(), retryPolicy, circuitBreakerPolicy, timeoutPolicy);
        return wrappedPolicy;
    }
    
}
