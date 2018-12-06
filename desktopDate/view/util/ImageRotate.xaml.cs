using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace desktopDate.view.util {
	/// <summary>
	/// ImageRotate.xaml 的交互逻辑
	/// </summary>
	public partial class ImageRotate : UserControl {
		public ImageRotate() {
			InitializeComponent();
		}
		
		//Source
		public static readonly DependencyProperty SourcePro = DependencyProperty.Register("Source", typeof(ImageSource), typeof(ImageRotate), new PropertyMetadata(null));
		[Category("Appearance")]
		public ImageSource Source {
			get { return (ImageSource)GetValue(SourcePro); }
			set { SetCurrentValue(SourcePro, value); }
		}

	}
}
