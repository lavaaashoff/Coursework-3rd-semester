using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Views;
using System;
using CouseWork3Semester.Services;

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

            DebugLogger.Write($"LoginPresenter: login clicked for user={username}");

            try
            {
                var currentEmployee = _authManager.Login(username, password);

                _view.Hide();
                DebugLogger.Write("LoginPresenter: view hidden, invoking onLoginSuccess");

                var success = false;
                try
                {
                    _onLoginSuccess?.Invoke(currentEmployee);
                    success = true;
                    DebugLogger.Write("LoginPresenter: onLoginSuccess completed");
                }
                catch (Exception ex)
                {
                    DebugLogger.Write("LoginPresenter: onLoginSuccess failed", ex);
                    _view.MessageTextBlock.Text = $"Error: {ex.Message}";
                    _view.Show();
                }

                if (success)
                {
                    _view.Close();
                    DebugLogger.Write("LoginPresenter: login view closed");
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Write("LoginPresenter: auth failed", ex);
                _view.MessageTextBlock.Text = $"Error: {ex.Message}";
            }
        }
    }
}