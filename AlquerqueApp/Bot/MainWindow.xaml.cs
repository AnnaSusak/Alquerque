﻿using System;
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
        const int button_size = 30;
        static bool isFinished = false;
        static TextBlock gameInfo = new TextBlock();
        
        public MainWindow()
        {
            InitializeComponent();
            // downloading playing field
            var directory = FilePath.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            field.Source = new BitmapImage(new Uri(FilePath.Combine(directory, "Resourses/playing_field.jpg")));
            ArrangeButtons(509, 65, true, -68, 70);
            ArrangeButtons(227, 360, false, 68, -70);
            // подготовка поля для отображения текста
            PrepareGameInfo();
            // подготовка сетей 
            int port = 8005;
            string ip_address = "127.0.0.1";
            IPAddress ip = IPAddress.Parse(ip_address);
            IPEndPoint endPoint = new IPEndPoint(ip, port);
            Socket client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client_socket.Connect(endPoint);
            gameInfo.Text = "Вы успешно подключились к серверу!\n" +
                "Подождите, пока другие юзеры подключаться.";
            // поток для получения сообщений от сервера
            Thread receiver = new Thread(GetMessageFromServer);
            receiver.Start(client_socket);
            // игровой процесс

        }
       void GetMessageFromServer(object obj)
        {
            Socket socket = (Socket)obj;
            try
            {
                while (true)
                {
                    string message = NetLib.BasicNetMethods.ReadDateFromNet(socket);
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        gameInfo.Text = message;
                    });

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
        void PrepareGameInfo()
        {
            gameInfo.VerticalAlignment = VerticalAlignment.Top;
            gameInfo.Width = 770;
            gameInfo.Height = 65;
            gameInfo.Background = Brushes.Yellow;
            canvas.Children.Add(gameInfo);
        }
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
    }
}
