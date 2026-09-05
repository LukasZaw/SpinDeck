using SpinDeck_Win_app.Models;
using SpinDeck_Win_app.Services;
using SpinDeck_Win_app.Windows;
using System.Windows;
using System.Windows.Controls;

namespace SpinDeck_Win_app.Views
{
    public partial class ActionsView : UserControl
    {
        private readonly ActionManager _actionManager;


        public ActionsView(
            ActionManager actionManager)
        {
            InitializeComponent();

            _actionManager =
                actionManager;


            ActionsItemsControl.ItemsSource =
                _actionManager.Actions;
        }


        // =====================================================
        // ADD
        // =====================================================

        private void AddActionButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var window =
                new ActionEditorWindow
                {
                    Owner = Window.GetWindow(this)
                };


            if (window.ShowDialog() == true &&
                window.Action != null)
            {
                _actionManager.Add(
                    window.Action
                );
            }
        }


        // =====================================================
        // EDIT
        // =====================================================

        private void EditActionButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is ActionConfiguration action)
            {
                var window =
                    new ActionEditorWindow(action)
                    {
                        Owner = Window.GetWindow(this)
                    };


                if (window.ShowDialog() == true)
                {
                    ActionsItemsControl.Items.Refresh();

                    _actionManager.Edit(
                        action
                    );
                }
            }
        }


        // =====================================================
        // DELETE
        // =====================================================

        private void DeleteActionButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is ActionConfiguration action)
            {
                var result =
                    MessageBox.Show(
                        $"Are you sure you want to delete action - \"{action.Name}\"?",
                        "Delete action",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );


                if (result ==
                    MessageBoxResult.Yes)
                {
                    _actionManager.Remove(
                        action
                    );
                }
            }
        }
    }
}