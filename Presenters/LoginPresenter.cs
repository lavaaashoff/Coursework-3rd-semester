using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Views;
using System.Windows;

namespace CouseWork3Semester.Presenters
{
    public class LoginPresenter
    {
        private readonly IAuthManager _authManager;
        private readonly LoginView _view;

        public LoginPresenter(IAuthManager authManager, LoginView view)
        {
            _authManager = authManager;
            _view = view;

            // Подключаем обработчики событий
            _view.LoginButton.Click += OnLoginButtonClicked;
        }

        private void OnLoginButtonClicked(object sender, RoutedEventArgs e)
        {
            string username = _view.UsernameTextBox.Text;
            string password = _view.PasswordBox.Password;

            try
            {
                _authManager.Login(username, password);
                MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _view.Close();
            }
            catch (System.Exception ex)
            {
                _view.MessageTextBlock.Text = $"Error: {ex.Message}";
            }
        }
    }
}