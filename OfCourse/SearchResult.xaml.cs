using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OfCourse
{
	/// <summary>
	///     Interaction logic for SearchResult.xaml
	/// </summary>
	public partial class SearchResult
	{
		#region Fields

		public static string[] DepartmentNames =
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

		private Dept _department;

		#endregion

		#region Constructors and Destructors

		public SearchResult()
		{
			this.InitializeComponent();
		}

		#endregion

		public string AntiReqs { get; set; }

		public string CourseName { get; set; }

		public int CourseNumber { get; set; }

		public ClassType CourseType { get; set; }

		public short DaysOfWeek { get; set; }

		public Dept Department
		{
			get { return this._department; }
			set
			{
				this._department = value;
				switch (value)
				{
					case Dept.ART:
					case Dept.ENGL:
					case Dept.MUSI:
						this.Faculty = Faculty.Arts;
						break;
					case Dept.BIOL:
					case Dept.CPSC:
					case Dept.MATH:
						this.Faculty = Faculty.Science;
						break;
					case Dept.BSEN:
						this.Faculty = Faculty.Business;
						break;
					case Dept.EDUC:
						this.Faculty = Faculty.Education;
						break;
					case Dept.ENGG:
					case Dept.SENG:
						this.Faculty = Faculty.Engineering;
						break;
				}
			}
		}

		public string Description { get; set; }

		public short Duration { get; set; }

		public Faculty Faculty { get; set; }

		public int Id { get; set; }

		public string PreReqs { get; set; }

		public string Professor { get; set; }

		public short StartTime { get; set; }

		public short Status { get; set; }

		#region Methods and Operators

		public string GetTypeName()
		{
			if (this.CourseType == ClassType.Lecture)
			{
				return "Lecture";
			}

			if (this.CourseType == ClassType.Tutorial)
			{
				return "Tutorial";
			}

			return "Laboratory";
		}

		public void SetLabels()
		{
			this.CNum.Content = DepartmentNames[(int) this.Department] + " " + this.CourseNumber;
			this.CName.Content = this.CourseName;
			this.CProf.Content = this.Professor;
			this.CType.Content = (this.CourseType == ClassType.Lecture
				? "Lecture"
				: this.CourseType == ClassType.Tutorial ? "Tutorial" : "Laboratory");

			if (this.Status == 1)
			{
				this.ToolTip = "This course is closed";
				this.CStatus.Content = "CLOSED";
				this.CStatus.Style = (Style) this.FindResource("CourseStatusClosed");
			}
			else if (this.Status == 2)
			{
				this.ToolTip = "This course is under a waitlist";
				this.CStatus.Content = "WAITLIST";
				this.CStatus.Style = (Style) this.FindResource("CourseStatusWait");
			}
			else
			{
				this.ToolTip = null;
				this.CStatus.Content = "OPEN";
				this.CStatus.Style = (Style) this.FindResource("CourseStatusOpen");
			}

			this.CTime.Content = "";

			foreach (Day d in Enum.GetValues(typeof (Day)))
			{
				if ((this.DaysOfWeek & (int) d) > 0)
				{
					this.CTime.Content += Enum.GetName(typeof (Day), d);
				}
			}

			this.CTime.Content += "  " + this.StartTime + ":00 - " + (this.StartTime + this.Duration) + ":00";
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				var data = new DataObject();
				data.SetData("Object", this);

				DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
			}
		}

		#endregion
	}
}