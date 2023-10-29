using System;
using System.Windows.Media.Imaging;
using System.Reflection;
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

using System.Windows.Navigation;
using System.Windows.Shapes;
using FilePath = System.IO.Path;

namespace Bot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var directory = FilePath.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            field.Source = new BitmapImage(new Uri(FilePath.Combine(directory, "Resourses/playing_field.jpg")));
        }
    }
}
