using Monke2.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Monke2.Views.Pages
{
	public partial class DashboardPage : INavigableView<DashboardViewModel>
	{
		public DashboardViewModel ViewModel { get; }

		public DashboardPage(DashboardViewModel viewModel)
		{
			InitializeComponent();  // This line is crucial

			ViewModel = viewModel;
			DataContext = ViewModel; // Assign ViewModel to DataContext
		}
	}
}
