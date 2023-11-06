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
using System.Runtime.ConstrainedExecution;

namespace Bot
{
    public class Position
    {
        public double x;
        public double y;
        public Position(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
    public class InfoForMove
    {
        public int ind_pos;
        public int ind_but;
        public bool eat;
        public int other_but_ind;
        public InfoForMove(int pos, int ind, bool eat)
        {
            ind_pos = pos;
            ind_but = ind;
            this.eat = eat;
        }
        public InfoForMove(int pos, int ind, bool eat, int ind_other)
        {
            ind_pos = pos;
            ind_but = ind;
            this.eat = eat;
            other_but_ind = ind_other;
        }
    }
    public partial class MainWindow : Window
    {
        private List<Button> buttons_black = new List<Button>();
        private List<Button> buttons_red= new List<Button>();
        private List<Button> my_buttons= new List<Button>();
        private List<Button> other_buttons= new List<Button>();
      //  private int[][] pos_black;
       // private int[][] pos_red;
       // private int[][] pos_my;
        const int BUTTON_SIZE = 30;
        static bool isFinished = false;
        static TextBlock gameInfo = new TextBlock();
        static bool isRed;
        static double empty_x = 373;
        static double empty_y = 205;
        List<Position> empty_pos = new List<Position>();
        const int width_step = 68;
        const int height_step = 70;
        // List<Button> enabled_buts;
        // SortedDictionary<int, int> enabled_inds = new SortedDictionary<int, int>();
        List<InfoForMove> enabled_moves = new List<InfoForMove>();
        Random r = new Random();
        private Socket client_socket;

        public MainWindow()
        {
            InitializeComponent();
            // downloading playing field
            var directory = FilePath.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            field.Source = new BitmapImage(new Uri(FilePath.Combine(directory, "Resourses/playing_field.jpg")));
            ArrangeButtons(509, 65, true, -width_step, height_step);
            ArrangeButtons(237, 345, false, width_step, -height_step);
            empty_pos.Add(new Position(373, 205));
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
                b.Foreground = Brushes.Cyan;
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
                    buttons_red.Add(b);
                }
                else
                {
                    b.Background = Brushes.Black;
                    buttons_black.Add(b);
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
                      //  MessageBox.Show("red");
                    }
                    if (message.Contains(Lib.Commands.COLOR_MESSAGE_BLACK))
                    {
                        isRed = false;
                        my_buttons = buttons_black;
                        other_buttons = buttons_red;
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (ThreadStart)delegate ()
                        {
                            gameInfo.Background = Brushes.Beige;
                        });
                        /// MessageBox.Show("black");
                    }
                   /* if (message.Contains(Lib.Commands.WAIT))
                    {
                        Thread.Sleep(6000);
                    }*/
                    if (message.Contains(Lib.Commands.YOUWIN))
                    {
                        MessageBox.Show("You win!");
                        isFinished = true;
                    }
                    if (message.Contains(Lib.Commands.YOUR_TURN_MESSAGE))
                    {
                        try {
                            //    enabled_buts = new List<Button>();
                            //    enabled_inds = new SortedDictionary<int, int>();
                            enabled_moves = new List<InfoForMove>();
                            try
                            {
                                if (my_buttons.Count != 0)
                                    await FindPossibleButtonIndsForMove();
                                //   MessageBox.Show("found");
                             //   MessageBox.Show(enabled_moves.Count.ToString());
                                if (enabled_moves.Count == 0 || my_buttons.Count== 0)
                                {
                                    MessageBox.Show("Вы проиграли");
                                    Lib.BasicNetMethods.SendDataToNet(client_socket,Lib.Commands.ILOSE.ToString());
                                    isFinished = true;
                                } else
                                {
                                    int mv = r.Next(0, enabled_moves.Count - 1);
                                    await MakeMove(enabled_moves[mv], true);
                                    message = "";
                                    Lib.BasicNetMethods.SendDataToNet(client_socket, enabled_moves[mv].ind_but + " " +
                                        enabled_moves[mv].ind_pos.ToString());
                                }                             
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message + " " + (my_buttons == buttons_red));
                            }
                        } catch (Exception e)
                        {
                            MessageBox.Show(e.Message + " " + (my_buttons == buttons_red));
                        }

                    } if (message.Contains(Lib.Commands.OTHER_TURN_MESSAGE))
                    {
                        int ind = int.Parse(message.Split()[0]);
                        int ind_empty = int.Parse(message.Split()[1]);
                       // MessageBox.Show(ind + " " + ind_empty);
                        string a = "r";
                        if (my_buttons == buttons_red) { a = "b"; }
                        await MakeMove(new InfoForMove(ind_empty, ind, false), false);
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
        async Task MakeMove (InfoForMove inf, bool my)
        {
            try
            {
                int ind_of_empty_pos = inf.ind_pos;
                int ind_of_button = inf.ind_but;
                double a = empty_pos[ind_of_empty_pos].x;
                double b = empty_pos[ind_of_empty_pos].y;
                await Dispatcher.InvokeAsync(() =>
                {
                    if (my)
                    {
                        empty_pos[ind_of_empty_pos].x = Canvas.GetLeft(my_buttons[ind_of_button]);
                        empty_pos[ind_of_empty_pos].y = Canvas.GetBottom(my_buttons[ind_of_button]);
                        Canvas.SetBottom(my_buttons[ind_of_button], b);
                        Canvas.SetLeft(my_buttons[ind_of_button], a);
                        if (inf.eat)
                        {

                            empty_pos.Add(new Position(Canvas.GetLeft(other_buttons[inf.other_but_ind]),
                                Canvas.GetBottom(other_buttons[inf.other_but_ind])));
                            other_buttons[inf.other_but_ind].Visibility = Visibility.Hidden;
                            other_buttons.RemoveAt(inf.other_but_ind);
                        }
                    }
                    else
                    {
                        empty_pos[ind_of_empty_pos].x = Canvas.GetLeft(other_buttons[ind_of_button]);
                        empty_pos[ind_of_empty_pos].y = Canvas.GetBottom(other_buttons[ind_of_button]);
                        Canvas.SetBottom(other_buttons[ind_of_button], b);
                        Canvas.SetLeft(other_buttons[ind_of_button], a);
                    }

                });
                await Task.Delay(105);
            } catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            

        }
        async Task FindPossibleButtonIndsForMove()
        {
                await Dispatcher.InvokeAsync(() =>
                {
                    double x;
                    double y;
                    string a = "";
                    for (int i = 0; i < my_buttons.Count; i++) {
                        for(int j = 0; j < empty_pos.Count; j++)
                        {
                            x = Canvas.GetLeft(my_buttons[i]);
                            y = Canvas.GetBottom(my_buttons[i]);
                            double empty_x = empty_pos[j].x;
                            double empty_y = empty_pos[j].y;
// просто шаг на пустую клетку
                      //      MessageBox.Show("m " +i + " "+ empty_x + " " + empty_y + " " + x + " " + y);
                            if (x == empty_x && (y - height_step == empty_y || y + height_step == empty_y))
                            {
                                enabled_moves.Add(new InfoForMove(j, i, false));
                                a += i;
                            }
                          else if (y == empty_y && (x - width_step == empty_x || x + width_step == empty_x))
                            {
                                enabled_moves.Add(new InfoForMove(j, i, false));
                                a += i;
                            }
                           else if (x + width_step == empty_x && (y - height_step == empty_y | y + height_step == empty_y))
                            {
                                enabled_moves.Add(new InfoForMove(j, i, false));
                                a += i;
                            }
                            else if (y + height_step == empty_y && (x - width_step == empty_x | x + width_step == empty_x))
                            {
                                enabled_moves.Add(new InfoForMove(j, i, false));
                                a += i;
                            }
                            else if (x - width_step == empty_x && (y - height_step == empty_y | y + height_step == empty_y))
                            {
                                enabled_moves.Add(new InfoForMove(j, i, false));
                                a += i;
                            }
                            else if (y - height_step == empty_y && (x - width_step == empty_x | x + width_step == empty_x))
                            {
                                enabled_moves.Add(new InfoForMove(j, i, false));
                                a += i;
                            }
                            // съесть чужую фишку
                            if(x == empty_x && y + 2 * height_step == empty_y)
                            {
                                bool ok = false;
                                int res_ind = 0;
                                for (int k = 0; !ok && k < other_buttons.Count; k++)
                                {
                                    if (Canvas.GetLeft(other_buttons[k]) == x && Canvas.GetBottom(other_buttons[k]) + height_step == y)
                                    {
                                        ok = true;
                                        res_ind = k;
                                    }
                                }
                                if(ok)
                                {
                                    enabled_moves.Add(new InfoForMove(j, i, true, res_ind));
                                    MessageBox.Show(j + " " + i + " " + true + " " + res_ind);
                                } /*else
                                {
                                    string cur = "";
                                    for (int k = 0; !ok && k < other_buttons.Count; k++)
                                    {
                                        cur += k + " " + i+ " "  + Canvas.GetBottom(other_buttons[k]) + height_step + "\n";
                                    }
                                   // MessageBox.Show(cur);
                                }*/
                            }
                        }
                    }
                  //  MessageBox.Show(a);
                });
                await Task.Delay(25);
        }
    }
}
