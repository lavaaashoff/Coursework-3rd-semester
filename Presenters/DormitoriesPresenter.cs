using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Views;

namespace CouseWork3Semester.Presenters
{
    public class DormitoriesPresenter
    {
        private readonly DormitoriesView _view;
        private readonly IDormitoryRegistry _registry;
        private readonly IPermissionManager _permissionManager;
        private readonly IEmployee _currentEmployee;

        public DormitoriesPresenter(
            DormitoriesView view,
            IDormitoryRegistry registry,
            IPermissionManager permissionManager,
            IEmployee currentEmployee)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
            _currentEmployee = currentEmployee ?? throw new ArgumentNullException(nameof(currentEmployee));

            WireEvents();
            ApplyPermissions();
            LoadDormitories();
        }

        private void WireEvents()
        {
            _view.AddDormitoryButton.Click += (s, e) => AddDormitory();
            _view.UpdatePhotoButton.Click += (s, e) => UpdatePhoto();
            _view.RemoveDormitoryButton.Click += (s, e) => RemoveSelectedDormitory();
            _view.RefreshButton.Click += (s, e) => LoadDormitories();
        }

        private void ApplyPermissions()
        {
            // Управление общежитиями: добавление/удаление
            var canManage = _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageDormitories");
            _view.AddDormitoryGroup.Visibility = canManage ? Visibility.Visible : Visibility.Collapsed;
            _view.RemoveDormitoryButton.Visibility = canManage ? Visibility.Visible : Visibility.Collapsed;

            // Редактирование общежитий: фото
            var canEdit = _permissionManager.CanRolePerformAction(_currentEmployee.Role, "EditDormitories");
            _view.EditPhotoGroup.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadDormitories()
        {
            var items = _registry.GetAllDormitories()
                .Select(MapToViewItem)
                .OrderBy(d => d.Number)
                .ToList();

            _view.DormitoriesGrid.ItemsSource = items;
        }

        private DormitoryViewItem MapToViewItem(IDormitory dorm)
        {
            var rooms = dorm.GetAllRooms() ?? new List<IRoom>();
            var totalPlaces = dorm.GetTotalPlacesCount();
            var occupied = dorm.GetOccupiedPlacesCount();
            var available = dorm.GetAvailablePlacesCount();
            var roomsCount = rooms.Count;

            // Процент загрузки (интерфейс предоставляет метод GetOccupancyPercentage)
            var occupancy = dorm.GetOccupancyPercentage();

            return new DormitoryViewItem
            {
                Number = dorm.Number,
                Address = dorm.Address,
                PhotoPath = dorm.PhotoPath,
                RoomsCount = roomsCount,
                TotalPlaces = totalPlaces,
                OccupiedPlaces = occupied,
                AvailablePlaces = available,
                OccupancyPercent = Math.Round(occupancy, 1)
            };
        }

        private void AddDormitory()
        {
            try
            {
                if (!_permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageDormitories"))
                {
                    MessageBox.Show("You do not have permission to add dormitories.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(_view.NewDormNumberTextBox.Text, out var number) || number <= 0)
                {
                    MessageBox.Show("Dormitory number must be a positive integer.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var address = _view.NewDormAddressTextBox.Text?.Trim();
                var photo = _view.NewDormPhotoTextBox.Text?.Trim();

                if (string.IsNullOrWhiteSpace(address))
                {
                    MessageBox.Show("Address is required.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Создаём через реализацию IDormitory (Dormitory). Если у вас другой конструктор — подстройте.
                var dormitory = new Models.Dormitory(number, address, photo ?? string.Empty);

                _registry.AddDormitory(dormitory);

                _view.NewDormNumberTextBox.Text = "";
                _view.NewDormAddressTextBox.Text = "";
                _view.NewDormPhotoTextBox.Text = "";

                LoadDormitories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add dormitory: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePhoto()
        {
            try
            {
                if (!_permissionManager.CanRolePerformAction(_currentEmployee.Role, "EditDormitories"))
                {
                    MessageBox.Show("You do not have permission to edit dormitories.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.DormitoriesGrid.SelectedItem is DormitoryViewItem item)
                {
                    var newPhoto = _view.EditPhotoPathTextBox.Text?.Trim();
                    var dorm = _registry.GetDormitoryByNumber(item.Number);
                    if (dorm == null)
                    {
                        MessageBox.Show("Dormitory not found.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var ok = dorm.UpdatePhoto(newPhoto ?? string.Empty);
                    if (!ok)
                    {
                        MessageBox.Show("Invalid photo path.", "Validation error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    _view.EditPhotoPathTextBox.Text = "";
                    LoadDormitories();
                }
                else
                {
                    MessageBox.Show("Select a dormitory first.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update photo: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveSelectedDormitory()
        {
            try
            {
                if (!_permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageDormitories"))
                {
                    MessageBox.Show("You do not have permission to remove dormitories.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.DormitoriesGrid.SelectedItem is DormitoryViewItem item)
                {
                    var ok = _registry.RemoveDormitory(item.Number);
                    if (!ok)
                    {
                        MessageBox.Show("Unable to remove dormitory. Ensure it has no occupants.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    LoadDormitories();
                }
                else
                {
                    MessageBox.Show("Select a dormitory first.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove dormitory: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // View-модель для отображения в таблице
        private class DormitoryViewItem
        {
            public int Number { get; set; }
            public string Address { get; set; }
            public string PhotoPath { get; set; }
            public int RoomsCount { get; set; }
            public int TotalPlaces { get; set; }
            public int OccupiedPlaces { get; set; }
            public int AvailablePlaces { get; set; }
            public double OccupancyPercent { get; set; }
        }
    }
}