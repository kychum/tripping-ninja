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
	/// Interaction logic for ScheduleItem.xaml
	/// </summary>
	public partial class ScheduleItem : UserControl
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
	}
}