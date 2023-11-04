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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace Bot
{

    public partial class MainWindow : Window
    {
        private Button[] buttons_black = new Button[12];
        private Button[] buttons_red = new Button[12];
        private Button[] my_buttons;
        private Button[] other_buttons;
        private int[][] pos_black;
        private int[][] pos_red;
        private int[][] pos_my;
        const int BUTTON_SIZE = 30;
        static bool isFinished = false;
        static TextBlock gameInfo = new TextBlock();
        static bool isRed;
        static double empty_x = 373;
        static double empty_y = 205;
        const int width_step = 68;
        const int height_step = 70;
        // List<Button> enabled_buts;
        List<int> enabled_inds;
        Random r = new Random();
        private Socket client_socket;

        public MainWindow()
        {
            InitializeComponent();
            // downloading playing field
            var directory = FilePath.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            field.Source = new BitmapImage(new Uri(FilePath.Combine(directory, "Resourses/playing_field.jpg")));
            ArrangeButtons(509, 65, true, -width_step, height_step);
            ArrangeButtons(227, 360, false, width_step, -height_step);
            // подготовка поля для отображения текста
            PrepareGameInfo();
            // подготовка сетей 
            PrepareNetwork();
        }
        void ArrangeButtons(int start_l, int start_b, bool is_red, int change_l, int change_b)
        {
            int l = start_l;
            int bottom = start_b;
            for (int i = 1; i < 13; i++)
            {
                Button b = new Button();
                b.Width = BUTTON_SIZE;
                b.Height = BUTTON_SIZE;
                b.IsEnabled = false;
                b.Content = (i - 1).ToString();
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
        void PrepareGameInfo()
        {
            gameInfo.VerticalAlignment = VerticalAlignment.Top;
            gameInfo.Width = 770;
            gameInfo.Height = 65;
            gameInfo.Background = Brushes.Yellow;
            canvas.Children.Add(gameInfo);
        }
        void PrepareNetwork()
        {
            int port = 8005;
            string ip_address = "127.0.0.1";
            IPAddress ip = IPAddress.Parse(ip_address);
            IPEndPoint endPoint = new IPEndPoint(ip, port);
            client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client_socket.Connect(endPoint);
            // поток для получения сообщений от сервера
            Thread receiver = new Thread(GetMessageFromServer);
            receiver.Start(client_socket);
        }
        async void GetMessageFromServer(object obj)
        {
            Socket socket = (Socket)obj;
            try
            {
                string message;
                string prev_m = "";
                while(!isFinished)
                {
                    message = Lib.BasicNetMethods.ReadDateFromNet(socket);
                    if (message.Contains(Lib.Commands.COLOR_MESSAGE_RED))
                    {
                        isRed = true;
                        my_buttons = buttons_red;
                        other_buttons = buttons_black;
                    }
                    if (message.Contains(Lib.Commands.COLOR_MESSAGE_BLACK))
                    {
                        isRed = false;
                        my_buttons = buttons_black;
                        other_buttons = buttons_red;
                    }
                    if (message.Contains(Lib.Commands.WAIT))
                    {
                        Thread.Sleep(5000);
                    }
                    if (message.Contains(Lib.Commands.YOUWIN))
                    {
                        MessageBox.Show("You win!");
                        isFinished = true;
                    }
                    if (message.Contains(Lib.Commands.YOUR_TURN_MESSAGE))
                    {
                        try {
                            //    enabled_buts = new List<Button>();
                            enabled_inds = new List<int>();
                            try
                            {
                                await EnableButtons();
                                if (enabled_inds.Count == 0)
                                {
                                    MessageBox.Show("Вы проиграли");
                                    Lib.BasicNetMethods.SendDataToNet(client_socket,Lib.Commands.ILOSE);
                                    isFinished = true;
                                } else
                                {
                                    int mv = r.Next(0, enabled_inds.Count - 1);
                                    await MakeMove(enabled_inds[mv], true);
                                    message = "";
                                    Lib.BasicNetMethods.SendDataToNet(client_socket, enabled_inds[mv].ToString());
                                }                             
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                            }
                        } catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }

                    } if (message.Contains(Lib.Commands.OTHER_TURN_MESSAGE))
                    {
                        int ind = int.Parse(message.Split()[0]);
                        string a = "r";
                        if (my_buttons == buttons_red) { a = "b"; }
                        await MakeMove(ind, false);
                        message = "";
                    }
                    if (prev_m != message)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (ThreadStart)delegate ()
                        {
                            gameInfo.Text = message;
                        });
                        prev_m = message;
                    }


                }
            }
            catch (Exception e)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                     (ThreadStart)delegate ()
                     {
                         gameInfo.Text = "[SERVER] " + e.Message;
                     });
                socket.Close();
                isFinished = true;
            }
        }
        async Task MakeMove (int i, bool my)
        {
            double a = empty_x;
            double b = empty_y;
            await Dispatcher.InvokeAsync(() =>
            {
                if (my)
                {
                    empty_x = Canvas.GetLeft(my_buttons[i]);
                    empty_y = Canvas.GetBottom(my_buttons[i]);
                    Canvas.SetBottom(my_buttons[i], b);
                    Canvas.SetLeft(my_buttons[i], a);
                } else
                {
                    empty_x = Canvas.GetLeft(other_buttons[i]);
                    empty_y = Canvas.GetBottom(other_buttons[i]);
                    Canvas.SetBottom(other_buttons[i], b);
                    Canvas.SetLeft(other_buttons[i], a);
                }
                
            });
            await Task.Delay(105);

        }
        async Task EnableButtons()
        {
                await Dispatcher.InvokeAsync(() =>
                {
                    double x;
                    double y;
                    string a = "";
                    for (int i = 0; i < 12; i++) {
                        x = Canvas.GetLeft(my_buttons[i]);
                        y = Canvas.GetBottom(my_buttons[i]);
                        if(x == empty_x && (y - height_step == empty_y || y + height_step == empty_y))
                        {
                            enabled_inds.Add(i);
                            a += i;
                        } else if (y == empty_y && (x - width_step == empty_x || x + width_step == empty_x))
                        {
                            enabled_inds.Add(i);
                            a += i;
                        } else if (x + width_step == empty_x && (y - height_step == empty_y | y + height_step == empty_y))
                        {
                            enabled_inds.Add(i);
                            a += i;
                        }
                        else if (y + height_step == empty_y && (x - width_step == empty_x | x + width_step == empty_x))
                        {
                            enabled_inds.Add(i);
                            a += i;
                        }
                        else if (x - width_step == empty_x && (y - height_step == empty_y | y + height_step == empty_y))
                        {
                            enabled_inds.Add(i);
                            a += i;
                        }
                        else if (y - height_step == empty_y && (x - width_step == empty_x | x + width_step == empty_x))
                        {
                            enabled_inds.Add(i);
                            a += i;
                        }
                    }
                  //  MessageBox.Show(a);
                });
                await Task.Delay(25);
        }
    }
}
