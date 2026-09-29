using eCommerce.Core.DTO;
using eCommerce.Core.Entities;
using eCommerce.Core.RepositoryContracts;

namespace eCommerce.Infrastructure.Repositories;

internal class UsersRepository : IUsersRepository
{
    public async Task<ApplicationUser?> AddUser(ApplicationUser user)
    {
        //Generate a new unique user Id for the user

        user.UserId = Guid.NewGuid();

        return user;
    }

    public async Task<ApplicationUser?> GetUserByEmailAndPassword(string? email, string? password)
    {
        return new ApplicationUser()
        {
            UserId = Guid.NewGuid(),
            Email = email,
            Password = password,
            PersonName = "Person Name",
            Gender = GenderOptions.Male.ToString(),
        };
    }


}
