using ToDoApp.Models;

namespace ToDoApp.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserModel>> GetAllUsersAndTheirTasks();
        Task<IEnumerable<ToDoTaskModel>> GetTasksByUserID(int userId);
        Task<UserModel> AddUser(UserModel user);
        Task UpdateUser(UserModel user);
        Task DeleteUser(int userId);
        Task<ToDoTaskModel> AddTask(ToDoTaskModel task);
        Task UpdateTask(int id, ToDoTaskModel task);
        Task DeleteTask(int userId, int id);
    }
}
