using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text.Classification;

namespace Vim.VisualStudio.RecentFiles
{
    // DialogWindow (not plain Window): Visual Studio's shell routes keys like Enter/Delete/
    // arrows to whatever it considers the "active view" independent of raw Win32 focus. A plain
    // WPF Window never registers with that system, so those keys kept going to the editor behind
    // this popup even though it visibly had focus. DialogWindow is VS's own base class for
    // exactly this - it integrates with the shell's keyboard/modal state correctly.
    public partial class RecentFilesWindow : DialogWindow
    {
        private readonly IReadOnlyList<RecentFileEntry> _allEntries;
        private readonly ObservableCollection<RecentFileEntry> _filteredEntries = new ObservableCollection<RecentFileEntry>();
        private bool _isClosing;

        internal string ChosenFilePath { get; private set; }

        internal RecentFilesWindow(IEditorFormatMap editorFormatMap, IReadOnlyList<RecentFileEntry> entries)
        {
            InitializeComponent();

            _allEntries = entries;
            ResultsList.ItemsSource = _filteredEntries;

            ApplyTheme(editorFormatMap);
            ApplyFilter(string.Empty);

            Loaded += delegate { SearchBox.Focus(); };
        }

        private void ApplyTheme(IEditorFormatMap editorFormatMap)
        {
            var background = editorFormatMap.GetBackgroundBrush(
                RecentFilesFormatDefinitionNames.Chrome,
                RecentFilesChromeFormatDefinition.DefaultBackgroundColor);
            var foreground = editorFormatMap.GetForegroundBrush(
                RecentFilesFormatDefinitionNames.Chrome,
                RecentFilesChromeFormatDefinition.DefaultForegroundColor);
            var selection = editorFormatMap.GetBackgroundBrush(
                RecentFilesFormatDefinitionNames.Selection,
                RecentFilesSelectionFormatDefinition.DefaultBackgroundColor);
            var searchBoxBackground = editorFormatMap.GetBackgroundBrush(
                RecentFilesFormatDefinitionNames.SearchBox,
                RecentFilesSearchBoxFormatDefinition.DefaultBackgroundColor);
            var searchBoxBorder = editorFormatMap.GetForegroundBrush(
                RecentFilesFormatDefinitionNames.SearchBox,
                RecentFilesSearchBoxFormatDefinition.DefaultBorderColor);

            ChromeBorder.Background = background;
            ChromeBorder.BorderBrush = foreground;
            SearchBoxBorder.Background = searchBoxBackground;
            SearchBoxBorder.BorderBrush = searchBoxBorder;
            SearchBox.Foreground = foreground;
            SearchBox.CaretBrush = foreground;
            ResultsList.Foreground = foreground;
            EmptyHint.Foreground = foreground;
            Resources["SelectionBrush"] = selection;
        }

        private void ApplyFilter(string query)
        {
            var selectedPath = (ResultsList.SelectedItem as RecentFileEntry)?.FilePath;

            IEnumerable<RecentFileEntry> matches;
            if (string.IsNullOrWhiteSpace(query))
            {
                matches = _allEntries;
            }
            else
            {
                matches = _allEntries
                    .Select(entry => (entry, score: FuzzyMatcher.Score(entry.FilePath, query)))
                    .Where(x => x.score >= 0)
                    .OrderByDescending(x => x.score)
                    .Select(x => x.entry);
            }

            _filteredEntries.Clear();
            foreach (var entry in matches)
            {
                _filteredEntries.Add(entry);
            }

            EmptyHint.Visibility = _filteredEntries.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            if (_filteredEntries.Count == 0)
            {
                return;
            }

            var restoredIndex = selectedPath == null
                ? -1
                : _filteredEntries.ToList().FindIndex(entry => entry.FilePath == selectedPath);
            ResultsList.SelectedIndex = restoredIndex >= 0 ? restoredIndex : 0;
        }

        private void MoveSelection(int delta)
        {
            if (_filteredEntries.Count == 0)
            {
                return;
            }

            var next = ResultsList.SelectedIndex + delta;
            next = Math.Max(0, Math.Min(_filteredEntries.Count - 1, next));
            ResultsList.SelectedIndex = next;
            ResultsList.ScrollIntoView(ResultsList.SelectedItem);
        }

        private void ChooseSelected()
        {
            if (ResultsList.SelectedItem is RecentFileEntry entry)
            {
                ChosenFilePath = entry.FilePath;
                SafeClose();
            }
        }

        /// <summary>
        /// Close() triggers deactivation as part of its own shutdown, which re-enters
        /// OnDeactivated below; calling Close() again from there while the first call is still
        /// unwinding throws (WPF disallows closing a window that is already closing). This
        /// guards against that reentrancy regardless of which path triggered the close first.
        /// </summary>
        private void SafeClose()
        {
            if (_isClosing)
            {
                return;
            }

            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _isClosing = true;
            base.OnClosing(e);
        }

        private void OnSearchTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyFilter(SearchBox.Text);
        }

        private void OnResultsListMouseUp(object sender, MouseButtonEventArgs e)
        {
            ChooseSelected();
        }

        private void OnDeactivated(object sender, EventArgs e)
        {
            SafeClose();
        }

        private void DeleteBackward()
        {
            if (SearchBox.SelectionLength > 0)
            {
                SearchBox.SelectedText = string.Empty;
            }
            else if (SearchBox.CaretIndex > 0)
            {
                var index = SearchBox.CaretIndex;
                SearchBox.Text = SearchBox.Text.Remove(index - 1, 1);
                SearchBox.CaretIndex = index - 1;
            }
        }

        private void DeleteForward()
        {
            if (SearchBox.SelectionLength > 0)
            {
                SearchBox.SelectedText = string.Empty;
            }
            else if (SearchBox.CaretIndex < SearchBox.Text.Length)
            {
                var index = SearchBox.CaretIndex;
                SearchBox.Text = SearchBox.Text.Remove(index, 1);
                SearchBox.CaretIndex = index;
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    SafeClose();
                    e.Handled = true;
                    break;
                case Key.Back:
                    DeleteBackward();
                    e.Handled = true;
                    break;
                case Key.Delete:
                    DeleteForward();
                    e.Handled = true;
                    break;
                case Key.Down:
                    MoveSelection(1);
                    e.Handled = true;
                    break;
                case Key.Up:
                    MoveSelection(-1);
                    e.Handled = true;
                    break;
                case Key.Enter:
                    ChooseSelected();
                    e.Handled = true;
                    break;
            }
        }
    }
}
