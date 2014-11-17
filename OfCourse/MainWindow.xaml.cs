/* TODO: 
 *    - Get the schedule done
 *          - Find something better than a grid for the schedule
 *    - Get the cart running
 *    - Get drag-and-drop working
 *    - Add in the rest of the search options
 *    - Make the thing look nice (?)
 *    - Have a button to clear the faculty filter?
 *    - Stop the scrollbar from messing with contol placement (ref. http://stackoverflow.com/questions/24694723/reserve-space-for-scrollviewer )
 *    - Keyboard accessiblity?
 *    - Get some borders up to make things look better
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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace OfCourse
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<SearchResult> results = new List<SearchResult>();
        private List<Border> hoverBorders = new List<Border>();

        public MainWindow()
        {
            InitializeComponent();
            LoadClasses();
            Expander.Toggle.Click += Toggle_Click;
        }

        void Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (ResultsPane.Visibility == Visibility.Visible)
            {
                ToggleResults(Visibility.Collapsed);
            }
            else
            {
                ToggleResults(Visibility.Visible);
            }
        }

        void ToggleResults(Visibility v)
        {
            ResultsPane.Visibility = v;
            if (v == Visibility.Visible)
                Expander.Arrow.Content = "<";
            else
                Expander.Arrow.Content = ">";
        }

        public void LoadClasses()
        {
            StreamReader inFile = new StreamReader("..\\..\\classes.txt");
            try
            {
                if (inFile.ReadLine() != "1.1") // Get Version
                {
                    // Version mismatch! Probably should do something here. Or not.
                }
                int numClasses = Convert.ToInt32(inFile.ReadLine()); // Maybe superfluous since an array isn't being used
                do
                {
                    SearchResult r = new SearchResult();
                    r.id = Convert.ToInt32(inFile.ReadLine());
                    r.department = (Dept)Convert.ToInt32(inFile.ReadLine());
                    r.courseNum = Convert.ToInt32(inFile.ReadLine());
                    r.name = inFile.ReadLine();
                    r.desc = inFile.ReadLine();
                    r.prof = inFile.ReadLine();
                    r.type = (ClassType)Convert.ToInt32(inFile.ReadLine());
                    r.days = Convert.ToInt16(inFile.ReadLine());
                    r.startTime = Convert.ToInt16(inFile.ReadLine());
                    r.duration = Convert.ToInt16(inFile.ReadLine());
                    r.SetLabels();
                    r.MouseEnter += r_MouseEnter;
                    r.MouseLeave += r_MouseLeave;
                    results.Add(r);
                }
                while (inFile.Peek() != -1);
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

            results = results.OrderBy(o => o.CName.Content).ToList(); // Huzzah! Sorts the list of courses. LINQ.
            foreach (SearchResult r in results)
            {
                Results.Children.Add(r);
            }
        }

        void r_MouseLeave(object sender, MouseEventArgs e)
        {
            foreach (Border b in hoverBorders)
            {
                Schedule.LayoutRoot.Children.Remove(b);
            }

            hoverBorders.Clear();
        }

        void r_MouseEnter(object sender, MouseEventArgs e)
        {
            SearchResult r = (SearchResult)sender;
            int row = r.startTime-6;
            int span = r.duration;

            foreach (Day d in Enum.GetValues(typeof(Day)))
            {
                if ((r.days & (int)d) > 0)
                {
                    SetBorder(row, (int)(Math.Log((int)d,2)), span);
                }
            }
        }

        void SetBorder(int row, int col, int span)
        {
            Border b = new Border();
            b.BorderThickness = new Thickness(5.0);
            b.BorderBrush = Brushes.Pink;
            Grid.SetColumn(b, col);
            Grid.SetRow(b, row);
            Grid.SetRowSpan(b, span);
            hoverBorders.Add(b);

            Schedule.LayoutRoot.Children.Add(b);
        }

        private void Search(object sender, TextChangedEventArgs e)
        {
            ToggleResults(Visibility.Visible);
            string query = ((TextBox)sender).Text;
            int amountFound = 0;
            foreach (SearchResult res in results)
            {
                if ((Regex.IsMatch(res.CName.Content.ToString(), query, RegexOptions.IgnoreCase)) ||
                    (Regex.IsMatch(res.CProf.Content.ToString(), query, RegexOptions.IgnoreCase)) ||
                    (Regex.IsMatch(res.CDesc.Text.ToString(), query, RegexOptions.IgnoreCase)) &&
                    query != "")
                {
                    res.Visibility = Visibility.Visible;
                    amountFound++;
                }
                else
                {
                    res.Visibility = Visibility.Collapsed;
                }
            }
            if (amountFound > 0)
            {
                NotFoundLabel.Visibility = Visibility.Collapsed;
            }
            else{
                NotFoundLabel.Visibility = Visibility.Visible;
            }
        }
    }
}
