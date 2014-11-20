using System;
using System.Windows.Controls;

namespace OfCourse
{
	/// <summary>
	///     Interaction logic for SearchResult.xaml
	/// </summary>
	public partial class SearchResult : UserControl
	{
		public static string[] departmentNames =
		{
			"ART",
			"BIOL",
			"BSEN",
			"CPSC",
			"EDUC",
			"ENGG",
			"ENGL",
			"MATH",
			"MUSI",
			"SENG"
		};

		public SearchResult()
		{
			InitializeComponent();
		}

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

		public void SetLabels()
		{
			CNum.Content = departmentNames[(int) department] + " " + courseNum;
			CName.Content = name;
			CProf.Content = prof;
			CDesc.Text = desc;
			CType.Content = (type == ClassType.Lecture ? "Lecture" : type == ClassType.Tutorial ? "Tutorial" : "Laboratory");
			CTime.Content = "";

			foreach (Day d in Enum.GetValues(typeof (Day)))
			{
				if ((days & (int) d) > 0)
				{
					CTime.Content += Enum.GetName(typeof (Day), d);
				}
			}

			CTime.Content += "  " + startTime + ":00 - " + (startTime + duration) + ":00";
		}

		public string typeName()
		{
			if (type == ClassType.Lecture)
			{
				return "Lecture";
			}

			if (type == ClassType.Tutorial)
			{
				return "Tutorial";
			}

			return "Laboratory";
		}
	}
}