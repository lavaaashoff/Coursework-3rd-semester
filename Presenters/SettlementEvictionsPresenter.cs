using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Views;

namespace CouseWork3Semester.Presenters
{
    public class SettlementEvictionsPresenter
    {
        private readonly SettlementEvictionsView _view;
        private readonly IAccountingSystem _sys;
        private IEmployee _employee => _sys.GetCurrentEmployee();

        public SettlementEvictionsPresenter(SettlementEvictionsView view, IAccountingSystem accountingSystem)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _sys = accountingSystem ?? throw new ArgumentNullException(nameof(accountingSystem));

            WireEvents();
            ApplyPermissions();
            LoadDormitoriesForSettlement();
            LoadDocumentsDropdown();
            LoadOccupantsSelection();
            LoadSettlements();
            LoadEvictions();
            LoadDormitoriesForEviction();
        }

        private void WireEvents()
        {
            _view.RefreshButton.Click += (s, e) =>
            {
                LoadSettlements();
                LoadEvictions();
                LoadRoomOccupantsForEviction();
            };

            // Settlement create flow
            _view.DormitoryComboBoxSettle.SelectionChanged += (s, e) => LoadRoomsForSettlement();
            _view.RoomComboBoxSettle.SelectionChanged += (s, e) => { /* rooms chosen, no extra for now */ };
            _view.CreateSettlementButton.Click += (s, e) => CreateSettlement();

            // Settlement actions
            _view.PerformSettlementButton.Click += (s, e) => PerformSelectedSettlement();
            _view.CompleteSettlementButton.Click += (s, e) => CompleteSelectedSettlement();
            _view.CancelSettlementButton.Click += (s, e) => CancelSelectedSettlement();

            // Eviction create flow
            _view.DormitoryComboBox.SelectionChanged += (s, e) => LoadRoomsForEviction();
            _view.RoomComboBox.SelectionChanged += (s, e) => LoadRoomOccupantsForEviction();
            _view.CreateEvictionButton.Click += (s, e) => CreateEviction();
            _view.ExecuteEvictionButton.Click += (s, e) => ExecuteSelectedEviction();
            _view.RemoveEvictionButton.Click += (s, e) => RemoveSelectedEviction();
        }

        private void ApplyPermissions()
        {
            var canManageEvictions = _sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageEvictions");
            var canManageOccupants = _sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageOccupants");

            // Создание заселения доступно при одном из прав
            var allowCreateSettlement = canManageEvictions || canManageOccupants;
            _view.CreateSettlementGroup.Visibility = allowCreateSettlement ? Visibility.Visible : Visibility.Collapsed;

            // Действия с заселением только при ManageEvictions
            _view.SettlementActionsGroup.Visibility = canManageEvictions ? Visibility.Visible : Visibility.Collapsed;

            // Блок выселений — создание/выполнение/удаление только при ManageEvictions
            _view.CreateEvictionGroup.Visibility = canManageEvictions ? Visibility.Visible : Visibility.Collapsed;
        }

        // Settlement: loaders
        private void LoadDormitoriesForSettlement()
        {
            var dorms = _sys.DormitoryRegistry.GetAllDormitories()
                .OrderBy(d => d.Number)
                .Select(d => new DormItem { Number = d.Number, Display = $"{d.Number} — {d.Address}" })
                .ToList();

            _view.DormitoryComboBoxSettle.ItemsSource = dorms;
            if (dorms.Any())
            {
                _view.DormitoryComboBoxSettle.SelectedIndex = 0;
                LoadRoomsForSettlement();
            }
        }

        private void LoadRoomsForSettlement()
        {
            var selectedDormNum = _view.DormitoryComboBoxSettle.SelectedValue as int?;
            if (selectedDormNum == null) return;

            var dorm = _sys.DormitoryRegistry.GetDormitoryByNumber(selectedDormNum.Value);
            var rooms = (dorm?.GetAllRooms() ?? new List<IRoom>())
                .OrderBy(r => r.Number)
                .Select(r => new RoomItem { Number = r.Number, Display = $"№{r.Number} (floor {r.Floor}, places {r.Type})" })
                .ToList();

            _view.RoomComboBoxSettle.ItemsSource = rooms;
            if (rooms.Any())
            {
                _view.RoomComboBoxSettle.SelectedIndex = 0;
            }
        }

        private void LoadDocumentsDropdown()
        {
            var docs = _sys.DocumentRegistry.GetAllDocuments() ?? new List<IDocument>();
            var items = docs.Select(d => new DocumentItem
            {
                Id = d.Id,
                Display = $"{d.Title} ({d.Series}-{d.Number})"
            }).OrderBy(x => x.Display).ToList();

            _view.DocumentComboBox.ItemsSource = items;
            if (items.Any())
            {
                _view.DocumentComboBox.SelectedIndex = 0;
            }
        }

        private void LoadOccupantsSelection()
        {
            var all = _sys.OccupantRegistry.GetAllOccupants() ?? new List<IRoomOccupant>();
            var items = all.Select(o => new OccupantRow
            {
                Id = o.Id,
                FullName = o.FullName,
                BirthDate = o.BirthDate.ToString("dd.MM.yyyy"),
                Type = o.GetOccupantType()
            }).OrderBy(x => x.FullName).ToList();

            _view.OccupantsSelectList.ItemsSource = items;
        }

        private void LoadSettlements()
        {
            var settlements = _sys.SettlementEvictionService.GetActiveSettlements() ?? new List<ISettlement>();
            var rows = settlements.Select(s => new SettlementRow
            {
                Id = s.Id,
                SettlementDate = s.SettlementDate.ToString("dd.MM.yyyy"),
                RoomNumber = s.Room?.Number ?? 0,
                OccupantsCount = s.Occupants?.Count ?? 0,
                DocumentTitle = s.Document?.Title ?? "",
                Status = s is CouseWork3Semester.Models.Settlement st ? st.Status.ToString() : "Active"
            }).OrderBy(r => r.SettlementDate).ToList();

            _view.SettlementsGrid.ItemsSource = rows;
        }

        // Evictions: loaders
        private void LoadDormitoriesForEviction()
        {
            var dorms = _sys.DormitoryRegistry.GetAllDormitories()
                .OrderBy(d => d.Number)
                .Select(d => new DormItem { Number = d.Number, Display = $"{d.Number} — {d.Address}" })
                .ToList();

            _view.DormitoryComboBox.ItemsSource = dorms;
            if (dorms.Any())
            {
                _view.DormitoryComboBox.SelectedIndex = 0;
                LoadRoomsForEviction();
            }
        }

        private void LoadRoomsForEviction()
        {
            var selectedDormNum = _view.DormitoryComboBox.SelectedValue as int?;
            if (selectedDormNum == null) return;

            var dorm = _sys.DormitoryRegistry.GetDormitoryByNumber(selectedDormNum.Value);
            var rooms = (dorm?.GetAllRooms() ?? new List<IRoom>())
                .OrderBy(r => r.Number)
                .Select(r => new RoomItem { Number = r.Number, Display = $"№{r.Number} (floor {r.Floor}, places {r.Type})" })
                .ToList();

            _view.RoomComboBox.ItemsSource = rooms;
            if (rooms.Any())
            {
                _view.RoomComboBox.SelectedIndex = 0;
                LoadRoomOccupantsForEviction();
            }
        }

        private void LoadRoomOccupantsForEviction()
        {
            var selectedDormNum = _view.DormitoryComboBox.SelectedValue as int?;
            var selectedRoomNum = _view.RoomComboBox.SelectedValue as int?;
            if (selectedDormNum == null || selectedRoomNum == null)
            {
                _view.RoomOccupantsGrid.ItemsSource = null;
                return;
            }

            var dorm = _sys.DormitoryRegistry.GetDormitoryByNumber(selectedDormNum.Value);
            var room = dorm?.GetAllRooms()?.FirstOrDefault(r => r.Number == selectedRoomNum.Value);
            if (room == null)
            {
                _view.RoomOccupantsGrid.ItemsSource = null;
                return;
            }

            var occupants = _sys.SettlementEvictionService.GetOccupantsInRoom(room) ?? new List<IRoomOccupant>();
            var rows = occupants.Select(o => new OccupantRow
            {
                Id = o.Id,
                FullName = o.FullName,
                BirthDate = o.BirthDate.ToString("dd.MM.yyyy"),
                Type = o.GetOccupantType()
            }).ToList();

            _view.RoomOccupantsGrid.ItemsSource = rows;
        }

        // Create Settlement
        private void CreateSettlement()
        {
            try
            {
                var canCreate = _sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageEvictions")
                                || _sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageOccupants");
                if (!canCreate)
                {
                    MessageBox.Show("No permission to create settlement.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dormNum = _view.DormitoryComboBoxSettle.SelectedValue as int?;
                var roomNum = _view.RoomComboBoxSettle.SelectedValue as int?;
                var date = _view.SettlementDatePicker.SelectedDate ?? DateTime.Now;
                var docIdObj = _view.DocumentComboBox.SelectedValue;

                if (dormNum == null || roomNum == null)
                {
                    MessageBox.Show("Select dormitory and room.", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (docIdObj is not Guid docId)
                {
                    MessageBox.Show("Select document.", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var dorm = _sys.DormitoryRegistry.GetDormitoryByNumber(dormNum.Value);
                var room = dorm?.GetAllRooms()?.FirstOrDefault(r => r.Number == roomNum.Value);
                if (room == null)
                {
                    MessageBox.Show("Room not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var doc = _sys.DocumentRegistry.FindDocumentById(docId);
                if (doc == null)
                {
                    MessageBox.Show("Document not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var selected = _view.OccupantsSelectList.SelectedItems.Cast<OccupantRow>().ToList();
                if (!selected.Any())
                {
                    MessageBox.Show("Select at least one occupant.", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var occupants = selected
                    .Select(o => _sys.OccupantRegistry.FindOccupantById(o.Id))
                    .Where(o => o != null)
                    .ToList();

                var available = room.GetAvailablePlacesCount();
                if (available < occupants.Count)
                {
                    MessageBox.Show($"Not enough places: available {available}, selected {occupants.Count}.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Валидация документа
                var format = _sys.DocumentValidator.CheckFormat(doc);
                if (!format.IsValid || !_sys.DocumentValidator.CheckValidity(doc, DateTime.Now))
                {
                    MessageBox.Show("Document is invalid for settlement.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Создаём заселение
                var settlement = new CouseWork3Semester.Models.Settlement();
                settlement.InitializeSettlement(occupants, room, doc, date);

                // Привязываем документ к жильцам (опционально)
                foreach (var occ in occupants)
                {
                    _sys.DocumentOccupantService.AttachDocumentToOccupant(doc, occ);
                }

                // Добавляем в сервис
                _sys.SettlementEvictionService.AddSettlement(settlement); // Если метод у вас называется иначе — используйте ваш.

                _view.SettlementDatePicker.SelectedDate = null;
                _view.OccupantsSelectList.UnselectAll();

                LoadSettlements();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create settlement: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Settlement actions
        private void PerformSelectedSettlement()
        {
            try
            {
                if (!_sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageEvictions"))
                {
                    MessageBox.Show("No permission.", "Access denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.SettlementsGrid.SelectedItem is not SettlementRow row)
                {
                    MessageBox.Show("Select a settlement.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var settlement = _sys.SettlementEvictionService.FindSettlementById(row.Id);
                if (settlement == null)
                {
                    MessageBox.Show("Settlement not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                settlement.PerformSettlement();
                LoadSettlements();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to perform settlement: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CompleteSelectedSettlement()
        {
            try
            {
                if (!_sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageEvictions"))
                {
                    MessageBox.Show("No permission.", "Access denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.SettlementsGrid.SelectedItem is not SettlementRow row)
                {
                    MessageBox.Show("Select a settlement.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var settlement = _sys.SettlementEvictionService.FindSettlementById(row.Id);
                if (settlement == null)
                {
                    MessageBox.Show("Settlement not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                settlement.CompleteSettlement();
                LoadSettlements();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to complete settlement: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelSelectedSettlement()
        {
            try
            {
                if (!_sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageEvictions"))
                {
                    MessageBox.Show("No permission.", "Access denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.SettlementsGrid.SelectedItem is not SettlementRow row)
                {
                    MessageBox.Show("Select a settlement.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var settlement = _sys.SettlementEvictionService.FindSettlementById(row.Id);
                if (settlement == null)
                {
                    MessageBox.Show("Settlement not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                settlement.CancelSettlement("Cancelled via UI");
                LoadSettlements();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to cancel settlement: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Create/execute/remove eviction — без изменений от предыдущей версии
        private void CreateEviction()
        {
            try
            {
                if (!_sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageEvictions"))
                {
                    MessageBox.Show("No permission.", "Access denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dormNum = _view.DormitoryComboBox.SelectedValue as int?;
                var roomNum = _view.RoomComboBox.SelectedValue as int?;
                var date = _view.EvictionDatePicker.SelectedDate ?? DateTime.Now;
                var reason = _view.EvictionReasonTextBox.Text?.Trim();

                if (dormNum == null || roomNum == null)
                {
                    MessageBox.Show("Select dormitory and room.", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrWhiteSpace(reason))
                {
                    MessageBox.Show("Reason is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var dorm = _sys.DormitoryRegistry.GetDormitoryByNumber(dormNum.Value);
                var room = dorm?.GetAllRooms()?.FirstOrDefault(r => r.Number == roomNum.Value);
                if (room == null)
                {
                    MessageBox.Show("Room not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var selectedOccupants = _view.RoomOccupantsGrid.SelectedItems.Cast<OccupantRow>().ToList();
                if (!selectedOccupants.Any())
                {
                    MessageBox.Show("Select at least one occupant.", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var occupants = selectedOccupants
                    .Select(o => _sys.OccupantRegistry.FindOccupantById(o.Id))
                    .Where(o => o != null)
                    .ToList();

                var eviction = new CouseWork3Semester.Models.Eviction();
                eviction.InitializeEviction(Guid.NewGuid(), occupants, room, reason, relatedSettlement: null);

                _sys.SettlementEvictionService.AddEviction(eviction);

                _view.EvictionDatePicker.SelectedDate = null;
                _view.EvictionReasonTextBox.Text = "";
                LoadEvictions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create eviction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSelectedEviction()
        {
            try
            {
                if (!_sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageEvictions"))
                {
                    MessageBox.Show("No permission.", "Access denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.EvictionsGrid.SelectedItem is not EvictionRow row)
                {
                    MessageBox.Show("Select an eviction.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var eviction = _sys.SettlementEvictionService.FindEvictionById(row.Id);
                if (eviction == null)
                {
                    MessageBox.Show("Eviction not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var ok = eviction.PerformEviction();
                if (!ok)
                {
                    MessageBox.Show("Eviction execution failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoadEvictions();
                LoadRoomOccupantsForEviction();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to execute eviction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveSelectedEviction()
        {
            try
            {
                if (!_sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageEvictions"))
                {
                    MessageBox.Show("No permission.", "Access denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.EvictionsGrid.SelectedItem is not EvictionRow row)
                {
                    MessageBox.Show("Select an eviction.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var ok = _sys.SettlementEvictionService.RemoveEviction(row.Id);
                if (!ok)
                {
                    MessageBox.Show("Unable to remove eviction.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoadEvictions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove eviction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadEvictions()
        {
            // Получаем список ожидающих/в процессе выселений
            var evictions = _sys.SettlementEvictionService.GetPendingEvictions() ?? new List<IEviction>();

            var rows = evictions.Select(e => new EvictionRow
            {
                Id = e.Id,
                EvictionDate = e.EvictionDate.ToString("dd.MM.yyyy"),
                RoomNumber = e.Room?.Number ?? 0,
                OccupantsCount = e.Occupants?.Count ?? 0,
                Reason = e.Reason,
                Status = e is CouseWork3Semester.Models.Eviction ev ? ev.Status.ToString() : "Pending"
            })
            .OrderBy(r => r.EvictionDate)
            .ToList();

            _view.EvictionsGrid.ItemsSource = rows;
        }

        // View row models
        private class DormItem { public int Number { get; set; } public string Display { get; set; } }
        private class RoomItem { public int Number { get; set; } public string Display { get; set; } }
        private class DocumentItem { public Guid Id { get; set; } public string Display { get; set; } }

        private class OccupantRow
        {
            public Guid Id { get; set; }
            public string FullName { get; set; }
            public string BirthDate { get; set; }
            public string Type { get; set; }
        }

        private class SettlementRow
        {
            public Guid Id { get; set; }
            public string SettlementDate { get; set; }
            public int RoomNumber { get; set; }
            public int OccupantsCount { get; set; }
            public string DocumentTitle { get; set; }
            public string Status { get; set; }
        }

        private class EvictionRow
        {
            public Guid Id { get; set; }
            public string EvictionDate { get; set; }
            public int RoomNumber { get; set; }
            public int OccupantsCount { get; set; }
            public string Reason { get; set; }
            public string Status { get; set; }
        }
    }
}