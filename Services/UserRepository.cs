using Microsoft.Data.SqlClient;
using System.Data;
using ToDoApp.Interfaces;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public class UserRepository : IUserRepository
    {
        private List<UserModel> _userList;
        private List<ToDoTaskModel> _todoList;
        private readonly IConfiguration _configuration;
        private readonly IDbConnection _connection;

        public UserRepository(IConfiguration configuration, IDbConnection connection)
        {
            _configuration = configuration;
            _connection = connection;
        }

        public async Task<IEnumerable<ToDoTaskModel>> GetTasksByUserID(int userId)
        {
            try
            {
                using (var _connection = new SqlConnection(_configuration.GetConnectionString("UserTasksDBConnection")))
                {
                    await _connection.OpenAsync();
                    var tasks = new List<ToDoTaskModel>();

                    
                    string userCheckQuery = @"
                    SELECT 
                        COUNT(*)
                    FROM 
                        UsersTable
                    WHERE 
                        UserID = @userId;";

                    using (var userCheckCommand = new SqlCommand(userCheckQuery, _connection))
                    {
                        userCheckCommand.Parameters.AddWithValue("@userId", userId);

                        int userCount = (int)await userCheckCommand.ExecuteScalarAsync();

                        if (userCount == 0)
                        {
                            throw new Exception($"User with ID {userId} does not exist.");
                        }
                    }

                    string query = @"
                    SELECT * FROM 
                        TaskTable
                    WHERE 
                        UserID = @userId";

                    using (var command = new SqlCommand(query, _connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var task = new ToDoTaskModel
                                {
                                    ID = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Notes = reader.GetString(2),
                                    Completed = reader.GetBoolean(3),
                                    UserID = reader.GetInt32(4)
                                };
                                tasks.Add(task);
                            }
                        }
                        return tasks;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve user tasks: " + ex.Message);
            }
        }

        public async Task<IEnumerable<UserModel>> GetAllUsersAndTheirTasks()
        {
            using (var _connection = new SqlConnection(_configuration.GetConnectionString("UserTasksDBConnection")))
            {
                await _connection.OpenAsync();
                string query = @"
                SELECT 
                    UsersTable.*, TaskTable.*
                FROM 
                    UsersTable
                LEFT JOIN 
                    TaskTable 
                ON 
                    UsersTable.UserID = TaskTable.UserID";
                try
                {
                    using (var command = new SqlCommand(query, _connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            List<UserModel> users = new List<UserModel>();
                            Dictionary<int, UserModel> userDictionary = new Dictionary<int, UserModel>();
                            UserModel existingUser = null;
                            int currentUserId = 0;

                            while (await reader.ReadAsync())
                            {
                                int userId = reader.GetInt32(0);

                                if (userId != currentUserId)
                                {
                                    currentUserId = userId;

                                    if (!userDictionary.TryGetValue(userId, out existingUser))
                                    {
                                        existingUser = new UserModel
                                        {
                                            UserID = userId,
                                            Username = reader.IsDBNull(1) ? null : reader.GetString(1),
                                            Password = reader.IsDBNull(2) ? null : reader.GetString(2),
                                            Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                                            Tasks = new List<ToDoTaskModel>()
                                        };
                                        users.Add(existingUser);
                                        userDictionary[userId] = existingUser;
                                    }
                                }

                                ToDoTaskModel task = new ToDoTaskModel
                                {
                                    ID = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                    Title = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    Notes = reader.IsDBNull(6) ? null : reader.GetString(6),
                                    Completed = reader.IsDBNull(7) ? false : reader.GetBoolean(7),
                                    UserID = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                                };
                                existingUser.Tasks.Add(task);
                            }
                            return users;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to retrieve users: " + ex.Message);
                }
            }
        }

        public async Task<UserModel> AddUser(UserModel user)
        {
            try
            {
                using (var _connection = new SqlConnection(_configuration.GetConnectionString("UserTasksDBConnection")))
                {
                    await _connection.OpenAsync();

                    string query = @"
                    INSERT INTO 
                        UsersTable 
                            (Username, Password, Email)
                    VALUES 
                            (@Username, @Password, @Email);
                    SELECT SCOPE_IDENTITY();";

                    using (var command = new SqlCommand(query, _connection))
                    {
                        command.Parameters.AddWithValue("@Username", user.Username);
                        command.Parameters.AddWithValue("@Password", user.Password);
                        command.Parameters.AddWithValue("@Email", user.Email);
                        user.UserID = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }
                }
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to add user: " + ex.Message);
            }
        }

        public async Task UpdateUser(UserModel user)
        {
            try
            {
                using (var _connection = new SqlConnection(_configuration.GetConnectionString("UserTasksDBConnection")))
                {
                    await _connection.OpenAsync();

                    string query = @"
                    UPDATE 
                        UsersTable
                    SET
                        Username = @Username, Password = @Password, Email = @Email
                    WHERE 
                        UserID = @UserID;";

                    using (var command = new SqlCommand(query, _connection))
                    {
                        command.Parameters.AddWithValue("@Username", user.Username);
                        command.Parameters.AddWithValue("@Password", user.Password);
                        command.Parameters.AddWithValue("@Email", user.Email);
                        command.Parameters.AddWithValue("@UserID", user.UserID);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update user: " + ex.Message);
            }
        }

        public async Task DeleteUser(int userId)
        {
            try
            {
                using (var _connection = new SqlConnection(_configuration.GetConnectionString("UserTasksDBConnection")))
                {
                    await _connection.OpenAsync();
                    using (var transaction = _connection.BeginTransaction())
                    {
                        try
                        {
                            string deleteTasksQuery = @"
                            DELETE FROM
                                TaskTable
                            WHERE
                                UserID = @UserID;";

                            using (var deleteTasksCommand = new SqlCommand(deleteTasksQuery, _connection, transaction))
                            {
                                deleteTasksCommand.Parameters.AddWithValue("@UserID", userId);
                                await deleteTasksCommand.ExecuteNonQueryAsync();
                            }

                            string deleteUserQuery = @"
                            DELETE FROM 
                                UsersTable
                            WHERE 
                                UserID = @UserID;";

                            using (var deleteUserCommand = new SqlCommand(deleteUserQuery, _connection, transaction))
                            {
                                deleteUserCommand.Parameters.AddWithValue("@UserID", userId);
                                await deleteUserCommand.ExecuteNonQueryAsync();
                            }
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete user and tasks: " + ex.Message);
            }
        }

        public async Task<ToDoTaskModel> AddTask(ToDoTaskModel task)
        {
            try
            {
                using (var _connection = new SqlConnection(_configuration.GetConnectionString("UserTasksDBConnection")))
                {
                    await _connection.OpenAsync();

                    string query = @"
                    INSERT INTO 
                        TaskTable 
                            (Title, Notes, Completed, UserID)
                    VALUES 
                            (@Title, @Notes, @Completed, @UserID);
                    SELECT SCOPE_IDENTITY();";

                    using (var command = new SqlCommand(query, _connection))
                    {
                        command.Parameters.AddWithValue("@Title", task.Title);
                        command.Parameters.AddWithValue("@Notes", task.Notes);
                        command.Parameters.AddWithValue("@Completed", task.Completed);
                        command.Parameters.AddWithValue("@UserID", task.UserID);

                        task.ID = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }
                }
                return task;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to add task: " + ex.Message);
            }
        }

        public async Task UpdateTask(int id, ToDoTaskModel task)
        {
            try
            {
                using (var _connection = new SqlConnection(_configuration.GetConnectionString("UserTasksDBConnection")))
                {
                    await _connection.OpenAsync();

                    string query = @"
                    UPDATE 
                        TaskTable
                    SET
                        Title = @Title, Notes = @Notes, Completed = @Completed
                    WHERE 
                        ID = @ID AND UserID = @UserID;";

                    using (var command = new SqlCommand(query, _connection))
                    {
                        command.Parameters.AddWithValue("@Title", task.Title);
                        command.Parameters.AddWithValue("@Notes", task.Notes);
                        command.Parameters.AddWithValue("@Completed", task.Completed);
                        command.Parameters.AddWithValue("@ID", id);
                        command.Parameters.AddWithValue("@UserID", task.UserID);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update task: " + ex.Message);
            }
        }

        public async Task DeleteTask(int userId, int id)
        {
            try
            {
                using (var _connection = new SqlConnection(_configuration.GetConnectionString("UserTasksDBConnection")))
                {
                    await _connection.OpenAsync();

                    string query = @"
                    DELETE FROM 
                        TaskTable
                    WHERE 
                        ID = @ID AND UserID = @UserID;";

                    using (var command = new SqlCommand(query, _connection))
                    {
                        command.Parameters.AddWithValue("@ID", id);
                        command.Parameters.AddWithValue("@UserID", userId);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete task: " + ex.Message);
            }
        }
    }
}

