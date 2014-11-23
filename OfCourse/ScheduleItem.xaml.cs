using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OfCourse
{
	/// <summary>
	/// Interaction logic for ScheduleItem.xaml
	/// </summary>
	public partial class ScheduleItem
	{
		public int id { get; set; }

		// Seems like too much of a bother to use dependency values
		public int row { get; set; }
		public int col { get; set; }
		public int span { get; set; }

		public ScheduleItem()
		{
			this.InitializeComponent();
		}

		private void Root_OnMouseEnter(object sender, MouseEventArgs e)
		{
			var window = ((MainWindow)Application.Current.MainWindow);
			var result = window.GetSearchResultById(id);
			var hoverData = (SearchResult)window.FindName("HoverData");
			var scheduleItem = (ScheduleItem)sender;

			if (hoverData == null)
			{
				return;
			}

			var point = new Point(
				scheduleItem.ActualWidth / 2 - hoverData.ActualWidth / 2,
				-hoverData.ActualHeight);
			GeneralTransform gt = scheduleItem.TransformToVisual(window);
			point = gt.Transform(point);

			hoverData.id = result.id;
			hoverData.courseNum = result.courseNum;
			hoverData.name = result.name;
			hoverData.desc = result.desc;
			hoverData.prof = result.prof;
			hoverData.type = result.type;
			hoverData.days = result.days;
			hoverData.startTime = result.startTime;
			hoverData.duration = result.duration;
			hoverData.prereqs = result.prereqs;
			hoverData.antireqs = result.antireqs;
			hoverData.status = result.status;
			hoverData.faculty = result.faculty;
			hoverData.department = result.department;
			hoverData.SetLabels();

			hoverData.Visibility = Visibility.Visible;
			((Button)hoverData.FindName("DetailsButton")).Visibility = Visibility.Hidden;

			hoverData.Margin = new Thickness(point.X, point.Y, 0, 0);
		}

		private void Root_OnMouseLeave(object sender, MouseEventArgs e)
		{
			var window = ((MainWindow)Application.Current.MainWindow);
			var hoverData = (SearchResult)window.FindName("HoverData");

			if (hoverData == null)
			{
				return;
			}

			hoverData.Visibility = Visibility.Hidden;
		}
	}
}