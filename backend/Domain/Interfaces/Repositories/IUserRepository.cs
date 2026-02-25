using Domain.Entities;

namespace Domain.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserEntity>> GetAllAsync();
    }

}
