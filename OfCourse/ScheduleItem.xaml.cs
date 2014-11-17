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

		public ScheduleItem()
		{
			this.InitializeComponent();
		}
	}
}