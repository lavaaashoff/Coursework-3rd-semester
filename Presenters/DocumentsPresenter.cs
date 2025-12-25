using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Views;

namespace CouseWork3Semester.Presenters
{
    public class DocumentsPresenter
    {
        private readonly DocumentsView _view;
        private readonly IAccountingSystem _sys;

        public DocumentsPresenter(DocumentsView view, IAccountingSystem accountingSystem)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _sys = accountingSystem ?? throw new ArgumentNullException(nameof(accountingSystem));

            WireEvents();
            ApplyPermissions();
            LoadDocuments();
            LoadOccupantsDropdown();
        }

        private void WireEvents()
        {
            _view.RefreshButton.Click += (s, e) => { LoadDocuments(); LoadAttachmentsForSelected(); };
            _view.SearchButton.Click += (s, e) => SearchDocument();
            _view.ClearSearchButton.Click += (s, e) => { _view.SearchNumberTextBox.Text = ""; LoadDocuments(); };

            _view.AddDocumentButton.Click += (s, e) => AddDocument();
            _view.UpdateCommentButton.Click += (s, e) => UpdateSelectedComment();
            _view.RemoveDocumentButton.Click += (s, e) => RemoveSelectedDocument();

            _view.DocumentsGrid.SelectionChanged += (s, e) => LoadAttachmentsForSelected();

            _view.AttachButton.Click += (s, e) => AttachSelected();
            _view.DetachButton.Click += (s, e) => DetachSelected();
        }

        private void ApplyPermissions()
        {
            var employee = _sys.GetCurrentEmployee();
            var canManage = _sys.PermissionManager.CanRolePerformAction(employee.Role, "ManageDocuments");

            _view.AddDocumentGroup.Visibility = canManage ? Visibility.Visible : Visibility.Collapsed;
            _view.EditDocumentGroup.Visibility = canManage ? Visibility.Visible : Visibility.Collapsed;
            _view.AttachGroup.Visibility = canManage ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadDocuments()
        {
            var docs = _sys.DocumentRegistry.GetAllDocuments() ?? new List<IDocument>();
            var items = docs.Select(d => new DocumentViewItem
            {
                Id = d.Id,
                Series = d.Series,
                Number = d.Number,
                Title = d.Title,
                IssueDateString = d.IssueDate.ToString("dd.MM.yyyy"),
                IssuedBy = d.IssuedBy,
                Comment = d.Comment ?? ""
            })
            .OrderBy(x => x.Title)
            .ToList();

            _view.DocumentsGrid.ItemsSource = items;

            ClearAddFields();
            _view.NewCommentTextBox.Text = "";
        }

        private void SearchDocument()
        {
            var number = _view.SearchNumberTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(number))
            {
                LoadDocuments();
                return;
            }

            var doc = _sys.DocumentRegistry.FindDocumentByNumber(number);
            if (doc == null)
            {
                MessageBox.Show("Document not found.", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var item = new DocumentViewItem
            {
                Id = doc.Id,
                Series = doc.Series,
                Number = doc.Number,
                Title = doc.Title,
                IssueDateString = doc.IssueDate.ToString("dd.MM.yyyy"),
                IssuedBy = doc.IssuedBy,
                Comment = doc.Comment ?? ""
            };
            _view.DocumentsGrid.ItemsSource = new List<DocumentViewItem> { item };
        }

        private void AddDocument()
        {
            try
            {
                var employee = _sys.GetCurrentEmployee();
                if (!_sys.PermissionManager.CanRolePerformAction(employee.Role, "ManageDocuments"))
                {
                    MessageBox.Show("You do not have permission to register documents.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var series = _view.SeriesTextBox.Text?.Trim() ?? "";
                var number = _view.NumberTextBox.Text?.Trim() ?? "";
                var title = _view.TitleTextBox.Text?.Trim() ?? "";
                var issuedBy = _view.IssuedByTextBox.Text?.Trim() ?? "";
                var issueDate = _view.IssueDatePicker.SelectedDate ?? DateTime.Now;
                var comment = _view.CommentTextBox.Text?.Trim();

                var doc = new CouseWork3Semester.Models.Document(series, number, title, issueDate, issuedBy, comment);

                // Валидация
                var format = _sys.DocumentValidator.CheckFormat(doc);
                if (!format.IsValid)
                {
                    var message = (format.Errors != null && format.Errors.Any())
                        ? string.Join("; ", format.Errors)
                        : "Document format is invalid";
                    MessageBox.Show(message, "Validation error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var unique = _sys.DocumentValidator.CheckUniqueness(doc, _sys.DocumentRegistry.GetAllDocuments());
                if (!unique)
                {
                    MessageBox.Show("Document with the same number already exists.", "Validation error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _sys.DocumentRegistry.RegisterDocument(doc);

                ClearAddFields();
                LoadDocuments();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to register document: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSelectedComment()
        {
            try
            {
                var employee = _sys.GetCurrentEmployee();
                if (!_sys.PermissionManager.CanRolePerformAction(employee.Role, "ManageDocuments"))
                {
                    MessageBox.Show("You do not have permission to edit documents.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.DocumentsGrid.SelectedItem is not DocumentViewItem item)
                {
                    MessageBox.Show("Select a document first.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var newComment = _view.NewCommentTextBox.Text?.Trim() ?? "";
                _sys.DocumentRegistry.FindDocumentById(item.Id).Comment = newComment;

                _view.NewCommentTextBox.Text = "";
                LoadDocuments();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update comment: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveSelectedDocument()
        {
            try
            {
                var employee = _sys.GetCurrentEmployee();
                if (!_sys.PermissionManager.CanRolePerformAction(employee.Role, "ManageDocuments"))
                {
                    MessageBox.Show("You do not have permission to remove documents.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.DocumentsGrid.SelectedItem is not DocumentViewItem item)
                {
                    MessageBox.Show("Select a document first.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var ok = _sys.DocumentRegistry.RemoveDocument(item.Id);
                if (!ok)
                {
                    MessageBox.Show("Unable to remove document.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoadDocuments();
                _view.AttachmentsGrid.ItemsSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove document: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAttachmentsForSelected()
        {
            if (_view.DocumentsGrid.SelectedItem is not DocumentViewItem item)
            {
                _view.AttachmentsGrid.ItemsSource = null;
                return;
            }

            var doc = _sys.DocumentRegistry.FindDocumentById(item.Id);
            if (doc == null)
            {
                _view.AttachmentsGrid.ItemsSource = null;
                return;
            }

            var occupants = _sys.DocumentOccupantService.GetOccupantsByDocument(doc) ?? new List<IRoomOccupant>();
            var rows = occupants.Select(o => new AttachmentViewItem
            {
                OccupantId = o.Id,
                FullName = o.FullName,
                Type = o.GetOccupantType(),
                BirthDateString = o.BirthDate.ToString("dd.MM.yyyy")
            }).ToList();

            _view.AttachmentsGrid.ItemsSource = rows;
        }

        private void LoadOccupantsDropdown()
        {
            var all = _sys.OccupantRegistry.GetAllOccupants() ?? new List<IRoomOccupant>();
            var items = all.Select(o => new OccupantDropdownItem
            {
                Id = o.Id,
                Display = $"{o.FullName} ({o.GetOccupantType()})"
            }).OrderBy(x => x.Display).ToList();

            _view.OccupantComboBox.ItemsSource = items;
            if (items.Any())
            {
                _view.OccupantComboBox.SelectedIndex = 0;
            }
        }

        private void AttachSelected()
        {
            try
            {
                var employee = _sys.GetCurrentEmployee();
                if (!_sys.PermissionManager.CanRolePerformAction(employee.Role, "ManageDocuments"))
                {
                    MessageBox.Show("You do not have permission to attach documents.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.DocumentsGrid.SelectedItem is not DocumentViewItem docItem)
                {
                    MessageBox.Show("Select a document first.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var doc = _sys.DocumentRegistry.FindDocumentById(docItem.Id);
                if (doc == null)
                {
                    MessageBox.Show("Document not found.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var occIdObj = _view.OccupantComboBox.SelectedValue;
                if (occIdObj is not Guid occId)
                {
                    MessageBox.Show("Select an occupant.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var occupant = _sys.OccupantRegistry.FindOccupantById(occId);
                if (occupant == null)
                {
                    MessageBox.Show("Occupant not found.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var ok = _sys.DocumentOccupantService.AttachDocumentToOccupant(doc, occupant);
                if (!ok)
                {
                    MessageBox.Show("Failed to attach document.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoadAttachmentsForSelected();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to attach: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DetachSelected()
        {
            try
            {
                var employee = _sys.GetCurrentEmployee();
                if (!_sys.PermissionManager.CanRolePerformAction(employee.Role, "ManageDocuments"))
                {
                    MessageBox.Show("You do not have permission to detach documents.", "Access denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_view.DocumentsGrid.SelectedItem is not DocumentViewItem docItem)
                {
                    MessageBox.Show("Select a document first.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (_view.AttachmentsGrid.SelectedItem is not AttachmentViewItem attItem)
                {
                    MessageBox.Show("Select an attached occupant row.", "Hint",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var ok = _sys.DocumentOccupantService.DetachDocumentFromOccupant(docItem.Id, attItem.OccupantId);
                if (!ok)
                {
                    MessageBox.Show("Failed to detach document.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoadAttachmentsForSelected();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to detach: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearAddFields()
        {
            _view.SeriesTextBox.Text = "";
            _view.NumberTextBox.Text = "";
            _view.TitleTextBox.Text = "";
            _view.IssueDatePicker.SelectedDate = null;
            _view.IssuedByTextBox.Text = "";
            _view.CommentTextBox.Text = "";
        }

        private class DocumentViewItem
        {
            public Guid Id { get; set; }
            public string Series { get; set; }
            public string Number { get; set; }
            public string Title { get; set; }
            public string IssueDateString { get; set; }
            public string IssuedBy { get; set; }
            public string Comment { get; set; }
        }

        private class AttachmentViewItem
        {
            public Guid OccupantId { get; set; }
            public string FullName { get; set; }
            public string Type { get; set; }
            public string BirthDateString { get; set; }
        }

        private class OccupantDropdownItem
        {
            public Guid Id { get; set; }
            public string Display { get; set; }
        }
    }
}