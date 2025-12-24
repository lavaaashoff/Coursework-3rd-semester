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
        private readonly IAccountingSystem _sys;
        private IEmployee _employee => _sys.GetCurrentEmployee();

        public DormitoriesPresenter(DormitoriesView view, IAccountingSystem accountingSystem)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _sys = accountingSystem ?? throw new ArgumentNullException(nameof(accountingSystem));

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
            var canManage = _sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageDormitories");
            _view.AddDormitoryGroup.Visibility = canManage ? Visibility.Visible : Visibility.Collapsed;
            _view.RemoveDormitoryButton.Visibility = canManage ? Visibility.Visible : Visibility.Collapsed;

            var canEdit = _sys.PermissionManager.CanRolePerformAction(_employee.Role, "EditDormitories");
            _view.EditPhotoGroup.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadDormitories()
        {
            var items = _sys.DormitoryRegistry.GetAllDormitories()
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
                if (!_sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageDormitories"))
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

                var dormitory = new CouseWork3Semester.Models.Dormitory(number, address, photo ?? string.Empty);
                _sys.DormitoryRegistry.AddDormitory(dormitory);

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
                if (!_sys.PermissionManager.CanRolePerformAction(_employee.Role, "EditDormitories"))
                {
                    MessageBox.Show("You do not have permission to edit dormitories.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.DormitoriesGrid.SelectedItem is DormitoryViewItem item)
                {
                    var newPhoto = _view.EditPhotoPathTextBox.Text?.Trim();
                    var dorm = _sys.DormitoryRegistry.GetDormitoryByNumber(item.Number);
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
                if (!_sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageDormitories"))
                {
                    MessageBox.Show("You do not have permission to remove dormitories.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.DormitoriesGrid.SelectedItem is DormitoryViewItem item)
                {
                    var ok = _sys.DormitoryRegistry.RemoveDormitory(item.Number);
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