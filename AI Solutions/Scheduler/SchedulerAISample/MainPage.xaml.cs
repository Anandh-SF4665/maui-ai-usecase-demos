
using SchedulerAISample.ViewModel;

namespace SchedulerAISample
{
	public partial class MainPage : ContentPage
	{

		public MainPage()
		{
			InitializeComponent();
			BindingContext =new SchedulerViewModel();
		}
	}
}
