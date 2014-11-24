/* TODO: 
 *    - Add in advanced search options
 *    - Keyboard accessiblity?
 *    - Get mouse hover when disabled to highlight the course somehow
 *    - Integrate requisites to the program
 *    - (Would be nice) Get drag-and-drop to dynamically update [Alex]
 *    
 *  Comments from Richard:
 *    - What is the relationship between the cart and schedule?
 *        - How will the cart look?
 *    - Prereqs, coreqs, antireqs?
 *    - Use more space, a bigger window won't hurt
 *    - How much of a course registration system do we want this to become?
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace OfCourse
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		#region Constants

		private const string CourseFile = "xylophone.txt";
		private const string FallCoursesFile = "..\\..\\classes.txt";
		private const string WinterCoursesFile = "..\\..\\winter.txt";

		#endregion

		#region Fields

		private readonly List<Border> hoverBorders = new List<Border>();
		private readonly List<int> otherTermCart = new List<int>();
		private readonly List<int> otherTermSchedule = new List<int>();
		private readonly List<ScheduleItem> schedItems = new List<ScheduleItem>();
		private readonly DispatcherTimer statusToastTimer = new DispatcherTimer();
		private List<int> cartIds = new List<int>();
		private bool isWinter;

		private int[,] itemsInSlot = new int[12, 5];
		private List<SearchResult> results = new List<SearchResult>();
		private List<int> schedIds = new List<int>();

		#endregion

		#region Constructors and Destructors

		public MainWindow()
		{
			this.InitializeComponent();
			this.LoadClasses(FallCoursesFile);
			this.Expander.Toggle.Click += this.Toggle_Click;
			this.Schedule.Drop += this.SchedulePanel_Drop;
			this.Cart.Cart.Drop += this.Cart_Drop;
			this.Cart.CartIcon.Drop += this.Cart_Drop;
			this.Cart.Trash.Drop += this.Trash_Drop;
			this.ResultsPane.Drop += this.Trash_Drop;

			this.statusToastTimer.Tick += (timerSender, timerEventArgs) =>
			{
				var statusPanel = (WrapPanel)this.FindName("ButtonStatus");

				if (statusPanel != null)
				{
					statusPanel.Visibility = Visibility.Hidden;
				}

				this.statusToastTimer.Stop();
			};
			this.statusToastTimer.Interval = new TimeSpan(0, 0, 3);

			int classesLoaded = 0;
			try
			{
				classesLoaded = this.LoadScheduleFromFile(CourseFile);
			}
			catch (IOException)
			{
				// No presaved classes, no need to tell the user.
			}

			if (classesLoaded == 0)
			{
				((Grid)this.FindName("HelpOverlay")).Visibility = Visibility.Visible;
			}
		}

		#endregion

		#region Methods and Operators

		public SearchResult GetSearchResultById(int id)
		{
			return this.results.FirstOrDefault(r => r.Id == id);
		}

		public bool HasConflict(int row, int col, int span)
		{
			return this.schedItems
				.Where(si => si.Column == col)
				.Any(si => (si.Row <= row && (si.Row + si.SpanCount - 1) >= row) || (row <= si.Row && (row + span - 1) >= si.Row));
		}

		public void LoadClasses(string file)
		{
			var inFile = new StreamReader(file);
			try
			{
				if (inFile.ReadLine() != "1.3") // Get Version
				{
					// Version mismatch! Probably should do something here. Or not.
				}

				do
				{
					var r = new SearchResult
					{
						Id = Convert.ToInt32(inFile.ReadLine()),
						Department = (Dept)Convert.ToInt32(inFile.ReadLine()),
						CourseNumber = Convert.ToInt32(inFile.ReadLine()),
						CourseName = inFile.ReadLine(),
						Description = inFile.ReadLine(),
						Professor = inFile.ReadLine(),
						CourseType = (ClassType)Convert.ToInt32(inFile.ReadLine()),
						DaysOfWeek = Convert.ToInt16(inFile.ReadLine()),
						StartTime = Convert.ToInt16(inFile.ReadLine()),
						Duration = Convert.ToInt16(inFile.ReadLine()),
						PreReqs = inFile.ReadLine(),
						AntiReqs = inFile.ReadLine(),
						Status = Convert.ToInt16(inFile.ReadLine())
					};
					r.SetLabels();
					r.MouseEnter += this.r_MouseEnter;
					r.MouseLeave += this.r_MouseLeave;
					r.MouseDoubleClick += this.r_MouseDoubleClick;
					r.DetailsButton.Click += this.ShowDetails;
					this.results.Add(r);
				} while (inFile.Peek() != -1);
			}
			catch
			{
				MessageBox.Show("Something went wrong when reading the file!");
			}
			finally
			{
				inFile.Close();
			}

			this.results = this.results.OrderBy(o => o.CNum.Content).ToList(); // Huzzah! Sorts the list of courses. LINQ.
			foreach (SearchResult r in this.results)
			{
				this.Results.Children.Add(r);
			}
		}

		public void MakeScheduleItem(int newId, int row, int col, int span, string name, string type)
		{
			var i = new ScheduleItem
			{
				Id = newId,
				Row = row,
				Column = col,
				SpanCount = span,
				CNum = { Content = name },
				CTimes = { Content = (row + 6) + ":00 - " + (row + 6 + span) + ":00" },
				CType = { Content = type }
			};
			i.MouseDoubleClick += this.i_MouseDoubleClick;

			Grid.SetRow(i, row);
			Grid.SetColumn(i, col);
			Grid.SetRowSpan(i, span);

			//// Here's an idea for when we get classes at half-hour (or more) intervals
			//// Can also be useful for shifting things horizontally during conflicts.
			////double h = Schedule.LayoutRoot.RowDefinitions[1].ActualHeight;
			////i.Margin = new Thickness(0,h/2,0,h/2);

			this.schedItems.Add(i);
			this.Schedule.LayoutRoot.Children.Add(i);
			for (int cnt = 0; cnt < span; cnt++)
			{
				this.itemsInSlot[row + cnt - 1, col - 1]++;
			}
		}

		public void RemoveCourse(int id, bool changingTerms = false)
		{
			foreach (ScheduleItem i in this.schedItems)
			{
				if (i.Id == id)
				{
					for (int cnt = 0; cnt < i.SpanCount; cnt++)
					{
						this.itemsInSlot[i.Row + cnt - 1, i.Column - 1]--;
					}

					this.Schedule.LayoutRoot.Children.Remove(i);
				}
			}

			this.schedItems.RemoveAll(i => i.Id == id);
			if (!changingTerms)
			{
				this.schedIds.Remove(id);
			}

			this.ResizeItems();

			SearchResult result = this.results.Find(s => s.Id == id);
			result.Style = null;
		}

		public void ResizeItems()
		{
			double scheduleWidth = this.Schedule.LayoutRoot.ColumnDefinitions[1].ActualWidth;
			foreach (ScheduleItem item in this.schedItems)
			{
				int maxConflicts = 0;
				for (int cnt = 0; cnt < item.SpanCount; cnt++)
				{
					if (this.itemsInSlot[cnt + item.Row - 1, item.Column - 1] > maxConflicts)
					{
						maxConflicts = this.itemsInSlot[cnt + item.Row - 1, item.Column - 1];
					}
				}

				IEnumerable<ScheduleItem> conflictItems =
					this.schedItems.Where(
						si =>
							((si.Column == item.Column) &&
							((si.Row <= item.Row && (si.Row + si.SpanCount - 1) >= item.Row) ||
							(item.Row <= si.Row && (item.Row + item.SpanCount - 1) >= si.Row))));

				int itemNum = 0;
				foreach (ScheduleItem sch in conflictItems)
				{
					if (sch != item)
					{
						if (sch.Margin.Right != 0)
						{
							itemNum++;
						}
						else
						{
							break;
						}
					}
				}

				item.Margin = new Thickness(scheduleWidth * itemNum / (maxConflicts == 0 ? 1 : maxConflicts), 0,
					(maxConflicts - itemNum - 1) * scheduleWidth / (maxConflicts == 0 ? 1 : maxConflicts), 0);
			}
		}

		private void AddResult(SearchResult result)
		{
			if (this.schedItems.All(si => si.Id != result.Id))
			{
				result.Style = (Style)result.FindResource("PlacedOnSchedule");
				this.schedIds.Add(result.Id);

				foreach (Day d in Enum.GetValues(typeof(Day)))
				{
					if ((result.DaysOfWeek & (int)d) > 0)
					{
						this.MakeScheduleItem(result.Id, result.StartTime - 6, (int)Math.Log((int)d, 2), result.Duration,
							SearchResult.DepartmentNames[(int)result.Department] + result.CourseNumber, result.GetTypeName());
					}
				}

				this.ResizeItems();
			}

			foreach (Border b in this.hoverBorders)
			{
				this.Schedule.LayoutRoot.Children.Remove(b);
			}
		}

		private void AddToCart(SearchResult result)
		{
			if (this.Results.Children.Contains(result))
			{
				this.Results.Children.Remove(result);
				this.Cart.DisplayArea.Children.Add(result);
				this.cartIds.Add(result.Id);
			}
		}

		private void Cart_Drop(object sender, DragEventArgs e)
		{
			this.AddToCart((SearchResult)e.Data.GetData("Object"));

			this.Cart.Style = null;
		}

		private void Cart_OnDragEnter(object sender, DragEventArgs e)
		{
			this.Cart.Style = (Style)this.FindResource("DropTarget");
		}

		private void Cart_OnDragLeave(object sender, DragEventArgs e)
		{
			this.Cart.Style = null;
		}

		private void ChangeTerm(object sender, SelectionChangedEventArgs e)
		{
			if (!this.IsLoaded)
			{
				return;
			}

			string file = this.isWinter ? FallCoursesFile : WinterCoursesFile;
			this.isWinter = !this.isWinter;

			foreach (int id in this.schedIds.ToList())
			{
				this.RemoveCourse(id, true);
			}

			this.itemsInSlot = new int[12, 5];
			this.results.Clear();
			this.schedItems.Clear();
			this.Results.Children.Clear();
			this.Cart.DisplayArea.Children.Clear();

			this.LoadClasses(file);

			this.SwapLists(this.schedIds, this.otherTermSchedule);

			foreach (int id in this.schedIds.ToList())
			{
				this.AddResult(this.results.Find(r => r.Id == id));
			}

			this.SwapLists(this.cartIds, this.otherTermCart);
			foreach (SearchResult result in this.results.Where(r => this.cartIds.Contains(r.Id)))
			{
				this.AddToCart(result);
			}

			this.schedIds = this.schedIds.Distinct().ToList();
			this.cartIds = this.cartIds.Distinct().ToList();
		}

		private void ClearSchedule()
		{
			var itemsCopy = new List<ScheduleItem>(this.schedItems); // Allow concurrent modification
			foreach (ScheduleItem si in itemsCopy)
			{
				this.RemoveCourse(si.Id);
			}
		}

		private void ClearSearch(object sender, RoutedEventArgs e)
		{
			this.SearchBox.Text = string.Empty;
		}

		private void ComboboxChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.ResultsPane != null) // Needs this since the combobox is loaded before the schedule
			{
				this.FilterResults();
			}
		}

		private void CourseDetailOverlay_OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			this.CourseDetailOverlay.Children.Clear();
			this.CourseDetailOverlay.Visibility = Visibility.Collapsed;
		}

		private void DiscardDraft_OnClick(object sender, RoutedEventArgs e)
		{
			MessageBoxResult rsltMessageBox =
				MessageBox.Show(
					"Are you sure you want to revert all unsaved changes and revert to the previously saved version?\n\nThis CANNOT be undone.",
					"Revert changes?", MessageBoxButton.YesNo, MessageBoxImage.Warning);

			switch (rsltMessageBox)
			{
				case MessageBoxResult.No:
					return;
			}

			var statusPanel = (WrapPanel)this.FindName("ButtonStatus");
			var statusText = (TextBlock)this.FindName("ButtonStatusText");

			statusPanel.Visibility = Visibility.Visible;
			statusText.Text = "Loading...";

			try
			{
				this.LoadScheduleFromFile(CourseFile);

				statusText.Text = "Draft re-loaded from last saved version.";
			}
			catch (IOException)
			{
				statusText.Text = "We didn't find any previous drafts, so the schedule has been cleared.";
			}
			catch (InvalidOperationException)
			{
				statusText.Text = "We didn't find any previous drafts, so the schedule has been cleared.";
			}

			this.statusToastTimer.Stop();
			this.statusToastTimer.Start();
		}

		private void FilterResults()
		{
			if (!this.ResultsPane.IsVisible)
			{
				this.ToggleResults(Visibility.Visible);
			}

			string query = this.SearchBox.Text;
			int faculty = this.FacultyFilter.SelectedIndex;
			int amountFound = 0;

			foreach (SearchResult res in this.results)
			{
				if ((Regex.IsMatch(res.CName.Content.ToString(), query, RegexOptions.IgnoreCase) ||
					Regex.IsMatch(res.CProf.Content.ToString(), query, RegexOptions.IgnoreCase) ||
					Regex.IsMatch(res.CNum.Content.ToString(), query, RegexOptions.IgnoreCase)) &&
					(faculty == 0 || (Faculty)faculty == res.Faculty))
				{
					res.Visibility = Visibility.Visible;
					amountFound++;
				}
				else if (this.Results.Children.Contains(res))
				{
					res.Visibility = Visibility.Collapsed;
				}
			}

			this.NotFoundLabel.Visibility = amountFound > 0 ? Visibility.Collapsed : Visibility.Visible;
		}

		private void FinalizeDraft_OnClick(object sender, RoutedEventArgs e)
		{
			MessageBoxResult rsltMessageBox = MessageBox.Show(
				"Are you sure you want to enroll in these courses?",
				"Complete enrollment?",
				MessageBoxButton.YesNo,
				MessageBoxImage.Warning);

			switch (rsltMessageBox)
			{
				case MessageBoxResult.No:
					return;
			}

			var statusPanel = (WrapPanel)this.FindName("ButtonStatus");
			var statusText = (TextBlock)this.FindName("ButtonStatusText");

			statusPanel.Visibility = Visibility.Visible;
			statusText.Text = "Enrolling...";

			//// If this actually had a backend, this is where we'd enroll the student.

			statusText.Text = "You have been enrolled in your selected courses.";

			this.statusToastTimer.Stop();
			this.statusToastTimer.Start();
		}

		private void HelpButton_OnClick(object sender, RoutedEventArgs e)
		{
			((Grid)this.FindName("HelpOverlay")).Visibility = Visibility.Visible;
		}

		private void HelpOverlay_OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			((Grid)sender).Visibility = Visibility.Hidden;
		}

		private int LoadScheduleFromFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				File.Create(fileName);
			}

			this.ClearSchedule();

			int numClasses = 0;

			using (var sr = new StreamReader(fileName))
			{
				string line;
				var currentList = new List<int>();
				while ((line = sr.ReadLine()) != null)
				{
					if (line == "F")
					{
						currentList = this.isWinter ? this.otherTermSchedule : this.schedIds;
						currentList.Clear();
					}
					else if (line == "W")
					{
						currentList = this.isWinter ? this.schedIds : this.otherTermSchedule;
						currentList.Clear();
					}
					else
					{
						int id = Convert.ToInt32(line);
						if (currentList == this.schedIds)
						{
							this.AddResult(this.GetSearchResultById(id));
						}

						currentList.Add(id);
						numClasses++;
					}
				}

				sr.Close();
			}

			this.ResizeItems();

			return numClasses;
		}

		private void SaveDraft_OnClick(object sender, RoutedEventArgs e)
		{
			var statusPanel = (WrapPanel)this.FindName("ButtonStatus");
			var statusText = (TextBlock)this.FindName("ButtonStatusText");

			statusPanel.Visibility = Visibility.Visible;
			statusText.Text = "Saving...";

			try
			{
				File.WriteAllText(CourseFile, string.Empty);

				using (var file = new StreamWriter(CourseFile, true))
				{
					var savedIds = new HashSet<int>();

					file.WriteLine(this.isWinter ? "W" : "F");
					foreach (ScheduleItem i in this.schedItems)
					{
						if (savedIds.Contains(i.Id))
						{
							continue;
						}

						savedIds.Add(i.Id);

						file.WriteLine(Convert.ToString(i.Id));
					}

					file.WriteLine((this.isWinter ? "F" : "W"));
					foreach (int i in this.otherTermSchedule)
					{
						file.WriteLine(Convert.ToString(i));
					}

					file.Close();
				}
			}
			catch (IOException)
			{
				statusText.Text = "We were unable to save your draft!";
			}
			catch (InvalidOperationException)
			{
				statusText.Text = "We were unable to save your draft!";
			}

			statusText.Text = "Your current course setup for all semesters has been saved.";

			this.statusToastTimer.Stop();
			this.statusToastTimer.Start();
		}

		private void SchedulePanel_Drop(object sender, DragEventArgs e)
		{
			this.AddResult((SearchResult)e.Data.GetData("Object"));

			this.Schedule.Style = null;
		}

		private void SchedulePanel_OnDragEnter(object sender, DragEventArgs e)
		{
			this.Schedule.Style = (Style)this.FindResource("DropTarget");
		}

		private void SchedulePanel_OnDragLeave(object sender, DragEventArgs e)
		{
			this.Schedule.Style = null;
		}

		private void Schedule_OnResize(object sender, SizeChangedEventArgs e)
		{
			this.ResizeItems();
		}

		private void Search(object sender, TextChangedEventArgs e)
		{
			this.FilterResults();
		}

		private void SetBorder(int row, int col, int span)
		{
			var b = new Border
			{
				BorderThickness = new Thickness(5.0),
				BorderBrush = this.HasConflict(row, col, span) ? Brushes.Red : Brushes.Pink,
			};

			Grid.SetColumn(b, col);
			Grid.SetRow(b, row);
			Grid.SetRowSpan(b, span);
			this.hoverBorders.Add(b);

			this.Schedule.LayoutRoot.Children.Add(b);
		}

		private void ShowDetails(object sender, RoutedEventArgs e)
		{
			var details = new CourseDetails();
			var result = (SearchResult)((Grid)((Grid)((Button)sender).Parent).Parent).Parent; // Oh god.
			details.CName.Content = result.CName.Content;
			details.CNum.Content = result.CNum.Content;
			details.CProf.Content = result.CProf.Content;
			details.CTime.Content = result.CTime.Content;
			details.CType.Content = result.CType.Content;
			details.CStatus.Content = result.CStatus.Content;
			details.CStatus.Style = result.CStatus.Style;

			if (result.PreReqs != string.Empty)
			{
				string text = string.Empty;
				string[] chainList = result.PreReqs.Split(',');
				int chainCnt = 1;
				foreach (string chain in chainList)
				{
					text += "• ";
					string[] cidList = chain.Split('|');
					int cnt = 1;
					foreach (string cid in cidList)
					{
						string[] course = cid.Split('.');
						text += Enum.GetName(typeof(Dept), (Dept)Convert.ToInt32(course[0])) + " " + course[1];

						if (cnt++ < cidList.Count())
						{
							text += " or ";
						}
					}

					if (chainCnt++ <= chainList.Count())
					{
						text += "\n";
					}
				}

				details.Prereqs.Text = text;
			}
			else
			{
				details.PrereqContainer.Visibility = Visibility.Collapsed;
			}

			if (result.AntiReqs != string.Empty)
			{
				string text = result.AntiReqs
					.Split(',')
					.Select(chain => chain.Split('.'))
					.Aggregate(string.Empty,
						(current, course) =>
							current + ("• " + Enum.GetName(typeof(Dept), (Dept)Convert.ToInt32(course[0])) + " " + course[1] + "\n"));
				details.Antireqs.Text = text;
			}
			else
			{
				details.AntireqContainer.Visibility = Visibility.Collapsed;
			}

			this.CourseDetailOverlay.Visibility = Visibility.Visible;
			this.CourseDetailOverlay.Children.Add(details);
			double width = this.LayoutGrid.ActualWidth;
			double height = this.LayoutGrid.ActualHeight;
			double x = Mouse.GetPosition(this).X;
			double y = Mouse.GetPosition(this).Y;

			// ActualWidth/Height of details has not been calculated yet
			// Force measuring its size!
			details.Measure(new Size(width, height));
			details.Arrange(new Rect(0, 0, details.DesiredSize.Width, details.DesiredSize.Height));

			double dWidth = details.ActualWidth;
			double dHeight = details.ActualHeight;
			details.Margin = new Thickness(
				(x + dWidth > width ? width - dWidth : x),
				y + dHeight > height ? height - dHeight : y,
				0,
				0);
		}

		private void SwapLists(List<int> a, List<int> b)
		{
			var temp = new List<int>();
			temp.AddRange(a);
			a.Clear();
			a.AddRange(b);
			b.Clear();
			b.AddRange(temp);
		}

		private void ToggleResults(Visibility v)
		{
			// Animations! Because I'm bored.
			if (v == Visibility.Visible)
			{
				this.Expander.Arrow.Content = "<";
				((Storyboard)this.FindResource("ExpandResults")).Begin();
			}
			else
			{
				this.Expander.Arrow.Content = ">";
				((Storyboard)this.FindResource("HideResults")).Begin();
			}
		}

		private void Toggle_Click(object sender, RoutedEventArgs e)
		{
			this.ToggleResults(this.ResultsPane.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
		}

		private void Trash_Drop(object sender, DragEventArgs e)
		{
			var result = (SearchResult)e.Data.GetData("Object");
			if (this.Cart.DisplayArea.Children.Contains(result))
			{
				this.Cart.DisplayArea.Children.Remove(result);
				this.cartIds.Remove(result.Id);
				this.Results.Children.Clear();

				// Re-sort the children of Results. Otherwise the re-added items are placed at the end.
				foreach (SearchResult r in this.results.Where(res => !this.Cart.DisplayArea.Children.Contains(res)))
				{
					this.Results.Children.Add(r);
				}
			}

			this.FilterResults(); // Ensure the filters are followed
		}

		private void i_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			this.RemoveCourse(((ScheduleItem)sender).Id);
		}

		private void r_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			this.AddResult((SearchResult)sender);
		}

		private void r_MouseEnter(object sender, MouseEventArgs e)
		{
			var r = (SearchResult)sender;
			int row = r.StartTime - 6;
			int span = r.Duration;

			if (this.schedItems.All(si => si.Id != r.Id))
			{
				foreach (Day d in Enum.GetValues(typeof(Day)))
				{
					if ((r.DaysOfWeek & (int)d) > 0)
					{
						this.SetBorder(row, (int)(Math.Log((int)d, 2)), span);
					}
				}
			}
			else
			{
				foreach (ScheduleItem si in this.schedItems.Where(item => item.Id == r.Id))
				{
					((Storyboard)si.FindResource("DoHighlight")).Begin();
				}
			}
		}

		private void r_MouseLeave(object sender, MouseEventArgs e)
		{
			var result = (SearchResult)sender;
			if (this.schedItems.All(si => si.Id != result.Id))
			{
				foreach (Border b in this.hoverBorders)
				{
					this.Schedule.LayoutRoot.Children.Remove(b);
				}
			}
			else
			{
				foreach (ScheduleItem item in this.schedItems.Where(si => si.Id == result.Id))
				{
					((Storyboard)item.FindResource("DoHighlight")).Stop();
				}
			}

			this.hoverBorders.Clear();
		}

		#endregion
	}
}