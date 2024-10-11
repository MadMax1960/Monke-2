using Monke2.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Monke2.Views.Pages
{
	public partial class DashboardPage : INavigableView<DashboardViewModel>
	{
		public DashboardViewModel ViewModel { get; }

		public DashboardPage(DashboardViewModel viewModel, SettingsViewModel settingsViewModel)
		{
			InitializeComponent();
			ViewModel = new DashboardViewModel(settingsViewModel); // Pass the shared SettingsViewModel instance
			DataContext = ViewModel;
		}
	}
}
