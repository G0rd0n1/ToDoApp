using Microsoft.AspNetCore.Mvc;
using ToDoApp.Interfaces;
using ToDoApp.Models;

namespace ToDoApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var users = await _userRepository.GetAllUsersAndTheirTasks();
            return Ok(users);
        }

        [HttpGet("{userId}/Tasks")]
        public async Task<IActionResult> GetTasksByUserID(int userId)
        {
            var tasks = await _userRepository.GetTasksByUserID(userId);
            return Ok(tasks);
        }

        [HttpPost("AddUser")]
        public async Task<IActionResult> AddUser([FromBody] UserModel user)
        {
            try
            {
                var addedUser = await _userRepository.AddUser(user);
                return Ok(addedUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error adding user: {ex.Message}");
            }
        }

        [HttpPut("{userId}/UpdateUser")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserModel UserToUpdate)
        {
            try
            {
                UserToUpdate.UserID = userId;
                await _userRepository.UpdateUser(UserToUpdate);
                return Ok(UserToUpdate);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating user: {ex.Message}");
            }
        }

        [HttpDelete("{userId}/DeleteUser")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                await _userRepository.DeleteUser(userId);
                return Ok($"User with ID {userId} and all associated tasks deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting user and tasks: {ex.Message}");
            }
        }


        [HttpPost("{userId}/AddTask")]
        public async Task<IActionResult> AddTask(int userId, [FromBody] ToDoTaskModel task)
        {
            try
            {
                task.UserID = userId;
                var addedTask = await _userRepository.AddTask(task);
                return Ok(addedTask);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error adding task: {ex.Message}");
            }
        }


        [HttpPut("{userId}/UpdateTask/{id}")]
        public async Task<IActionResult> UpdateTask(int userId, int id, [FromBody] ToDoTaskModel taskToUpdate)
        {
            try
            {
                taskToUpdate.UserID = userId;
                taskToUpdate.ID = id;
                await _userRepository.UpdateTask(id, taskToUpdate);
                return Ok(taskToUpdate);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating task: {ex.Message}");
            }
        }

        [HttpDelete("{userId}/DeleteTask/{id}")]
        public async Task<IActionResult> DeleteTask(int userId, int id)
        {
            try
            {
                await _userRepository.DeleteTask(userId, id);
                return Ok($"Task with ID {id} deleted successfully for user with ID {userId}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting task: {ex.Message}");
            }
        }

    }
}
