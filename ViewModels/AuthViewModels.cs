using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using KnewFate.Services;

namespace KnewFate.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly IAuthenticationService _authService;
    private readonly ILocalizationService _localizationService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private bool _rememberMe = false;
    private bool _isPasswordHidden = true;
    private bool _isLoading = false;
    private string _errorMessage = string.Empty;
    private string _emailError = string.Empty;
    private string _passwordError = string.Empty;

    public event PropertyChangedEventHandler PropertyChanged;

    public LoginViewModel(IAuthenticationService authService, ILocalizationService localizationService)
    {
        _authService = authService;
        _localizationService = localizationService;
        
        LoginCommand = new Command(async () => await OnLoginAsync(), () => CanLogin);
        ForgotPasswordCommand = new Command(async () => await OnForgotPasswordAsync());
        NavigateToRegisterCommand = new Command(async () => await OnNavigateToRegisterAsync());
        TogglePasswordVisibilityCommand = new Command(OnTogglePasswordVisibility);
        ValidateCommand = new Command(OnValidate);
    }

    #region Properties

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
            EmailError = string.Empty;
            ((Command)LoginCommand).ChangeCanExecute();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
            PasswordError = string.Empty;
            ((Command)LoginCommand).ChangeCanExecute();
        }
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set
        {
            _rememberMe = value;
            OnPropertyChanged();
        }
    }

    public bool IsPasswordHidden
    {
        get => _isPasswordHidden;
        set
        {
            _isPasswordHidden = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PasswordToggleIcon));
        }
    }

    public string PasswordToggleIcon => IsPasswordHidden ? "👁️" : "🙈";

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotLoading));
            ((Command)LoginCommand).ChangeCanExecute();
        }
    }

    public bool IsNotLoading => !IsLoading;

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public string EmailError
    {
        get => _emailError;
        set
        {
            _emailError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasEmailError));
        }
    }

    public bool HasEmailError => !string.IsNullOrEmpty(EmailError);

    public string PasswordError
    {
        get => _passwordError;
        set
        {
            _passwordError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasPasswordError));
        }
    }

    public bool HasPasswordError => !string.IsNullOrEmpty(PasswordError);

    public bool CanLogin => !IsLoading && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && 
                           !HasEmailError && !HasPasswordError;

    #endregion

    #region Commands

    public ICommand LoginCommand { get; }
    public ICommand ForgotPasswordCommand { get; }
    public ICommand NavigateToRegisterCommand { get; }
    public ICommand TogglePasswordVisibilityCommand { get; }
    public ICommand ValidateCommand { get; }

    #endregion

    #region Command Handlers

    private async Task OnLoginAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            if (!ValidateInputs())
            {
                return;
            }

            var result = await _authService.LoginAsync(Email.Trim(), Password);

            if (result.IsSuccess)
            {
                // Navigate to main app
                await Shell.Current.GoToAsync("//dashboard");
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnForgotPasswordAsync()
    {
        await Shell.Current.GoToAsync("forgotpassword");
    }

    private async Task OnNavigateToRegisterAsync()
    {
        await Shell.Current.GoToAsync("register");
    }

    private void OnTogglePasswordVisibility()
    {
        IsPasswordHidden = !IsPasswordHidden;
    }

    private void OnValidate()
    {
        ValidateInputs();
        ((Command)LoginCommand).ChangeCanExecute();
    }

    #endregion

    #region Validation

    private bool ValidateInputs()
    {
        var isValid = true;

        // Email validation
        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = _localizationService.GetLocalizedString("EmailRequired");
            isValid = false;
        }
        else if (!IsValidEmail(Email))
        {
            EmailError = _localizationService.GetLocalizedString("InvalidEmailFormat");
            isValid = false;
        }

        // Password validation
        if (string.IsNullOrWhiteSpace(Password))
        {
            PasswordError = _localizationService.GetLocalizedString("PasswordRequired");
            isValid = false;
        }

        return isValid;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RegisterViewModel : INotifyPropertyChanged
{
    private readonly IAuthenticationService _authService;
    private readonly ILocalizationService _localizationService;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private bool _isPasswordHidden = true;
    private bool _isConfirmPasswordHidden = true;
    private bool _isLoading = false;
    private string _errorMessage = string.Empty;
    private string _firstNameError = string.Empty;
    private string _lastNameError = string.Empty;
    private string _emailError = string.Empty;
    private string _passwordError = string.Empty;
    private string _confirmPasswordError = string.Empty;

    public event PropertyChangedEventHandler PropertyChanged;

    public RegisterViewModel(IAuthenticationService authService, ILocalizationService localizationService)
    {
        _authService = authService;
        _localizationService = localizationService;
        
        RegisterCommand = new Command(async () => await OnRegisterAsync(), () => CanRegister);
        NavigateToLoginCommand = new Command(async () => await OnNavigateToLoginAsync());
        TogglePasswordVisibilityCommand = new Command(OnTogglePasswordVisibility);
        ToggleConfirmPasswordVisibilityCommand = new Command(OnToggleConfirmPasswordVisibility);
        ValidateCommand = new Command(OnValidate);
    }

    #region Properties

    public string FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            OnPropertyChanged();
            FirstNameError = string.Empty;
            ((Command)RegisterCommand).ChangeCanExecute();
        }
    }

    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged();
            LastNameError = string.Empty;
            ((Command)RegisterCommand).ChangeCanExecute();
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
            EmailError = string.Empty;
            ((Command)RegisterCommand).ChangeCanExecute();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
            PasswordError = string.Empty;
            ((Command)RegisterCommand).ChangeCanExecute();
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            _confirmPassword = value;
            OnPropertyChanged();
            ConfirmPasswordError = string.Empty;
            ((Command)RegisterCommand).ChangeCanExecute();
        }
    }

    public bool IsPasswordHidden
    {
        get => _isPasswordHidden;
        set
        {
            _isPasswordHidden = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PasswordToggleIcon));
        }
    }

    public bool IsConfirmPasswordHidden
    {
        get => _isConfirmPasswordHidden;
        set
        {
            _isConfirmPasswordHidden = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ConfirmPasswordToggleIcon));
        }
    }

    public string PasswordToggleIcon => IsPasswordHidden ? "👁️" : "🙈";
    public string ConfirmPasswordToggleIcon => IsConfirmPasswordHidden ? "👁️" : "🙈";

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotLoading));
            ((Command)RegisterCommand).ChangeCanExecute();
        }
    }

    public bool IsNotLoading => !IsLoading;

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    // Error properties for each field
    public string FirstNameError
    {
        get => _firstNameError;
        set
        {
            _firstNameError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasFirstNameError));
        }
    }

    public bool HasFirstNameError => !string.IsNullOrEmpty(FirstNameError);

    public string LastNameError
    {
        get => _lastNameError;
        set
        {
            _lastNameError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasLastNameError));
        }
    }

    public bool HasLastNameError => !string.IsNullOrEmpty(LastNameError);

    public string EmailError
    {
        get => _emailError;
        set
        {
            _emailError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasEmailError));
        }
    }

    public bool HasEmailError => !string.IsNullOrEmpty(EmailError);

    public string PasswordError
    {
        get => _passwordError;
        set
        {
            _passwordError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasPasswordError));
        }
    }

    public bool HasPasswordError => !string.IsNullOrEmpty(PasswordError);

    public string ConfirmPasswordError
    {
        get => _confirmPasswordError;
        set
        {
            _confirmPasswordError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasConfirmPasswordError));
        }
    }

    public bool HasConfirmPasswordError => !string.IsNullOrEmpty(ConfirmPasswordError);

    public bool CanRegister => !IsLoading && 
                              !string.IsNullOrWhiteSpace(FirstName) && 
                              !string.IsNullOrWhiteSpace(LastName) && 
                              !string.IsNullOrWhiteSpace(Email) && 
                              !string.IsNullOrWhiteSpace(Password) && 
                              !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                              !HasFirstNameError && !HasLastNameError && 
                              !HasEmailError && !HasPasswordError && !HasConfirmPasswordError;

    #endregion

    #region Commands

    public ICommand RegisterCommand { get; }
    public ICommand NavigateToLoginCommand { get; }
    public ICommand TogglePasswordVisibilityCommand { get; }
    public ICommand ToggleConfirmPasswordVisibilityCommand { get; }
    public ICommand ValidateCommand { get; }

    #endregion

    #region Command Handlers

    private async Task OnRegisterAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            if (!ValidateInputs())
            {
                return;
            }

            var result = await _authService.RegisterAsync(Email.Trim(), Password, ConfirmPassword, FirstName.Trim(), LastName.Trim());

            if (result.IsSuccess)
            {
                // Navigate to main app
                await Shell.Current.GoToAsync("//dashboard");
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Registration failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnNavigateToLoginAsync()
    {
        await Shell.Current.GoToAsync("login");
    }

    private void OnTogglePasswordVisibility()
    {
        IsPasswordHidden = !IsPasswordHidden;
    }

    private void OnToggleConfirmPasswordVisibility()
    {
        IsConfirmPasswordHidden = !IsConfirmPasswordHidden;
    }

    private void OnValidate()
    {
        ValidateInputs();
        ((Command)RegisterCommand).ChangeCanExecute();
    }

    #endregion

    #region Validation

    private bool ValidateInputs()
    {
        var isValid = true;

        // First name validation
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            FirstNameError = _localizationService.GetLocalizedString("FirstNameRequired");
            isValid = false;
        }

        // Last name validation
        if (string.IsNullOrWhiteSpace(LastName))
        {
            LastNameError = _localizationService.GetLocalizedString("LastNameRequired");
            isValid = false;
        }

        // Email validation
        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = _localizationService.GetLocalizedString("EmailRequired");
            isValid = false;
        }
        else if (!IsValidEmail(Email))
        {
            EmailError = _localizationService.GetLocalizedString("InvalidEmailFormat");
            isValid = false;
        }

        // Password validation
        if (string.IsNullOrWhiteSpace(Password))
        {
            PasswordError = _localizationService.GetLocalizedString("PasswordRequired");
            isValid = false;
        }
        else if (Password.Length < 8)
        {
            PasswordError = _localizationService.GetLocalizedString("PasswordTooShort");
            isValid = false;
        }

        // Confirm password validation
        if (string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ConfirmPasswordError = _localizationService.GetLocalizedString("PasswordRequired");
            isValid = false;
        }
        else if (Password != ConfirmPassword)
        {
            ConfirmPasswordError = _localizationService.GetLocalizedString("PasswordsDoNotMatch");
            isValid = false;
        }

        return isValid;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ForgotPasswordViewModel : INotifyPropertyChanged
{
    private readonly IAuthenticationService _authService;
    private readonly ILocalizationService _localizationService;
    private string _email = string.Empty;
    private bool _isLoading = false;
    private bool _isSuccess = false;
    private string _errorMessage = string.Empty;
    private string _emailError = string.Empty;

    public event PropertyChangedEventHandler PropertyChanged;

    public ForgotPasswordViewModel(IAuthenticationService authService, ILocalizationService localizationService)
    {
        _authService = authService;
        _localizationService = localizationService;
        
        SendResetCommand = new Command(async () => await OnSendResetAsync(), () => CanSendReset);
        BackToLoginCommand = new Command(async () => await OnBackToLoginAsync());
        ValidateCommand = new Command(OnValidate);
    }

    #region Properties

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
            EmailError = string.Empty;
            IsSuccess = false;
            ((Command)SendResetCommand).ChangeCanExecute();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotLoading));
            ((Command)SendResetCommand).ChangeCanExecute();
        }
    }

    public bool IsNotLoading => !IsLoading;

    public bool IsSuccess
    {
        get => _isSuccess;
        set
        {
            _isSuccess = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public string EmailError
    {
        get => _emailError;
        set
        {
            _emailError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasEmailError));
        }
    }

    public bool HasEmailError => !string.IsNullOrEmpty(EmailError);

    public bool CanSendReset => !IsLoading && !string.IsNullOrWhiteSpace(Email) && !HasEmailError;

    #endregion

    #region Commands

    public ICommand SendResetCommand { get; }
    public ICommand BackToLoginCommand { get; }
    public ICommand ValidateCommand { get; }

    #endregion

    #region Command Handlers

    private async Task OnSendResetAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            IsSuccess = false;

            if (!ValidateInputs())
            {
                return;
            }

            var success = await _authService.ForgotPasswordAsync(Email.Trim());

            if (success)
            {
                IsSuccess = true;
            }
            else
            {
                ErrorMessage = "Failed to send reset email. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to send reset email: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnBackToLoginAsync()
    {
        await Shell.Current.GoToAsync("login");
    }

    private void OnValidate()
    {
        ValidateInputs();
        ((Command)SendResetCommand).ChangeCanExecute();
    }

    #endregion

    #region Validation

    private bool ValidateInputs()
    {
        var isValid = true;

        // Email validation
        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = _localizationService.GetLocalizedString("EmailRequired");
            isValid = false;
        }
        else if (!IsValidEmail(Email))
        {
            EmailError = _localizationService.GetLocalizedString("InvalidEmailFormat");
            isValid = false;
        }

        return isValid;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
