/* TODO: 
 *    - Get the schedule done
 *    - Get the cart running
 *    - Get drag-and-drop working
 *    - Add in the rest of the search options
 *    - Make the thing look nice (?)
 *    - Have a button to clear the faculty filter?
 *    - Find something better than a StackViewer
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
        private List<Course> courses = new List<Course>();
        private List<SearchResult> results = new List<SearchResult>();
        const int MON = 2;
        const int TUES = 4;
        const int WEDS = 8;
        const int THURS = 16;
        const int FRI = 32;

        public MainWindow()
        {
            InitializeComponent();
            LoadClasses();
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
                    Course c;
                    c.id = Convert.ToInt32(inFile.ReadLine());
                    c.department = (Dept)Convert.ToInt32(inFile.ReadLine());
                    c.courseNum = Convert.ToInt32(inFile.ReadLine());
                    c.name = inFile.ReadLine();
                    c.desc = inFile.ReadLine();
                    c.prof = inFile.ReadLine();
                    c.type = (ClassType)Convert.ToInt32(inFile.ReadLine());
                    c.days = Convert.ToInt16(inFile.ReadLine());
                    c.startTime = Convert.ToInt16(inFile.ReadLine());
                    c.duration = Convert.ToInt16(inFile.ReadLine());

                    SearchResult r = new SearchResult();
                    r.CName.Content = departmentNames[(int)c.department] + " " + c.courseNum + " - " + c.name;
                    r.CProf.Content = c.prof;
                    r.CDesc.Text = c.desc;
                    r.CType.Content = (c.type == ClassType.Lecture ? "Lecture" : c.type == ClassType.Tutorial ? "Tutorial" : "Laboratory");
                    r.CTime.Content = "";

                    // ew.
                    if ((c.days & MON) > 0)
                    {
                        r.CTime.Content += "M";
                    }
                    if ((c.days & TUES) > 0)
                    {
                        r.CTime.Content += "T";
                    }
                    if ((c.days & WEDS) > 0)
                    {
                        r.CTime.Content += "W";
                    }
                    if ((c.days & THURS) > 0)
                    {
                        r.CTime.Content += "R";
                    }
                    if ((c.days & FRI) > 0)
                    {
                        r.CTime.Content += "F";
                    }
                    r.CTime.Content += "  " + c.startTime + ":00 - " + (c.startTime + c.duration) + ":00";


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

        public string[] departmentNames = {
            "ART",
            "BIOL",
            "BSEN",
            "CPSC",
            "EDUC",
            "ENGG",
            "ENGL",
            "MATH",
            "MUSI",
            "SENG",
        };

        private void Search(object sender, TextChangedEventArgs e)
        {
            ResultsPane.Visibility = Visibility.Visible;
            string query = ((TextBox)sender).Text;
            foreach (SearchResult res in results)
            {
                if ((Regex.IsMatch(res.CName.Content.ToString(), query, RegexOptions.IgnoreCase)) ||
                    (Regex.IsMatch(res.CProf.Content.ToString(), query, RegexOptions.IgnoreCase)) ||
                    (Regex.IsMatch(res.CDesc.Text.ToString(), query, RegexOptions.IgnoreCase)) &&
                    query != "")
                {
                    res.Visibility = Visibility.Visible;
                }
                else
                {
                    res.Visibility = Visibility.Collapsed;
                }
            }
        }
    }

    public struct Course
    {
        public int id;
        public Dept department;
        public int courseNum;
        public string name;
        public string desc;
        public string prof;
        public ClassType type;
        public short days;         // Encoded in bits. _SFTWTMS (could be reversed if desired)
        public short startTime;
        public short duration;     // Number of hours
    }

    public enum Dept
    {
        ART = 0,
        BIOL,
        BSEN,
        CPSC,
        EDUC,
        ENGG,
        ENGL,
        MATH,
        MUSI,
        SENG,
    }

    public enum ClassType
    {
        Lecture = 0,
        Tutorial,
        Laboratory,
    };

}
