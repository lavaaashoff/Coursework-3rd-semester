using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CouseWork3Semester.Enums;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Views;

namespace CouseWork3Semester.Presenters
{
    public class ResidentsPresenter
    {
        private readonly ResidentsView _view;
        private readonly IOccupantRegistry _registry;
        private readonly IPermissionManager _permissionManager;
        private readonly IPassportValidator _passportValidator;
        private readonly IEmployee _currentEmployee;

        public ResidentsPresenter(
            ResidentsView view,
            IOccupantRegistry registry,
            IPermissionManager permissionManager,
            IPassportValidator passportValidator,
            IEmployee currentEmployee)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
            _passportValidator = passportValidator ?? throw new ArgumentNullException(nameof(passportValidator));
            _currentEmployee = currentEmployee ?? throw new ArgumentNullException(nameof(currentEmployee));

            WireEvents();
            ApplyPermissions();
            InitGenderCombo();
            LoadResidents();
            LoadChildren();
        }

        private void WireEvents()
        {
            _view.AddResidentButton.Click += (s, e) => AddResident();
            _view.UpdateResidentButton.Click += (s, e) => UpdateSelectedResident();
            _view.RemoveResidentButton.Click += (s, e) => RemoveSelectedResident();
            _view.RefreshResidentsButton.Click += (s, e) => { LoadResidents(); LoadChildren(); };
            _view.AddChildButton.Click += (s, e) => AddChild();
            _view.ResidentsGrid.SelectionChanged += (s, e) => FillEditFieldsFromSelection();
        }

        private void ApplyPermissions()
        {
            var canManage = _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageOccupants");

            _view.AddResidentGroup.Visibility = canManage ? Visibility.Visible : Visibility.Collapsed;
            _view.EditResidentGroup.Visibility = canManage ? Visibility.Visible : Visibility.Collapsed;
            _view.AddChildGroup.Visibility = canManage ? Visibility.Visible : Visibility.Collapsed;
        }

        private void InitGenderCombo()
        {
            _view.GenderComboBox.ItemsSource = Enum.GetValues(typeof(Gender));
            _view.GenderComboBox.SelectedIndex = 0;
        }

        private void LoadResidents()
        {
            var residents = _registry.GetAllResidents() ?? new List<IResident>();
            var items = residents.Select(r => new ResidentViewItem
            {
                Id = r.Id,
                RegistrationNumber = r.RegistrationNumber,
                FullName = r.FullName,
                Gender = r.Gender.ToString(),
                BirthDateString = r.BirthDate.ToString("dd.MM.yyyy"),
                PassportShort = r.Passport != null ? $"{r.Passport.Series} {r.Passport.Number}" : "",
                IssuedBy = r.Passport != null ? r.Passport.IssuedBy : "",
                CheckInDateString = r.CheckInDate.ToString("dd.MM.yyyy"),
                WorkStatus = r.WorkStatus,
                Workplace = r.Workplace ?? "",
                StudyStatus = r.StudyStatus,
                StudyPlace = r.StudyPlace ?? ""
            }).OrderBy(x => x.RegistrationNumber).ToList();

            _view.ResidentsGrid.ItemsSource = items;
            ClearAddResidentFields();
            ClearEditResidentFields();
        }

        private void LoadChildren()
        {
            var children = _registry.GetAllChildren() ?? new List<IChild>();
            var residents = _registry.GetAllResidents() ?? new List<IResident>();

            var items = children.Select(c =>
            {
                var parent = residents.FirstOrDefault(r => r.Id == c.ParentResidentId);
                return new ChildViewItem
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    BirthDateString = c.BirthDate.ToString("dd.MM.yyyy"),
                    ParentRegNumber = parent?.RegistrationNumber ?? 0,
                    ParentFullName = parent?.FullName ?? "(unknown)"
                };
            }).OrderBy(x => x.ParentRegNumber).ToList();

            _view.ChildrenGrid.ItemsSource = items;
            ClearAddChildFields();
        }

        private void AddResident()
        {
            try
            {
                if (!_permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageOccupants"))
                {
                    MessageBox.Show("You do not have permission to add residents.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(_view.RegNumberTextBox.Text, out var regNumber) || regNumber <= 0)
                {
                    MessageBox.Show("Registration number must be a positive integer.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var fullName = _view.FullNameTextBox.Text?.Trim();
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    MessageBox.Show("Full name is required.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (_view.GenderComboBox.SelectedItem is not Gender gender)
                {
                    MessageBox.Show("Select gender.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var birthDate = _view.BirthDatePicker.SelectedDate ?? DateTime.MinValue;
                if (birthDate == DateTime.MinValue || birthDate > DateTime.Now)
                {
                    MessageBox.Show("Birth date is invalid.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var series = _view.PassportSeriesTextBox.Text?.Trim() ?? "";
                var number = _view.PassportNumberTextBox.Text?.Trim() ?? "";
                var issuedBy = _view.PassportIssuedByTextBox.Text?.Trim() ?? "";
                var issueDate = _view.PassportIssueDatePicker.SelectedDate ?? DateTime.Now;

                var passport = new CouseWork3Semester.Models.Passport(series, number, issueDate, issuedBy);

                // Валидация паспорта интерфейсом валидатора
                var format = _passportValidator.CheckFormat(passport);
                if (!format.IsValid)
                {
                    MessageBox.Show($"Passport invalid: {string.Join("; ", format.Errors)}", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var checkIn = _view.CheckInDatePicker.SelectedDate ?? DateTime.Now;
                if (checkIn > DateTime.Now.AddDays(1))
                {
                    MessageBox.Show("Check-in date cannot be in the future.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var resident = new CouseWork3Semester.Models.Resident(
                    regNumber, fullName, gender, birthDate, passport, checkIn);

                _registry.AddResident(resident);
                LoadResidents();
                LoadChildren();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add resident: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSelectedResident()
        {
            try
            {
                if (!_permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageOccupants"))
                {
                    MessageBox.Show("You do not have permission to edit residents.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.ResidentsGrid.SelectedItem is not ResidentViewItem item)
                {
                    MessageBox.Show("Select a resident first.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Находим объект IResident по Id
                var resident = _registry.GetAllResidents().FirstOrDefault(r => r.Id == item.Id);
                if (resident == null)
                {
                    MessageBox.Show("Resident not found.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Обновляем только изменяемые поля согласно модели
                resident.WorkStatus = _view.WorkStatusCheckBox.IsChecked == true;
                resident.Workplace = _view.WorkplaceTextBox.Text?.Trim();
                resident.StudyStatus = _view.StudyStatusCheckBox.IsChecked == true;
                resident.StudyPlace = _view.StudyPlaceTextBox.Text?.Trim();

                LoadResidents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update resident: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveSelectedResident()
        {
            try
            {
                if (!_permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageOccupants"))
                {
                    MessageBox.Show("You do not have permission to remove residents.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.ResidentsGrid.SelectedItem is not ResidentViewItem item)
                {
                    MessageBox.Show("Select a resident first.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Внимание: удаление жильца оставит детей "без родителя".
                // В текущей модели связей пересборка детей не реализована.
                var ok = _registry.RemoveOccupant(item.Id);
                if (!ok)
                {
                    MessageBox.Show("Unable to remove resident.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoadResidents();
                LoadChildren();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove resident: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddChild()
        {
            try
            {
                if (!_permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageOccupants"))
                {
                    MessageBox.Show("You do not have permission to add children.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.ResidentsGrid.SelectedItem is not ResidentViewItem parentItem)
                {
                    MessageBox.Show("Select a parent resident in the Residents grid.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var childName = _view.ChildFullNameTextBox.Text?.Trim();
                if (string.IsNullOrWhiteSpace(childName))
                {
                    MessageBox.Show("Child full name is required.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var childBirth = _view.ChildBirthDatePicker.SelectedDate ?? DateTime.MinValue;
                if (childBirth == DateTime.MinValue || childBirth > DateTime.Now)
                {
                    MessageBox.Show("Child birth date is invalid.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var child = new CouseWork3Semester.Models.Child(childName, childBirth, parentItem.Id);
                _registry.AddChild(child);

                LoadChildren();
                ClearAddChildFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add child: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillEditFieldsFromSelection()
        {
            if (_view.ResidentsGrid.SelectedItem is ResidentViewItem item)
            {
                var resident = _registry.GetAllResidents().FirstOrDefault(r => r.Id == item.Id);
                if (resident != null)
                {
                    _view.WorkStatusCheckBox.IsChecked = resident.WorkStatus;
                    _view.WorkplaceTextBox.Text = resident.Workplace ?? "";
                    _view.StudyStatusCheckBox.IsChecked = resident.StudyStatus;
                    _view.StudyPlaceTextBox.Text = resident.StudyPlace ?? "";
                }
            }
        }

        private void ClearAddResidentFields()
        {
            _view.RegNumberTextBox.Text = "";
            _view.FullNameTextBox.Text = "";
            _view.GenderComboBox.SelectedIndex = 0;
            _view.BirthDatePicker.SelectedDate = null;
            _view.PassportSeriesTextBox.Text = "";
            _view.PassportNumberTextBox.Text = "";
            _view.PassportIssuedByTextBox.Text = "";
            _view.PassportIssueDatePicker.SelectedDate = null;
            _view.CheckInDatePicker.SelectedDate = null;
        }

        private void ClearEditResidentFields()
        {
            _view.WorkStatusCheckBox.IsChecked = false;
            _view.WorkplaceTextBox.Text = "";
            _view.StudyStatusCheckBox.IsChecked = false;
            _view.StudyPlaceTextBox.Text = "";
        }

        private void ClearAddChildFields()
        {
            _view.ChildFullNameTextBox.Text = "";
            _view.ChildBirthDatePicker.SelectedDate = null;
        }

        private class ResidentViewItem
        {
            public Guid Id { get; set; }
            public int RegistrationNumber { get; set; }
            public string FullName { get; set; }
            public string Gender { get; set; }
            public string BirthDateString { get; set; }
            public string PassportShort { get; set; }
            public string IssuedBy { get; set; }
            public string CheckInDateString { get; set; }
            public bool WorkStatus { get; set; }
            public string Workplace { get; set; }
            public bool StudyStatus { get; set; }
            public string StudyPlace { get; set; }
        }

        private class ChildViewItem
        {
            public Guid Id { get; set; }
            public string FullName { get; set; }
            public string BirthDateString { get; set; }
            public int ParentRegNumber { get; set; }
            public string ParentFullName { get; set; }
        }
    }
}