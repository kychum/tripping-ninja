/* TODO: 
 *    - Add in advanced search options
 *    - Keyboard accessiblity?
 *    - Get mouse hover when disabled to highlight the course somehow
 *    - Integrate requisites to the program
 *    - Integrate different terms to the program
 *    - (Would be nice) Get drag-and-drop to dynamically update [Alex]
 *    
 *  Comments from Richard:
 *    - What is the relationship between the cart and schedule?
 *          - How will the cart look?
 *    - Deal with switching semesters?
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
		private readonly List<Border> hoverBorders = new List<Border>();
		private readonly List<ScheduleItem> schedItems = new List<ScheduleItem>();
		private List<SearchResult> results = new List<SearchResult>();
        int[,] itemsInSlot = new int[12,5];

		public MainWindow()
		{
			InitializeComponent();
			LoadClasses();
			Expander.Toggle.Click += Toggle_Click;
            Cart.Cart.Drop += Cart_Drop;

			((Grid)FindName("HelpOverlay")).Visibility = Visibility.Visible;
			// TODO Load draft, if any
		}

		private void Toggle_Click(object sender, RoutedEventArgs e)
		{
			ToggleResults(ResultsPane.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
		}

		private void ToggleResults(Visibility v)
		{
			// Animations! Because I'm bored.
			if (v == Visibility.Visible)
			{
				Expander.Arrow.Content = "<";
				((Storyboard)FindResource("ExpandResults")).Begin();
			}
			else
			{
				Expander.Arrow.Content = ">";
				((Storyboard)FindResource("HideResults")).Begin();
			}
		}

		public void LoadClasses()
		{
			var inFile = new StreamReader("..\\..\\classes.txt");
			try
			{
				if (inFile.ReadLine() != "1.3") // Get Version
				{
					// Version mismatch! Probably should do something here. Or not.
				}

				//inFile.ReadLine(); // Discard numClasses
				do
				{
					var r = new SearchResult
					{
						id = Convert.ToInt32(inFile.ReadLine()),
						department = (Dept)Convert.ToInt32(inFile.ReadLine()),
						courseNum = Convert.ToInt32(inFile.ReadLine()),
						name = inFile.ReadLine(),
						desc = inFile.ReadLine(),
						prof = inFile.ReadLine(),
						type = (ClassType)Convert.ToInt32(inFile.ReadLine()),
						days = Convert.ToInt16(inFile.ReadLine()),
						startTime = Convert.ToInt16(inFile.ReadLine()),
						duration = Convert.ToInt16(inFile.ReadLine()),
                        prereqs = inFile.ReadLine(),
                        antireqs = inFile.ReadLine(),
                        status = Convert.ToInt16(inFile.ReadLine())
					};
					r.SetLabels();
					r.MouseEnter += r_MouseEnter;
					r.MouseLeave += r_MouseLeave;
					r.MouseDoubleClick += r_MouseDoubleClick;
					results.Add(r);
				} while (inFile.Peek() != -1);
			}
			catch
			{
				// Do something. Anything.
				MessageBox.Show("Something went wrong when reading the file!");
			}
			finally
			{
				inFile.Close();
			}

			results = results.OrderBy(o => o.CNum.Content).ToList(); // Huzzah! Sorts the list of courses. LINQ.
			foreach (SearchResult r in results)
			{
				Results.Children.Add(r);
			}
		}

		private void r_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
            AddResult((SearchResult)sender);
		}

		private void r_MouseLeave(object sender, MouseEventArgs e)
		{
			foreach (Border b in hoverBorders)
			{
				Schedule.LayoutRoot.Children.Remove(b);
			}

			hoverBorders.Clear();
		}

		private void r_MouseEnter(object sender, MouseEventArgs e)
		{
			var r = (SearchResult) sender;
            int row = r.startTime - 6;
            int span = r.duration;

			foreach (Day d in Enum.GetValues(typeof (Day)))
            {
				if ((r.days & (int) d) > 0)
                {
					SetBorder(row, (int) (Math.Log((int) d, 2)), span);
                }
            }
		}

		private void SetBorder(int row, int col, int span)
		{
			var b = new Border
			{
				BorderThickness = new Thickness(5.0),
				BorderBrush = HasConflict(row, col, span) ? Brushes.Red : Brushes.Pink,
			};

			Grid.SetColumn(b, col);
			Grid.SetRow(b, row);
			Grid.SetRowSpan(b, span);
			hoverBorders.Add(b);

			Schedule.LayoutRoot.Children.Add(b);
		}

        private void AddResult(SearchResult result)
        {
            result.IsEnabled = false;

            foreach (Day d in Enum.GetValues(typeof(Day)))
            {
                if ((result.days & (int)d) > 0)
                {
                    MakeScheduleItem(result.id, result.startTime - 6, (int)Math.Log((int)d, 2), result.duration,
                        SearchResult.departmentNames[(int)result.department] + result.courseNum, result.typeName());
                }
            }

            ResizeItems();
        }
		private void Search(object sender, TextChangedEventArgs e)
		{
            FilterResults();
		}

		private void ClearSearch(object sender, RoutedEventArgs e)
		{
			SearchBox.Text = "";
		}

		public void MakeScheduleItem(int newId, int row, int col, int span, string name, string type)
		{
			var i = new ScheduleItem();
			i.id = newId;
			Grid.SetRow(i, row);
			i.row = row;
			Grid.SetColumn(i, col);
			i.col = col;
			Grid.SetRowSpan(i, span);
			i.span = span;

			i.CNum.Content = name;
			i.CTimes.Content = (row + 6) + ":00 - " + (row + 6 + span) + ":00";
			i.CType.Content = type;

			// Here's an idea for when we get classes at half-hour (or more) intervals
			// Can also be useful for shifting things horizontally during conflicts.
			//double h = Schedule.LayoutRoot.RowDefinitions[1].ActualHeight;
			//i.Margin = new Thickness(0,h/2,0,h/2);

			schedItems.Add(i);
			Schedule.LayoutRoot.Children.Add(i);
            for (int cnt = 0; cnt < span; cnt++)
            {
                itemsInSlot[row + cnt - 1, col - 1]++;
            }
		}

        public void ResizeItems()
        {
            double scheduleWidth = Schedule.LayoutRoot.ColumnDefinitions[1].ActualWidth;
            foreach (var item in schedItems)
            {
                int maxConflicts = 0;
                for (int cnt = 0; cnt < item.span; cnt++)
                {
                    if (itemsInSlot[cnt + item.row - 1, item.col - 1] > maxConflicts)
                    {
                        maxConflicts = itemsInSlot[cnt + item.row - 1, item.col - 1];
                    }
                }
                var conflictItems = schedItems.Where(si => ((si.col == item.col) && ((si.row <= item.row && (si.row + si.span - 1) >= item.row) || (item.row <= si.row && (item.row + item.span - 1) >= si.row))));
                int itemNum = 0;
                foreach (var sch in conflictItems)
                {
                    if (sch != item)
                        if(sch.Margin.Right != 0)
                            itemNum++;
                    else
                        break;
                }
                item.Margin = new Thickness(scheduleWidth * itemNum / maxConflicts, 0, (maxConflicts - itemNum - 1) * scheduleWidth / maxConflicts, 0);
            }
        }

		public void RemoveCourse(int id)
		{
			foreach (ScheduleItem i in schedItems)
			{
				if (i.id == id)
				{
					Schedule.LayoutRoot.Children.Remove(i);
				}
			}
			schedItems.RemoveAll(i => i.id == id);
			results.Find(s => s.id == id).IsEnabled = true;
		}

		public bool HasConflict(int row, int col, int span)
		{
			return schedItems
				.Where(si => si.col == col)
				.Any(si => (si.row <= row && (si.row + si.span - 1) >= row) || (row <= si.row && (row + span - 1) >= si.row));
		}

		private void SaveDraft_OnClick(object sender, RoutedEventArgs e)
		{
			var statusPanel = (WrapPanel)FindName("ButtonStatus");
			var statusText = (TextBlock)FindName("ButtonStatusText");

			statusPanel.Visibility = Visibility.Visible;
			statusText.Text = "Saving...";

			// TODO Actually save the draft

			statusText.Text = "Your current course setup has been saved.";

			var aTimer = new DispatcherTimer();
			aTimer.Tick += (timerSender, timerEventArgs) =>
			{
				statusPanel.Visibility = Visibility.Hidden;
				aTimer.Stop();
			};
			aTimer.Interval = new TimeSpan(0, 0, 3);
			aTimer.Start();
		}

		private void DiscardDraft_OnClick(object sender, RoutedEventArgs e)
		{
			MessageBoxResult rsltMessageBox = MessageBox.Show("Are you sure you want to discard all unsaved changes? This CANNOT be undone.", "Discard draft?", MessageBoxButton.YesNo, MessageBoxImage.Warning);

			switch (rsltMessageBox)
			{
				case MessageBoxResult.No:
					return;
			}

			var statusPanel = (WrapPanel)FindName("ButtonStatus");
			var statusText = (TextBlock)FindName("ButtonStatusText");

			statusPanel.Visibility = Visibility.Visible;
			statusText.Text = "Saving...";

			// TODO Reload draft from previous, or wipe it if none is found

			statusText.Text = "All unsaved changes have been discarded.";

			var aTimer = new DispatcherTimer();
			aTimer.Tick += (timerSender, timerEventArgs) =>
			{
				statusPanel.Visibility = Visibility.Hidden;
				aTimer.Stop();
			};
			aTimer.Interval = new TimeSpan(0, 0, 3);
			aTimer.Start();
		}

		private void FinalizeDraft_OnClick(object sender, RoutedEventArgs e)
		{
			MessageBoxResult rsltMessageBox = MessageBox.Show("Are you sure you want to enroll in these courses?", "Complete enrollment?", MessageBoxButton.YesNo, MessageBoxImage.Warning);

			switch (rsltMessageBox)
			{
				case MessageBoxResult.No:
					return;
			}

			var statusPanel = (WrapPanel)FindName("ButtonStatus");
			var statusText = (TextBlock)FindName("ButtonStatusText");

			statusPanel.Visibility = Visibility.Visible;
			statusText.Text = "Saving...";

			// TODO What to do here?

			statusText.Text = "You have been enrolled in your selected courses.";

			var aTimer = new DispatcherTimer();
			aTimer.Tick += (timerSender, timerEventArgs) =>
			{
				statusPanel.Visibility = Visibility.Hidden;
				aTimer.Stop();
			};
			aTimer.Interval = new TimeSpan(0, 0, 3);
			aTimer.Start();
		}

		private void HelpOverlay_OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			((Grid)sender).Visibility = Visibility.Hidden;
		}

		private void HelpButton_OnClick(object sender, RoutedEventArgs e)
		{
			((Grid)FindName("HelpOverlay")).Visibility = Visibility.Visible;
		}

        private void SchedulePanel_Drop(object sender, DragEventArgs e)
        {
            AddResult((SearchResult)e.Data.GetData("Object"));
        }

        private void Cart_Drop(object sender, DragEventArgs e)
        {
            var result = (SearchResult) e.Data.GetData("Object");
            if (Results.Children.Contains(result))
            {
                Results.Children.Remove(result);
                Cart.DisplayArea.Children.Add(result);
            }
        }

        private void ComboboxChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ResultsPane != null) // Needs this since the combobox is loaded before the schedule
                FilterResults();
        }

        private void FilterResults()
        {
            if (!ResultsPane.IsVisible)
                ToggleResults(Visibility.Visible);
            string query = SearchBox.Text;
            int faculty = FacultyFilter.SelectedIndex;
            int amountFound = 0;
            foreach (SearchResult res in results)
            {
                if (((Regex.IsMatch(res.CName.Content.ToString(), query, RegexOptions.IgnoreCase)) ||
                    (Regex.IsMatch(res.CProf.Content.ToString(), query, RegexOptions.IgnoreCase)) ||
                    (Regex.IsMatch(res.CDesc.Text, query, RegexOptions.IgnoreCase)) ||
                    (Regex.IsMatch(res.CNum.Content.ToString(), query, RegexOptions.IgnoreCase))) &&
                    ((faculty == 0) || ((Faculty)faculty == res.faculty)))
                {
                    res.Visibility = Visibility.Visible;
                    amountFound++;
                }
                else
                {
                    res.Visibility = Visibility.Collapsed;
                }
            }

            NotFoundLabel.Visibility = amountFound > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Schedule_OnResize(object sender, SizeChangedEventArgs e)
        {
            ResizeItems();
        }
	}
}