using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CouseWork3Semester.Enums;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Views;

namespace CouseWork3Semester.Presenters
{
    public class ReportsSearchPresenter
    {
        private readonly ReportsSearchView _view;
        private readonly IAccountingSystem _sys;
        private IEmployee _employee => _sys.GetCurrentEmployee();

        public ReportsSearchPresenter(ReportsSearchView view, IAccountingSystem accountingSystem)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _sys = accountingSystem ?? throw new ArgumentNullException(nameof(accountingSystem));

            WireEvents();
            ApplyPermissions();
            LoadDormitories();
            LoadSearchResults();
        }

        private void WireEvents()
        {
            _view.ApplyFiltersButton.Click += (s, e) => LoadSearchResults();
            _view.DormitoryCombo.SelectionChanged += (s, e) => LoadAvailableRooms();
            _view.ReportFreeRoomsButton.Click += (s, e) => GenerateFreeRoomsReport();
            _view.ReportResidentsInDormButton.Click += (s, e) => GenerateResidentsInDormReport();
        }

        private void ApplyPermissions()
        {
            bool canGenerate = false;
            bool viewOnly = true;

            if (_sys.PermissionManager != null && _employee != null)
            {
                canGenerate = _sys.PermissionManager.CanRolePerformAction(_employee.Role, "GenerateReports");
            }

            var roleName = _employee.Role;
            if (roleName is UserRole.Administrator)
            {
                canGenerate = true;
                viewOnly = false;
            }
            else if (roleName is UserRole.Commandant)
            {
                canGenerate = true;
                viewOnly = true;
            }
            else
            {
                canGenerate = true;
                viewOnly = true;
            }

            _view.ReportFreeRoomsButton.IsEnabled = canGenerate;
            _view.ReportResidentsInDormButton.IsEnabled = canGenerate;

            _view.HintText.Text = viewOnly
                ? "View-only: поиск и генерация отчётов доступны, изменение данных недоступно."
                : "Full access: доступна генерация отчётов.";
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
                    LoadAvailableRooms();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load dormitories: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAvailableRooms()
        {
            try
            {
                var dormNum = _view.DormitoryCombo.SelectedValue as int?;
                var dorm = dormNum.HasValue ? _sys.DormitoryRegistry.GetDormitoryByNumber(dormNum.Value) : null;

                var rooms = (dorm?.GetAllRooms() ?? new List<IRoom>())
                    .Where(r => r.GetAvailablePlacesCount() > 0)
                    .OrderBy(r => r.Number)
                    .Select(r => new AvailableRoomRow
                    {
                        Number = r.Number,
                        Floor = r.Floor,
                        FreePlaces = r.GetAvailablePlacesCount(),
                        Capacity = r.Type
                    })
                    .ToList();

                _view.AvailableRoomsGrid.ItemsSource = rooms;
                _view.StatusText.Text = $"Loaded {rooms.Count} free room(s).";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load available rooms: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSearchResults()
        {
            try
            {
                var occQuery = _view.OccupantSearchTextBox.Text?.Trim() ?? "";
                var docNumber = _view.DocumentNumberTextBox.Text?.Trim() ?? "";

                var occs = string.IsNullOrWhiteSpace(occQuery)
                    ? _sys.OccupantRegistry.GetAllOccupants()
                    : _sys.SearchService.FindOccupant(occQuery);

                var occItems = occs.Select(o => new OccupantRow
                {
                    FullName = o.FullName,
                    Age = o.GetAge(),
                    Type = o.GetOccupantType()
                }).OrderBy(x => x.FullName).ToList();
                _view.OccupantsGrid.ItemsSource = occItems;

                var docs = _sys.DocumentRegistry.GetAllDocuments();
                if (!string.IsNullOrWhiteSpace(docNumber))
                {
                    var single = _sys.DocumentRegistry.FindDocumentByNumber(docNumber);
                    docs = (single != null) ? new List<IDocument> { single } : new List<IDocument>();
                }

                var docItems = docs.Select(d => new DocumentRow
                {
                    Title = d.Title,
                    Series = d.Series,
                    Number = d.Number,
                    IssuedBy = d.IssuedBy,
                    IssueDate = d.IssueDate.ToString("dd.MM.yyyy")
                }).OrderBy(x => x.Title).ToList();
                _view.DocumentsGrid.ItemsSource = docItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to apply filters: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateFreeRoomsReport()
        {
            try
            {
                var dormNum = _view.DormitoryCombo.SelectedValue as int?;
                IDormitory dorm = dormNum.HasValue ? _sys.DormitoryRegistry.GetDormitoryByNumber(dormNum.Value) : null;

                var report = _sys.ReportService.GenerateFreeRoomsReport(dorm);
                _view.ReportTextBox.Text = report;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to generate report: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateResidentsInDormReport()
        {
            try
            {
                var dormNum = _view.DormitoryCombo.SelectedValue as int?;
                if (dormNum == null)
                {
                    MessageBox.Show("Select a dormitory first.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var report = _sys.ReportService.GenerateDormResidentsReport(dormNum.Value);
                _view.ReportTextBox.Text = report;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to generate report: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private class DormItem
        {
            public int Number { get; set; }
            public string Display { get; set; }
        }

        private class AvailableRoomRow
        {
            public int Number { get; set; }
            public int Floor { get; set; }
            public int FreePlaces { get; set; }
            public int Capacity { get; set; }
        }

        private class OccupantRow
        {
            public string FullName { get; set; }
            public int Age { get; set; }
            public string Type { get; set; }
        }

        private class DocumentRow
        {
            public string Title { get; set; }
            public string Series { get; set; }
            public string Number { get; set; }
            public string IssuedBy { get; set; }
            public string IssueDate { get; set; }
        }
    }
}