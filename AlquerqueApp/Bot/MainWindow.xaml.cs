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
    
    public partial class MainWindow : Window
    {
        private Button[] buttons_black = new Button[12];
        private Button[] buttons_red = new Button[12];
        const int button_size = 30;
        
        void ArrangeButtons(int start_l, int start_b, bool is_red, int change_l, int change_b)
        {
            int l = start_l;
            int bottom = start_b;
            for (int i = 1; i < 13; i++)
            {
                Button b = new Button();
                b.Width = button_size;
                b.Height = button_size;
                Canvas.SetLeft(b, l);
                Canvas.SetBottom(b, bottom);
                canvas.Children.Add(b);
                l += change_l;
                if (i % 5 == 0)
                {
                    bottom += change_b;
                    l = start_l;
                }
                if (is_red)
                {
                    b.Background = Brushes.Red;
                    buttons_red[i - 1] = b;
                }
                else
                {
                    b.Background = Brushes.Black;
                    buttons_black[i - 1] = b;
                }
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            // downloading playing field
            var directory = FilePath.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            field.Source = new BitmapImage(new Uri(FilePath.Combine(directory, "Resourses/playing_field.jpg")));
            ArrangeButtons(509, 85, true, -68, 70);
            ArrangeButtons(237, 370, false, 68, -70);
        }
    }
}
