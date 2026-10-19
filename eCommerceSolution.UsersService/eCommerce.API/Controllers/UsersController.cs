

namespace eCommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpGet("{userID}")]
    public async Task<IActionResult> GetUserByUserID(Guid userID)
    {
        if (userID == Guid.Empty)
        {
            return BadRequest("Invalid UserID");
        }

        UserDTO? resposne = await _usersService.GetUserByUserID(userID);

        if (resposne == null)
        {
            return NotFound(response);
        }

        return Ok(resposne);
    }
}
