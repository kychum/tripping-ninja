using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OfCourse
{
	/// <summary>
	/// Interaction logic for SearchResult.xaml
	/// </summary>
	public partial class SearchResult : UserControl
	{
		public int id { get; set; }
		public Dept department { get; set; }
		public int courseNum { get; set; }
		public string name { get; set; }
		public string desc { get; set; }
		public string prof { get; set; }
		public ClassType type { get; set; }
		public short days { get; set; }
		public short startTime { get; set; }
		public short duration { get; set; }

        const int MON = 2;
        const int TUES = 4;
        const int WEDS = 8;
        const int THURS = 16;
        const int FRI = 32;
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
		
		public SearchResult()
		{
			this.InitializeComponent();
		}

        public void SetLabels()
        {
            CName.Content = departmentNames[(int)department] + " " + courseNum + " - " + name;
            CProf.Content = prof;
            CDesc.Text = desc;
            CType.Content = (type == ClassType.Lecture ? "Lecture" : type == ClassType.Tutorial ? "Tutorial" : "Laboratory");
            CTime.Content = "";

            // ew.
            if ((days & MON) > 0)
            {
                CTime.Content += "M";
            }
            if ((days & TUES) > 0)
            {
                CTime.Content += "T";
            }
            if ((days & WEDS) > 0)
            {
                CTime.Content += "W";
            }
            if ((days & THURS) > 0)
            {
                CTime.Content += "R";
            }
            if ((days & FRI) > 0)
            {
                CTime.Content += "F";
            }
            CTime.Content += "  " + startTime + ":00 - " + (startTime + duration) + ":00";
        }
	}
}