using AutoMapper;
using eCommerce.Core.DTO;
using eCommerce.Core.Entities;
using eCommerce.Core.RepositoryContracts;
using eCommerce.Core.ServiceContracts;

namespace eCommerce.Core.Services;

internal class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IMapper _mapper;
    public UsersService(IUsersRepository usersRepository, IMapper mapper)
    {
        _usersRepository = usersRepository;
        _mapper = mapper;

    }

    public async Task<AuthenticationResponse?> Login(LoginRequest loginRequest)
    {
        var user = await _usersRepository.GetUserByEmailAndPassword(loginRequest.Email, loginRequest.Password);

        if (user == null)
        {
            return null;
        }

       // return new AuthenticationResponse(user.UserId, user.Email, user.PersonName, user.Gender, "token", Success: true);
       return _mapper.Map<AuthenticationResponse>(user) with { Success = true, Token = "token"};
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



}
