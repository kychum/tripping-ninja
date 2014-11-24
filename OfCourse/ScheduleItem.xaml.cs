using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OfCourse
{
	/// <summary>
	///     Interaction logic for ScheduleItem.xaml
	/// </summary>
	public partial class ScheduleItem
	{
		#region Constructors and Destructors

		public ScheduleItem()
		{
			this.InitializeComponent();
		}

		#endregion

		public int Column { get; set; }

		public int Id { get; set; }

		public int Row { get; set; }

		public int SpanCount { get; set; }

		#region Methods and Operators

		private void Root_OnMouseEnter(object sender, MouseEventArgs e)
		{
			var window = ((MainWindow) Application.Current.MainWindow);
			SearchResult result = window.GetSearchResultById(this.Id);
			var hoverData = (SearchResult) window.FindName("HoverData");
			var scheduleItem = (ScheduleItem) sender;

			if (hoverData == null)
			{
				return;
			}

			var point = new Point(
				scheduleItem.ActualWidth/2 - hoverData.ActualWidth/2,
				-hoverData.ActualHeight);

			GeneralTransform gt = scheduleItem.TransformToVisual(window);
			point = gt.Transform(point);

			hoverData.Id = result.Id;
			hoverData.CourseNumber = result.CourseNumber;
			hoverData.CourseName = result.CourseName;
			hoverData.Description = result.Description;
			hoverData.Professor = result.Professor;
			hoverData.CourseType = result.CourseType;
			hoverData.DaysOfWeek = result.DaysOfWeek;
			hoverData.StartTime = result.StartTime;
			hoverData.Duration = result.Duration;
			hoverData.PreReqs = result.PreReqs;
			hoverData.AntiReqs = result.AntiReqs;
			hoverData.Status = result.Status;
			hoverData.Faculty = result.Faculty;
			hoverData.Department = result.Department;
			hoverData.SetLabels();

			hoverData.Visibility = Visibility.Visible;
			((Button) hoverData.FindName("DetailsButton")).Visibility = Visibility.Hidden;

			hoverData.Margin = new Thickness(point.X, point.Y, 0, 0);
		}

		private void Root_OnMouseLeave(object sender, MouseEventArgs e)
		{
			var window = ((MainWindow) Application.Current.MainWindow);
			var hoverData = (SearchResult) window.FindName("HoverData");

			if (hoverData == null)
			{
				return;
			}

			hoverData.Visibility = Visibility.Hidden;
		}

		#endregion
	}
}