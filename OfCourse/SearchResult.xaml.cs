using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;

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
        private Dept m_department;
		public Dept department 
        {
            get { return m_department; }
            set
            {
                m_department = value;
                switch (value)
                {
                    case Dept.ART:
                    case Dept.ENGL:
                    case Dept.MUSI:
                        faculty = Faculty.Arts;
                        break;
                    case Dept.BIOL:
                    case Dept.CPSC:
                    case Dept.MATH:
                        faculty = Faculty.Science;
                        break;
                    case Dept.BSEN:
                        faculty = Faculty.Business;
                        break;
                    case Dept.EDUC:
                        faculty = Faculty.Education;
                        break;
                    case Dept.ENGG:
                    case Dept.SENG:
                        faculty = Faculty.Engineering;
                        break;
                }
            }
        }
		public int courseNum { get; set; }
		public string name { get; set; }
		public string desc { get; set; }
		public string prof { get; set; }
		public ClassType type { get; set; }
		public short days { get; set; }
		public short startTime { get; set; }
		public short duration { get; set; }
        public string prereqs { get; set; }
        public string antireqs { get; set; }
        private short m_status;
        public short status {
            get { return m_status; } 
            set
            {
                m_status = value;
                if (value == 1)
                {
                    this.ToolTip = "This course is closed";
                    //this.Rectangle.Fill = Brushes.Pink;
                }
                else if (value == 2) 
                {
                    this.ToolTip = "This course is under a waitlist;";
                    //this.Rectangle.Fill = Brushes.LightYellow;
                }
                else
                {
                    //this.Rectangle.Fill = Brushes.LightGreen;
                }
            }
        }
        public Faculty faculty { get; set; }

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

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DataObject data = new DataObject();
                data.SetData("Object", this);

                DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
            }
        }
	}
}