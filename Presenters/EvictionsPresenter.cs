using CouseWork3Semester.Enums;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Models;
using CouseWork3Semester.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CouseWork3Semester.Presenters
{
    public class EvictionsPresenter
    {
        private readonly EvictionsView _view;
        private readonly IAccountingSystem _sys;
        private IEmployee _employee => _sys.GetCurrentEmployee();

        public EvictionsPresenter(EvictionsView view, IAccountingSystem accountingSystem)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _sys = accountingSystem ?? throw new ArgumentNullException(nameof(accountingSystem));

            WireEvents();
            LoadRooms();
            ApplyPermissions();
            LoadEvictions();
        }

        private void WireEvents()
        {
            _view.CreateEvictionButton.Click += (s, e) => CreateEviction();
            _view.RoomSelectCombo.SelectionChanged += (s, e) => LoadRoomOccupants();
        }

        private void ApplyPermissions()
        {
            bool canCreate = false;

            if (_sys.PermissionManager != null && _employee != null)
            {
                canCreate = _sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageEvictions");
            }

            if (!canCreate && _employee != null)
            {
                var roleName = _employee.Role;
                if (roleName is UserRole.Administrator || roleName is UserRole.Commandant)
                    canCreate = true;
            }

            _view.CreateEvictionButton.IsEnabled = canCreate;
            _view.StatusText.Text = canCreate
                ? "You have permission to add evictions."
                : "View-only: creation disabled for your role.";
        }

        private void LoadRooms()
        {
            try
            {
                var rooms = _sys.DormitoryRegistry.GetAllDormitories()
                    .SelectMany(d => d.GetAllRooms().Select(r => new RoomComboItem
                    {
                        DormitoryNumber = d.Number,
                        Room = r,
                        Display = $"Dorm {d.Number} | Room {r.Number} | Occupants: {r.GetAllOccupants().Count}"
                    }))
                    .ToList();
                _view.RoomSelectCombo.ItemsSource = rooms;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load rooms: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRoomOccupants()
        {
            try
            {
                var roomItem = _view.RoomSelectCombo.SelectedItem as RoomComboItem;
                var room = roomItem?.Room;
                if (room == null)
                {
                    _view.OccupantsSelectList.ItemsSource = null;
                    return;
                }

                var occupants = room.GetAllOccupants()
                    .Select(o => new OccupantViewItem
                    {
                        Id = o.Id,
                        FullName = o.FullName,
                        Type = o.GetOccupantType(),
                        BirthDate = o.BirthDate.ToString("dd.MM.yyyy"),
                        Source = o
                    })
                    .ToList();

                _view.OccupantsSelectList.ItemsSource = occupants;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load occupants: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadEvictions()
        {
            try
            {
                var rows = _sys.SettlementEvictionService.Evictions
                    .Select(ev => new EvictionRow
                    {
                        Id = ev.Id,
                        EvictionDate = ev.EvictionDate.ToString("dd.MM.yyyy"),
                        RoomNumber = ev.Room?.Number ?? 0,
                        OccupantsCount = ev.Occupants?.Count ?? 0,
                        Reason = ev.Reason ?? ""
                    })
                    .OrderByDescending(r => r.EvictionDate)
                    .ToList();

                _view.EvictionsGrid.ItemsSource = rows;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load evictions: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateEviction()
        {
            try
            {
                if (!_view.CreateEvictionButton.IsEnabled)
                {
                    MessageBox.Show("No permission to add evictions.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var roomItem = _view.RoomSelectCombo.SelectedItem as RoomComboItem;
                var room = roomItem?.Room;
                var reason = _view.ReasonTextBox.Text?.Trim() ?? "";
                var date = _view.EvictionDatePicker.SelectedDate ?? DateTime.Now;

                var selectedOccupants = _view.OccupantsSelectList.SelectedItems
                    .Cast<OccupantViewItem>()
                    .Select(x => x.Source)
                    .ToList();

                if (room == null)
                {
                    MessageBox.Show("Select a room.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (string.IsNullOrWhiteSpace(reason))
                {
                    MessageBox.Show("Enter eviction reason.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (!selectedOccupants.Any())
                {
                    MessageBox.Show("Select occupants to evict.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var related = _sys.SettlementEvictionService
                    .GetSettlementsForRoom(room)
                    .OrderByDescending(s => s.SettlementDate)
                    .FirstOrDefault();

                var eviction = new Eviction();
                eviction.InitializeEviction(Guid.NewGuid(), selectedOccupants, room, reason, related);

                _sys.SettlementEvictionService.AddEviction(eviction);

                _view.ReasonTextBox.Text = "";
                _view.EvictionDatePicker.SelectedDate = null;
                _view.OccupantsSelectList.UnselectAll();
                _view.StatusText.Text = $"Eviction added for room {room.Number}.";

                LoadEvictions();
                LoadRoomOccupants();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add eviction: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private class RoomComboItem
        {
            public string Display { get; set; }
            public IRoom Room { get; set; }
            public int DormitoryNumber { get; set; }
        }

        private class OccupantViewItem
        {
            public Guid Id { get; set; }
            public string FullName { get; set; }
            public string Type { get; set; }
            public string BirthDate { get; set; }
            public IRoomOccupant Source { get; set; }
        }

        private class EvictionRow
        {
            public Guid Id { get; set; }
            public string EvictionDate { get; set; }
            public int RoomNumber { get; set; }
            public int OccupantsCount { get; set; }
            public string Reason { get; set; }
        }
    }
}