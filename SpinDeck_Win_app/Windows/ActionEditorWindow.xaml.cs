using Microsoft.Win32;
using SpinDeck_Win_app.Models;
using System.Windows;
using System.Windows.Controls;

namespace SpinDeck_Win_app.Windows
{
    public partial class ActionEditorWindow : Window
    {
        public ActionConfiguration? Action { get; private set; }

        private readonly bool _editMode;


        // =====================================================
        // ADD
        // =====================================================

        public ActionEditorWindow()
        {
            InitializeComponent();

            _editMode = false;

            TypeComboBox.SelectedIndex = 0;

            UpdateTypePanel();
        }


        // =====================================================
        // EDIT
        // =====================================================

        public ActionEditorWindow(
            ActionConfiguration action)
        {
            InitializeComponent();

            _editMode = true;

            Action = action;

            TitleText.Text = "Edit action";
            Title = "Edit action";


            NameTextBox.Text =
                action.Name;


            if (action.Type == ActionType.Browser)
            {
                TypeComboBox.SelectedIndex = 0;

                BrowserTextBox.Text =
                    action.Value;
            }
            else
            {
                TypeComboBox.SelectedIndex = 1;

                ApplicationTextBox.Text =
                    action.Value;
            }


            UpdateTypePanel();
        }


        // =====================================================
        // TYPE CHANGE
        // =====================================================

        private void TypeComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            UpdateTypePanel();
        }


        private void UpdateTypePanel()
        {
            if (BrowserPanel == null ||
                ApplicationPanel == null ||
                TypeComboBox == null)
            {
                return;
            }


            if (TypeComboBox.SelectedIndex == 0)
            {
                BrowserPanel.Visibility =
                    Visibility.Visible;

                ApplicationPanel.Visibility =
                    Visibility.Collapsed;
            }
            else
            {
                BrowserPanel.Visibility =
                    Visibility.Collapsed;

                ApplicationPanel.Visibility =
                    Visibility.Visible;
            }
        }


        // =====================================================
        // BROWSE APPLICATION
        // =====================================================

        private void BrowseApplicationButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var dialog =
                new OpenFileDialog
                {
                    Title = "Choose an application",
                    Filter =
                        "Pliki wykonywalne (*.exe)|*.exe"
                };


            if (dialog.ShowDialog() == true)
            {
                ApplicationTextBox.Text =
                    dialog.FileName;
            }
        }


        // =====================================================
        // SAVE
        // =====================================================

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string name =
                NameTextBox.Text.Trim();


            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show(
                        "Enter an action name.",
                        "Name required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                NameTextBox.Focus();

                return;
            }


            ActionType type;


            if (TypeComboBox.SelectedIndex == 0)
            {
                type =
                    ActionType.Browser;
            }
            else
            {
                type =
                    ActionType.Application;
            }


            string value;


            if (type == ActionType.Browser)
            {
                value =
                    BrowserTextBox.Text.Trim();


                if (string.IsNullOrWhiteSpace(value))
                {
                    MessageBox.Show(
                        "Enter a web address.",
                        "Address required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );

                    BrowserTextBox.Focus();

                    return;
                }


                if (!Uri.TryCreate(
                    value,
                    UriKind.Absolute,
                    out Uri? uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp &&
                     uri.Scheme != Uri.UriSchemeHttps))
                {
                    MessageBox.Show(
                        "Enter a valid URL.\n\n" +
                        "Example:\n" +
                        "https://google.com",
                        "Invalid address",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );

                    BrowserTextBox.Focus();

                    return;
                }
            }
            else
            {
                value =
                    ApplicationTextBox.Text.Trim();


                if (string.IsNullOrWhiteSpace(value))
                {
                    MessageBox.Show(
                        "Choose an application.",
                        "Application required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );

                    return;
                }
            }


            // =================================================
            // CREATE / UPDATE
            // =================================================

            if (_editMode)
            {
                Action!.Name = name;

                Action.Type = type;

                Action.Value = value;
            }
            else
            {
                Action =
                    new ActionConfiguration
                    {
                        Name = name,
                        Type = type,
                        Value = value
                    };
            }


            DialogResult = true;

            Close();
        }


        // =====================================================
        // CANCEL
        // =====================================================

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }
    }
}