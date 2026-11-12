using System.Text.Json;
using AutoMapper;
using eCommerce.Core.DTO;
using eCommerce.Core.Entities;
using eCommerce.Core.RepositoryContracts;
using eCommerce.Core.ServiceContracts;
using Microsoft.Graph;

namespace eCommerce.Core.Services;

internal class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IMapper _mapper;
    private readonly GraphServiceClient _graphServiceClient;
    public UsersService(IUsersRepository usersRepository, IMapper mapper, GraphServiceClient graphServiceClient)
    {
        _usersRepository = usersRepository;
        _mapper = mapper;
        _graphServiceClient = graphServiceClient;

    }

    public async Task<AuthenticationResponse?> Login(LoginRequest loginRequest)
    {
        var user = await _usersRepository.GetUserByEmailAndPassword(loginRequest.Email, loginRequest.Password);

        if (user == null)
        {
            return null;
        }

        // return new AuthenticationResponse(user.UserId, user.Email, user.PersonName, user.Gender, "token", Success: true);
        return _mapper.Map<AuthenticationResponse>(user) with { Success = true, Token = "token" };
    }
    public async Task<AuthenticationResponse?> Register(RegisterRequest registerRequest)
    {
        ApplicationUser user = new ApplicationUser()
        {
            PersonName = registerRequest.PersonName,
            Email = registerRequest.Email,
            Password = registerRequest.Password,
            Gender = registerRequest.Gender.ToString(),
        };

        ApplicationUser? registeredUser = await _usersRepository.AddUser(user);

        if (registeredUser == null)
        {
            return null;
        }

        //Return success response

        //return new AuthenticationResponse
        //    (registeredUser.UserId, 
        //    registeredUser.Email, 
        //    registeredUser.PersonName, 
        //    registeredUser.Gender, 
        //    "token",
        //    Success: true);

        return _mapper.Map<AuthenticationResponse>(registeredUser) with { Success = true, Token = "token" };

    }

    public async Task<UserDTO> GetUserByUserID(Guid UserID)
    {
        //ApplicationUser? user = await _usersRepository.GetUserByUserID(UserID);

       var existingUser = await _graphServiceClient.Users[Convert.ToString(UserID)].GetAsync();

        if(existingUser == null)
        {
            return null;
        }
        Console.WriteLine(JsonSerializer.Serialize(existingUser));

        var user = new UserDTO(UserID, existingUser.UserPrincipalName, existingUser.GivenName, existingUser.Surname);

        return user;
    }


}
