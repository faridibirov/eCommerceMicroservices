using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using System.Net;
using System.Net.Http.Json;

namespace BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;

    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserDTO?> GetUserByUserID(Guid userId)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userId}");

            if (!response.IsSuccessStatusCode)
            {
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
