using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Views;

namespace CouseWork3Semester.Presenters
{
    public class RoomsPresenter
    {
        private readonly RoomsView _view;
        private readonly IDormitoryRegistry _registry;
        private readonly IPermissionManager _permissionManager;
        private readonly IEmployee _currentEmployee;

        private IDormitory _selectedDormitory;

        public RoomsPresenter(
            RoomsView view,
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
            LoadDormitoriesDropdown();
        }

        private void WireEvents()
        {
            _view.LoadRoomsButton.Click += (s, e) => LoadRooms();
            _view.AddRoomButton.Click += (s, e) => AddRoom();
            _view.UpdateRoomButton.Click += (s, e) => UpdateSelectedRoom();
            _view.RemoveRoomButton.Click += (s, e) => RemoveSelectedRoom();
            _view.RefreshButton.Click += (s, e) => LoadRooms();

            _view.RoomsGrid.SelectionChanged += (s, e) => FillEditFieldsFromSelection();
        }

        private void ApplyPermissions()
        {
            var canManageRooms = _permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageRooms");

            _view.AddRoomGroup.Visibility = canManageRooms ? Visibility.Visible : Visibility.Collapsed;
            _view.EditRoomGroup.Visibility = canManageRooms ? Visibility.Visible : Visibility.Collapsed;

            // Для Commandant/AdminStaff — просмотр.
            // Управление свободными местами делается через страницы заселения/выселения и сервисы, а не здесь.
        }

        private void LoadDormitoriesDropdown()
        {
            var dorms = _registry.GetAllDormitories()
                .OrderBy(d => d.Number)
                .Select(d => new DormitoryDropdownItem
                {
                    Number = d.Number,
                    Display = $"{d.Number} — {d.Address}"
                })
                .ToList();

            _view.DormitoryComboBox.ItemsSource = dorms;

            // Автовыбор первого, если есть
            if (dorms.Any())
            {
                _view.DormitoryComboBox.SelectedIndex = 0;
                _selectedDormitory = _registry.GetDormitoryByNumber(dorms[0].Number);
                LoadRooms();
            }
        }

        private void LoadRooms()
        {
            try
            {
                var selectedNumber = _view.DormitoryComboBox.SelectedValue;
                if (selectedNumber == null)
                {
                    MessageBox.Show("Select a dormitory first.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _selectedDormitory = _registry.GetDormitoryByNumber((int)selectedNumber);
                if (_selectedDormitory == null)
                {
                    MessageBox.Show("Dormitory not found.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var rooms = _selectedDormitory.GetAllRooms() ?? new List<IRoom>();

                var items = rooms.Select(MapToViewItem)
                                 .OrderBy(r => r.Number)
                                 .ToList();

                _view.RoomsGrid.ItemsSource = items;
                ClearAddFields();
                ClearEditFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load rooms: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private RoomViewItem MapToViewItem(IRoom room)
        {
            var occupants = room.GetAllOccupants() ?? new List<IRoomOccupant>();
            var available = room.GetAvailablePlacesCount();

            return new RoomViewItem
            {
                Number = room.Number,
                Floor = room.Floor,
                Area = room.Area,
                Type = room.Type,
                OccupantsCount = occupants.Count,
                AvailablePlaces = available
            };
        }

        private void AddRoom()
        {
            try
            {
                if (!_permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageRooms"))
                {
                    MessageBox.Show("You do not have permission to add rooms.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedDormitory == null)
                {
                    MessageBox.Show("Select a dormitory first.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!int.TryParse(_view.NewRoomNumberTextBox.Text, out var number) || number <= 0)
                {
                    MessageBox.Show("Room number must be a positive integer.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(_view.NewRoomFloorTextBox.Text, out var floor))
                {
                    MessageBox.Show("Floor must be an integer.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!double.TryParse(_view.NewRoomAreaTextBox.Text, out var area) || area <= 0)
                {
                    MessageBox.Show("Area must be a positive number.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(_view.NewRoomTypeTextBox.Text, out var type) || type <= 0)
                {
                    MessageBox.Show("Type (places) must be a positive integer.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Создаём комнату через реализацию IRoom
                var room = new Models.Room(number, area, type, floor);

                _selectedDormitory.AddRoom(room);

                ClearAddFields();
                LoadRooms();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add room: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSelectedRoom()
        {
            try
            {
                if (!_permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageRooms"))
                {
                    MessageBox.Show("You do not have permission to edit rooms.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedDormitory == null)
                {
                    MessageBox.Show("Select a dormitory first.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (_view.RoomsGrid.SelectedItem is not RoomViewItem item)
                {
                    MessageBox.Show("Select a room first.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Редактирование возможно ТОЛЬКО для пустых комнат (иначе интерфейс не позволяет изменить свойства)
                var existing = _selectedDormitory.GetAllRooms()?.FirstOrDefault(r => r.Number == item.Number);
                if (existing == null)
                {
                    MessageBox.Show("Room not found.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (existing.GetAllOccupants().Any())
                {
                    MessageBox.Show("Only empty rooms can be edited.", "Restriction",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Новые значения
                if (!int.TryParse(_view.EditRoomNumberTextBox.Text, out var newNumber) || newNumber <= 0)
                {
                    MessageBox.Show("Room number must be a positive integer.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(_view.EditRoomFloorTextBox.Text, out var newFloor))
                {
                    MessageBox.Show("Floor must be an integer.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!double.TryParse(_view.EditRoomAreaTextBox.Text, out var newArea) || newArea <= 0)
                {
                    MessageBox.Show("Area must be a positive number.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(_view.EditRoomTypeTextBox.Text, out var newType) || newType <= 0)
                {
                    MessageBox.Show("Type (places) must be a positive integer.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Реализация "редактирования": удалить пустую старую комнату и добавить новую с обновлёнными свойствами
                var removed = _selectedDormitory.RemoveRoom(existing.Number);
                if (!removed)
                {
                    MessageBox.Show("Failed to remove original room. Ensure it's empty.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newRoom = new Models.Room(newNumber, newArea, newType, newFloor);
                _selectedDormitory.AddRoom(newRoom);

                ClearEditFields();
                LoadRooms();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update room: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveSelectedRoom()
        {
            try
            {
                if (!_permissionManager.CanRolePerformAction(_currentEmployee.Role, "ManageRooms"))
                {
                    MessageBox.Show("You do not have permission to remove rooms.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedDormitory == null)
                {
                    MessageBox.Show("Select a dormitory first.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (_view.RoomsGrid.SelectedItem is not RoomViewItem item)
                {
                    MessageBox.Show("Select a room first.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var ok = _selectedDormitory.RemoveRoom(item.Number);
                if (!ok)
                {
                    MessageBox.Show("Unable to remove room. Ensure it has no occupants.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoadRooms();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove room: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillEditFieldsFromSelection()
        {
            if (_view.RoomsGrid.SelectedItem is RoomViewItem item)
            {
                _view.EditRoomNumberTextBox.Text = item.Number.ToString();
                _view.EditRoomFloorTextBox.Text = item.Floor.ToString();
                _view.EditRoomAreaTextBox.Text = item.Area.ToString("0.##");
                _view.EditRoomTypeTextBox.Text = item.Type.ToString();
            }
        }

        private void ClearAddFields()
        {
            _view.NewRoomNumberTextBox.Text = "";
            _view.NewRoomFloorTextBox.Text = "";
            _view.NewRoomAreaTextBox.Text = "";
            _view.NewRoomTypeTextBox.Text = "";
        }

        private void ClearEditFields()
        {
            _view.EditRoomNumberTextBox.Text = "";
            _view.EditRoomFloorTextBox.Text = "";
            _view.EditRoomAreaTextBox.Text = "";
            _view.EditRoomTypeTextBox.Text = "";
        }

        private class DormitoryDropdownItem
        {
            public int Number { get; set; }
            public string Display { get; set; }
        }

        private class RoomViewItem
        {
            public int Number { get; set; }
            public int Floor { get; set; }
            public double Area { get; set; }
            public int Type { get; set; }
            public int OccupantsCount { get; set; }
            public int AvailablePlaces { get; set; }
        }
    }
}