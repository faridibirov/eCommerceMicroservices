using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;


    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    public async Task<UserDTO?> GetUserByUserID(Guid userId)
    {
        try
        {

            string cacheKey = $"user:{userId}";
            string? cachedUser = await _distributedCache.GetStringAsync(cacheKey);

            if (cachedUser != null)
            {
                UserDTO? userFromCache = JsonSerializer.Deserialize<UserDTO>(cachedUser);

                return userFromCache;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/users/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    UserDTO? userFromFallback = await response.Content.ReadFromJsonAsync<UserDTO>();

                    if (userFromFallback == null)
                    {
                        throw new NotImplementedException("Fallback poilicy was not implemented");
                    }

                    return userFromFallback;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null, HttpStatusCode.BadRequest);
                }
                else
                {
                    // throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                    return new UserDTO(PersonName: "Temporarily Unavailable", Email: "Temporarily Unavailable", Gender: "Temporarily Unavailable", UserID: Guid.Empty);
                }
            }

            UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();

            if (user == null)
            {
                throw new ArgumentException("Invalid User ID");
            }

            string userJson = JsonSerializer.Serialize(user);

            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                 .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                 .SetSlidingExpiration(TimeSpan.FromSeconds(100));

            string cacheKeyToWrite = $"user:{userId}";

            await _distributedCache.SetStringAsync(cacheKeyToWrite, userJson, options);

            return user;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Request failed because of circuit breaker is in Open state. Returning dummy data.");
            return new UserDTO(PersonName: "Temporarily Unavailable (circuit breaker)", Email: "Temporarily Unavailable (circuit breaker)", Gender: "Temporarily Unavailable (circuit breaker)", UserID: Guid.Empty);
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Timoeout occured while fetching user data. Returning dummy data.");
            return new UserDTO(PersonName: "Temporarily Unavailable (timeout)", Email: "Temporarily Unavailable (timeout)", Gender: "Temporarily Unavailable (timeout)", UserID: Guid.Empty);
        }
    }
}
