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
    public class InventoryPresenter
    {
        private readonly InventoryView _view;
        private readonly IAccountingSystem _sys;
        private IEmployee _employee => _sys.GetCurrentEmployee();

        public InventoryPresenter(InventoryView view, IAccountingSystem accountingSystem)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _sys = accountingSystem ?? throw new ArgumentNullException(nameof(accountingSystem));

            WireEvents();
            ApplyPermissions();
            LoadDormitories();
        }

        private void WireEvents()
        {
            _view.RefreshButton.Click += (s, e) => LoadInventory();
            _view.DormitoryCombo.SelectionChanged += (s, e) => LoadRooms();
            _view.RoomCombo.SelectionChanged += (s, e) => LoadInventory();

            _view.AddItemButton.Click += (s, e) => AddItem();
            _view.UpdateItemButton.Click += (s, e) => UpdateSelectedItem();
            _view.RemoveItemButton.Click += (s, e) => RemoveSelectedItem();
        }

        private void ApplyPermissions()
        {
            bool canManage = false;

            if (_sys.PermissionManager != null && _employee != null)
            {
                canManage = _sys.PermissionManager.CanRolePerformAction(_employee.Role, "ManageInventory");
            }

            if (!canManage && _employee != null)
            {
                var roleName = _employee.Role;
                if (roleName is UserRole.Administrator || roleName is UserRole.Commandant)
                    canManage = true;
            }

            _view.AddItemButton.IsEnabled = canManage;
            _view.UpdateItemButton.IsEnabled = canManage;
            _view.RemoveItemButton.IsEnabled = canManage;
            _view.ManageHintText.Text = canManage
                ? "You have permission to manage inventory."
                : "View-only: add/update/remove are disabled for your role.";
        }

        private void LoadDormitories()
        {
            try
            {
                var dorms = _sys.DormitoryRegistry.GetAllDormitories()
                    .OrderBy(d => d.Number)
                    .Select(d => new DormItem { Number = d.Number, Display = $"{d.Number} — {d.Address}" })
                    .ToList();

                _view.DormitoryCombo.ItemsSource = dorms;
                if (dorms.Any())
                {
                    _view.DormitoryCombo.SelectedIndex = 0;
                    LoadRooms();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load dormitories: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRooms()
        {
            try
            {
                var dormNum = _view.DormitoryCombo.SelectedValue as int?;
                var dorm = dormNum.HasValue ? _sys.DormitoryRegistry.GetDormitoryByNumber(dormNum.Value) : null;
                var rooms = (dorm?.GetAllRooms() ?? new List<IRoom>())
                    .OrderBy(r => r.Number)
                    .Select(r => new RoomItem { Number = r.Number, Display = $"Room №{r.Number} (floor {r.Floor}, places {r.Type})" })
                    .ToList();

                _view.RoomCombo.ItemsSource = rooms;
                if (rooms.Any())
                {
                    _view.RoomCombo.SelectedIndex = 0;
                    LoadInventory();
                }
                else
                {
                    _view.InventoryGrid.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load rooms: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadInventory()
        {
            try
            {
                var dormNum = _view.DormitoryCombo.SelectedValue as int?;
                var roomNum = _view.RoomCombo.SelectedValue as int?;
                if (dormNum == null || roomNum == null)
                {
                    _view.InventoryGrid.ItemsSource = null;
                    return;
                }

                var items = _sys.InventoryRegistry.GetItemsForRoom(dormNum.Value, roomNum.Value)
                    .Select(i => new ItemRow
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Quantity = i.Quantity
                    })
                    .OrderBy(x => x.Name)
                    .ToList();

                _view.InventoryGrid.ItemsSource = items;
                _view.StatusText.Text = $"Loaded {items.Count} item(s).";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load inventory: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddItem()
        {
            try
            {
                var dormNum = _view.DormitoryCombo.SelectedValue as int?;
                var roomNum = _view.RoomCombo.SelectedValue as int?;
                var name = _view.ItemNameTextBox.Text?.Trim() ?? "";
                var qtyText = _view.ItemQuantityTextBox.Text?.Trim() ?? "0";

                if (dormNum == null || roomNum == null)
                {
                    MessageBox.Show("Select dormitory and room.", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Enter item name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (!int.TryParse(qtyText, out var qty) || qty < 0)
                {
                    MessageBox.Show("Quantity must be a non-negative integer.", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var item = new InventoryItem(name, qty, dormNum.Value, roomNum.Value);
                _sys.InventoryRegistry.AddItem(item);

                _view.ItemNameTextBox.Text = "";
                _view.ItemQuantityTextBox.Text = "";
                LoadInventory();
                _view.StatusText.Text = "Item added.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add item: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSelectedItem()
        {
            try
            {
                if (_view.InventoryGrid.SelectedItem is not ItemRow row)
                {
                    MessageBox.Show("Select an item in the list.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var name = _view.ItemNameTextBox.Text?.Trim() ?? row.Name;
                var qtyText = _view.ItemQuantityTextBox.Text?.Trim();
                var qty = row.Quantity;

                if (!string.IsNullOrWhiteSpace(qtyText))
                {
                    if (!int.TryParse(qtyText, out qty) || qty < 0)
                    {
                        MessageBox.Show("Quantity must be a non-negative integer.", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                var ok = _sys.InventoryRegistry.UpdateItem(row.Id, name, qty);
                if (!ok)
                {
                    MessageBox.Show("Failed to update item.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoadInventory();
                _view.StatusText.Text = "Item updated.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update item: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveSelectedItem()
        {
            try
            {
                if (_view.InventoryGrid.SelectedItem is not ItemRow row)
                {
                    MessageBox.Show("Select an item in the list.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var ok = _sys.InventoryRegistry.RemoveItem(row.Id);
                if (!ok)
                {
                    MessageBox.Show("Failed to remove item.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoadInventory();
                _view.StatusText.Text = "Item removed.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove item: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private class DormItem
        {
            public int Number { get; set; }
            public string Display { get; set; }
        }

        private class RoomItem
        {
            public int Number { get; set; }
            public string Display { get; set; }
        }

        private class ItemRow
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
        }
    }
}