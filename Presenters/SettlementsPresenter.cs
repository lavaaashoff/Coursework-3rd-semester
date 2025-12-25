using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CouseWork3Semester.Enums;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Models;
using CouseWork3Semester.Views;

namespace CouseWork3Semester.Presenters
{
    public class SettlementsPresenter
    {
        private readonly SettlementsView _view;
        private readonly IAccountingSystem _sys;
        private IEmployee _employee => _sys.GetCurrentEmployee();

        public SettlementsPresenter(SettlementsView view, IAccountingSystem accountingSystem)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _sys = accountingSystem ?? throw new ArgumentNullException(nameof(accountingSystem));

            WireEvents();
            LoadCombosAndLists();
            ApplyPermissions();
            LoadSettlements();
        }

        private void WireEvents()
        {
            _view.CreateSettlementButton.Click += (s, e) => CreateSettlement();
        }

        private void ApplyPermissions()
        {
            bool canCreate = false;

            if (_sys.PermissionManager != null && _employee != null)
            {
                canCreate = _sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageSettlements");
            }

            if (!canCreate && _employee != null)
            {
                var roleName = _employee.Role;
                if (roleName is UserRole.Administrator || roleName is UserRole.Commandant)
                    canCreate = true;
            }

            _view.CreateSettlementButton.IsEnabled = canCreate;
            _view.StatusText.Text = canCreate
                ? "You have permission to add settlements."
                : "View-only: creation disabled for your role.";
        }

        private void LoadCombosAndLists()
        {
            try
            {
                var rooms = _sys.DormitoryRegistry.GetAllDormitories()
                    .SelectMany(d => d.GetAllRooms().Select(r => new RoomComboItem
                    {
                        DormitoryNumber = d.Number,
                        Room = r,
                        Display = $"Dorm {d.Number} | Room {r.Number} | Places: {r.Type} | Free: {r.GetAvailablePlacesCount()}"
                    }))
                    .ToList();
                _view.RoomSelectCombo.ItemsSource = rooms;

                var docs = _sys.DocumentRegistry.GetAllDocuments() ?? new List<IDocument>();
                _view.DocumentSelectCombo.ItemsSource = docs;


                var occupants = _sys.OccupantRegistry.GetAllOccupants()
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
                MessageBox.Show($"Failed to load selection data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSettlements()
        {
            try
            {
                var rows = _sys.SettlementEvictionService.Settlements
                    .Select(s => new SettlementRow
                    {
                        Id = s.Id,
                        SettlementDate = s.SettlementDate.ToString("dd.MM.yyyy"),
                        RoomNumber = s.Room?.Number ?? 0,
                        OccupantsCount = s.Occupants?.Count ?? 0,
                        DocumentTitle = s.Document?.Title ?? ""
                    })
                    .OrderByDescending(r => r.SettlementDate)
                    .ToList();

                _view.SettlementsGrid.ItemsSource = rows;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load settlements: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateSettlement()
        {
            try
            {
                if (!_view.CreateSettlementButton.IsEnabled)
                {
                    MessageBox.Show("No permission to add settlements.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var roomItem = _view.RoomSelectCombo.SelectedItem as RoomComboItem;
                var room = roomItem?.Room;
                var doc = _view.DocumentSelectCombo.SelectedItem as IDocument;
                var date = _view.SettlementDatePicker.SelectedDate ?? DateTime.Now;

                var selectedOccupants = _view.OccupantsSelectList.SelectedItems.Cast<OccupantViewItem>()
                    .Select(x => x.Source)
                    .ToList();

                if (room == null)
                {
                    MessageBox.Show("Select a room.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (!selectedOccupants.Any())
                {
                    MessageBox.Show("Select occupants to settle.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var settlement = new Settlement();
                settlement.InitializeSettlement(selectedOccupants, room, doc, date);

                foreach (var occ in selectedOccupants)
                {
                    _sys.DocumentOccupantService?.AttachDocumentToOccupant(doc, occ);
                }

                _sys.SettlementEvictionService.AddSettlement(settlement);

                _view.SettlementDatePicker.SelectedDate = null;
                _view.OccupantsSelectList.UnselectAll();
                (_view.RoomSelectCombo as ComboBox)?.SelectedItem = null;
                (_view.DocumentSelectCombo as ComboBox)?.SelectedItem = null;

                LoadSettlements();
                _view.StatusText.Text = $"Settlement added for room {room.Number}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add settlement: {ex.Message}", "Error",
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

        private class SettlementRow
        {
            public Guid Id { get; set; }
            public string SettlementDate { get; set; }
            public int RoomNumber { get; set; }
            public int OccupantsCount { get; set; }
            public string DocumentTitle { get; set; }
        }
    }
}