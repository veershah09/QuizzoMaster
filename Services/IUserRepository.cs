using QuizApplicationMVC.Models;

namespace QuizApplicationMVC.Services
{
    public interface IUserRepository
    {
        Task<IEnumerable<Users>> GetAllUsersAsync();
        Task<Users> GetUserByIdAsync(int id);
        Task<Users> AuthenticateUserAsync(string email, string password);
        Task<string> AddUserAsync(Users user);
        Task<string> UpdateUserAsync(Users user);
        bool UserExists(int id);

        Task DeleteUserAsync(int id);
        
    }
}
