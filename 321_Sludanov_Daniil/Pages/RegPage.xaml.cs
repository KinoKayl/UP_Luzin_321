using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace _UP_Luzin_321.Pages
{
    public partial class RegPage : Page
    {
        private static readonly Regex FIOregex = new Regex(@"^[А-ЯЁ][а-яё]+$");
        private static readonly Regex PasswordRegex = new Regex(@"^(?=.*\d)[a-zA-Z0-9]{6,}$");
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        private readonly Entities db;

        public RegPage()
        {
            InitializeComponent();
            db = new Entities();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            LoginBox.Text = string.Empty;
            PassswordBox.Password = string.Empty;
            PassswordBoxAccept.Password = string.Empty;
            FIOBox.Text = string.Empty;
            Roles.SelectedIndex = 1;
            NavigationService.GoBack();
        }

        private void LoginBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(LoginBox.Text) && LoginBox.Text.Length > 0)
            {
                LoginBoxText.Visibility = Visibility.Collapsed;
            }
            else
            {
                LoginBoxText.Visibility = Visibility.Visible;
            }
        }

        private void LoginBoxText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LoginBox.Focus();
        }

        private void PassswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(PassswordBox.Password) && PassswordBox.Password.Length > 0)
            {
                PasswordBoxText.Visibility = Visibility.Collapsed;
            }
            else
            {
                PasswordBoxText.Visibility = Visibility.Visible;
            }
        }

        private void PasswordBoxText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PassswordBox.Focus();
        }

        private void PassswordBoxAccept_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(PassswordBoxAccept.Password) && PassswordBoxAccept.Password.Length > 0)
            {
                PasswordBoxAcceptText.Visibility = Visibility.Collapsed;
            }
            else
            {
                PasswordBoxAcceptText.Visibility = Visibility.Visible;
            }
        }

        private void PasswordBoxAcceptText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PassswordBoxAccept.Focus();
        }

        private void FIOBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(FIOBox.Text) && FIOBox.Text.Length > 0)
            {
                FIOBoxText.Visibility = Visibility.Collapsed;
            }
            else
            {
                FIOBoxText.Visibility = Visibility.Visible;
            }
        }

        private void FIOBoxText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FIOBox.Focus();
        }

        private void RegButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text;
            string password = GetHash(PassswordBox.Password);
            string passwordAccept = GetHash(PassswordBoxAccept.Password);
            string FIO = FIOBox.Text;
            int Role = (Roles.SelectedItem as ComboBoxItem)?.Tag as string == "1" ? 1 : 2;

            try
            {
                if (login.Length < 5)
                {
                    MessageBox.Show("Логин не может быть меньше 5 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!IsValidLogin(login))
                {
                    MessageBox.Show("Логин должен быть либо email, либо логином, состоящим из 5 и более символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (db.Users.Any(u => u.Login == login))
                {
                    MessageBox.Show("Пользователь с таким логином уже зарегистрирован в системе!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    LoginBox.Text = "";
                    return;
                }

                if (!PasswordRegex.IsMatch(password))
                {
                    MessageBox.Show("Пароль не может быть меньше 6 символов! Также, пароль включает только английские символы!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (passwordAccept != password)
                {
                    MessageBox.Show("Пароль не совпадает с введённым!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Roles.SelectedIndex == -1)
                {
                    MessageBox.Show("Выберите роль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!FIOcheck(FIO))
                {
                    MessageBox.Show("ФИО введено некорректно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newUser = new User
                {
                    Login = login,
                    Password = password,
                    FIO = FIO,
                    Role = Role
                };
                db.Users.Add(newUser);
                db.SaveChanges();

                MessageBox.Show("Регистрация завершена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var entityErrors in ex.EntityValidationErrors)
                {
                    foreach (var valError in entityErrors.ValidationErrors)
                    {
                        MessageBox.Show($"Ошибка: {valError.ErrorMessage}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private bool IsValidLogin(string login)
        {
            return login.Length >= 5 && (EmailRegex.IsMatch(login) || !string.IsNullOrWhiteSpace(login));
        }

        private bool FIOcheck(string FIO)
        {
            var splitFIO = FIO.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Подразумевается, что у пользователя нет отчества. В качестве данного условия, пришлось разделить ФИО на три части.
            if (splitFIO.Length < 2 || splitFIO.Length > 3)
            {
                return false;
            }

            foreach (var part in splitFIO)
            {
                if (string.IsNullOrWhiteSpace(part) || !FIOregex.IsMatch(part) || part.Length > 50)
                {
                    return false;
                }
            }
            return true;
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }
    }
}