using System.Windows;  // for Visibility
using System.Windows.Controls;  // for System.Windows.Controls.TextBox
using Monke2.ViewModels.Pages;
using Wpf.Ui.Controls;  // for other controls like those from WPF UI library

namespace Monke2.Views.Pages
{
	public partial class SettingsPage : INavigableView<SettingsViewModel>
	{
		public SettingsViewModel ViewModel { get; }

		public SettingsPage(SettingsViewModel viewModel)
		{
			InitializeComponent();
			ViewModel = viewModel;
			DataContext = ViewModel;  // Ensure DataContext is set to SettingsViewModel
		}

		// Event handler for GotFocus
		private void UserInputTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			if (sender is System.Windows.Controls.TextBox textBox && textBox.Text == "Enter your encryption key here...")
			{
				textBox.Text = string.Empty; // Clear placeholder text when focused
				textBox.Foreground = System.Windows.Media.Brushes.Black; // Optional: Set the color to black when user types
			}
		}

		// Event handler for LostFocus
		private void UserInputTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (sender is System.Windows.Controls.TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
			{
				textBox.Text = "Enter your input here..."; // Set placeholder text when the text box is empty
				textBox.Foreground = System.Windows.Media.Brushes.Gray; // Optional: Set the color to gray to mimic placeholder text
			}
		}
	}
}
