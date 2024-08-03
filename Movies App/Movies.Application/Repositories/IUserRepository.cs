using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public interface IUserRepository
    {
        public string Login(User user);
        public string Register(User user);
    }
}
