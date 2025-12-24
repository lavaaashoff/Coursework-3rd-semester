using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Views;
using System;

namespace CouseWork3Semester.Presenters
{
    public class LoginPresenter
    {
        private readonly IAuthManager _authManager;
        private readonly LoginView _view;
        private readonly Action<IEmployee> _onLoginSuccess;

        public LoginPresenter(IAuthManager authManager, LoginView view, Action<IEmployee> onLoginSuccess)
        {
            _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _onLoginSuccess = onLoginSuccess ?? throw new ArgumentNullException(nameof(onLoginSuccess));

            _view.LoginButton.Click += OnLoginButtonClicked;
        }

        private void OnLoginButtonClicked(object sender, EventArgs e)
        {
            var username = _view.UsernameTextBox.Text;
            var password = _view.PasswordBox.Password;

            try
            {
                var currentEmployee = _authManager.Login(username, password);

                // Сначала скрываем Login, чтобы приложение не закрывалось раньше времени
                _view.Hide();

                // Открываем Dashboard через колбэк (там MainWindow будет назначен на Dashboard)
                _onLoginSuccess?.Invoke(currentEmployee);

                // И только потом закрываем Login
                _view.Close();
            }
            catch (Exception ex)
            {
                _view.MessageTextBlock.Text = $"Error: {ex.Message}";
            }
        }
    }
}