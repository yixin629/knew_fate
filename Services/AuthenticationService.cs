using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Linq;
using SQLite;
using KnewFate.Models;

namespace KnewFate.Services;

public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(string email, string password, string confirmPassword, string firstName, string lastName);
    Task<bool> LogoutAsync();
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    Task<bool> IsAuthenticatedAsync();
    Task<User> GetCurrentUserAsync();
    Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
    event EventHandler<AuthStateChangedEventArgs> AuthStateChanged;
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IDatabaseService _databaseService;
    private readonly IPreferences _preferences;
    private readonly ISecureStorage _secureStorage;
    private User _currentUser;

    public event EventHandler<AuthStateChangedEventArgs> AuthStateChanged;

    public AuthenticationService(IDatabaseService databaseService, IPreferences preferences, ISecureStorage secureStorage)
    {
        _databaseService = databaseService;
        _preferences = preferences;
        _secureStorage = secureStorage;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthResult(false, "Email and password are required");
            }

            if (!IsValidEmail(email))
            {
                return new AuthResult(false, "Please enter a valid email address");
            }

            // Get user from database
            var user = await _databaseService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return new AuthResult(false, "Invalid email or password");
            }

            // Verify password
            if (!VerifyPassword(password, user.PasswordHash))
            {
                return new AuthResult(false, "Invalid email or password");
            }

            // Update last login
            user.LastLoginDate = DateTime.UtcNow;
            await _databaseService.UpdateUserAsync(user);

            // Store authentication token
            var token = GenerateAuthToken(user.Id);
            await _secureStorage.SetAsync("auth_token", token);
            await _secureStorage.SetAsync("user_id", user.Id.ToString());

            _currentUser = user;
            AuthStateChanged?.Invoke(this, new AuthStateChangedEventArgs(true, user));

            return new AuthResult(true, "Login successful", user);
        }
        catch (Exception ex)
        {
            return new AuthResult(false, $"Login failed: {ex.Message}");
        }
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string confirmPassword, string firstName, string lastName)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                return new AuthResult(false, "All fields are required");
            }

            if (!IsValidEmail(email))
            {
                return new AuthResult(false, "Please enter a valid email address");
            }

            if (password != confirmPassword)
            {
                return new AuthResult(false, "Passwords do not match");
            }

            if (password.Length < 8)
            {
                return new AuthResult(false, "Password must be at least 8 characters long");
            }

            // Check if user already exists
            var existingUser = await _databaseService.GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                return new AuthResult(false, "An account with this email already exists");
            }

            // Create new user
            var user = new User
            {
                Email = email.ToLowerInvariant(),
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = HashPassword(password),
                CreatedDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow,
                IsActive = true,
                IsPremium = false
            };

            await _databaseService.AddUserAsync(user);

            // Auto-login after registration
            var loginResult = await LoginAsync(email, password);
            return loginResult;
        }
        catch (Exception ex)
        {
            return new AuthResult(false, $"Registration failed: {ex.Message}");
        }
    }

    public Task<bool> LogoutAsync()
    {
        try
        {
            _secureStorage.Remove("auth_token");
            _secureStorage.Remove("user_id");
            _currentUser = null;

            AuthStateChanged?.Invoke(this, new AuthStateChangedEventArgs(false, null));
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                return false;
            }

            var user = await _databaseService.GetUserByEmailAsync(email);
            if (user == null)
            {
                // Return true even if user doesn't exist for security
                return true;
            }

            // Generate reset token (in a real app, you'd send this via email)
            var resetToken = Guid.NewGuid().ToString();
            var resetRequest = new PasswordResetRequest
            {
                UserId = user.Id,
                Token = resetToken,
                RequestedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                IsUsed = false
            };

            await _databaseService.AddPasswordResetRequestAsync(resetRequest);

            // In a real app, send email with reset link
            // For demo purposes, we'll store the token temporarily
            await _secureStorage.SetAsync($"reset_token_{email}", resetToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
            {
                return false;
            }

            if (newPassword.Length < 8)
            {
                return false;
            }

            var resetRequest = await _databaseService.GetPasswordResetRequestAsync(token);
            if (resetRequest == null || resetRequest.IsUsed || resetRequest.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            var user = await _databaseService.GetUserByIdAsync(resetRequest.UserId);
            if (user == null)
            {
                return false;
            }

            // Update password
            user.PasswordHash = HashPassword(newPassword);
            await _databaseService.UpdateUserAsync(user);

            // Mark reset request as used
            resetRequest.IsUsed = true;
            await _databaseService.UpdatePasswordResetRequestAsync(resetRequest);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var token = await _secureStorage.GetAsync("auth_token");
            var userIdStr = await _secureStorage.GetAsync("user_id");

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userIdStr))
            {
                return false;
            }

            if (int.TryParse(userIdStr, out int userId))
            {
                var user = await _databaseService.GetUserByIdAsync(userId);
                if (user != null && user.IsActive)
                {
                    _currentUser = user;
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<User> GetCurrentUserAsync()
    {
        if (_currentUser != null)
        {
            return _currentUser;
        }

        if (await IsAuthenticatedAsync())
        {
            return _currentUser;
        }

        return null;
    }

    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return false;
            }

            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                return false;
            }

            if (newPassword.Length < 8)
            {
                return false;
            }

            user.PasswordHash = HashPassword(newPassword);
            await _databaseService.UpdateUserAsync(user);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var emailAttribute = new EmailAddressAttribute();
            return emailAttribute.IsValid(email);
        }
        catch
        {
            return false;
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var salt = Guid.NewGuid().ToString();
        var combined = password + salt;
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        var hash = Convert.ToBase64String(hashedBytes);
        return $"{salt}:{hash}";
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            var salt = parts[0];
            var hash = parts[1];

            using var sha256 = SHA256.Create();
            var combined = password + salt;
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            var newHash = Convert.ToBase64String(hashedBytes);

            return hash == newHash;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateAuthToken(int userId)
    {
        var tokenData = new { UserId = userId, Timestamp = DateTime.UtcNow };
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(tokenData)));
    }
}

public class AuthResult
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public User User { get; }

    public AuthResult(bool isSuccess, string message, User user = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        User = user;
    }
}

public class AuthStateChangedEventArgs : EventArgs
{
    public bool IsAuthenticated { get; }
    public User User { get; }

    public AuthStateChangedEventArgs(bool isAuthenticated, User user)
    {
        IsAuthenticated = isAuthenticated;
        User = user;
    }
}
