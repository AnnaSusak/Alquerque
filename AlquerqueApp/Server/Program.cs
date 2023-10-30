using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using Server.Classes;
public class Program
{
    static List<Bot> botsWithoutGroup = new List<Bot>();
    static List<List<Bot>> bot_groups = new List<List<Bot>>();
    const int NUM_OF_BOTS_IN_GROUP = 2;
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        int port = 8005;
        string ip_address = "127.0.0.1";
        IPAddress ip = IPAddress.Parse(ip_address);
        IPEndPoint endPoint = new IPEndPoint(ip, port);
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
            ProtocolType.Tcp);
        serverSocket.Bind(endPoint);
        serverSocket.Listen(30);
        while (true)
        {
            Console.WriteLine("[SERVER] ожидает подключения пользователей.");
            Socket clientSocket = serverSocket.Accept();
            Thread manager = new Thread(WorkWithClient);
            manager.Start(clientSocket);
        }
    }
    static void GroupUsersIfPossible()
    {
        if (botsWithoutGroup.Count >= NUM_OF_BOTS_IN_GROUP)
        {
            List<Bot> bot_group = new List<Bot>();
            for (int i = 0; i < NUM_OF_BOTS_IN_GROUP; i++)
            {
                Bot b = botsWithoutGroup[i];
                bot_group.Add(b);
                Console.WriteLine($"[SERVER] пользователь {b.Name} добавлен в группу {bot_groups.Count + 1}.");
                NetLib.BasicNetMethods.SendDataToNet(b.Socket, "Вас успешно добавили в группу.");
            }
            bot_groups.Add(bot_group);
            foreach (var b in bot_group)
            {
                b.Group = bot_group;
                botsWithoutGroup.Remove(b);
            }
            Console.WriteLine("[SERVER] добавленные в группу боты могут начать играть.");
        }
    }
    static void WorkWithClient(object obj)
    {
        Socket client_socket = (Socket)obj;
        Bot bot = new Bot(client_socket);
        NetLib.BasicNetMethods.SendDataToNet(bot.Socket, $"Ваше имя: {bot.Name}\n");
        botsWithoutGroup.Add(bot);
        int cur_thread = Thread.CurrentThread.ManagedThreadId;
        if (botsWithoutGroup.Count > 1)
        {
            //группировка
            GroupUsersIfPossible();
        }
        else
        {
            NetLib.BasicNetMethods.SendDataToNet(bot.Socket, "Пока других игроков нет, ожидайте");
        }
        try
        {
            while (true)
            {
               // процесс игры
            }
        }
        catch (Exception e)
        {
            // бот вышел, перенаправляем ботов из его группы, удаляем группу и др ненужные данные
            Console.WriteLine(bot.Name + " -> " + e.Message);
            Console.WriteLine($"Пользователь {bot.Name} вышел.");
            if (bot.Group != null && bot.Group.Count > 1)
            {
                foreach (var b in bot.Group)
                {
                    if (b != bot)
                    {
                        botsWithoutGroup.Add(b);
                        b.Group = new List<Bot>();
                        NetLib.BasicNetMethods.SendDataToNet(b.Socket, "Другой игрок из вашей группы вышел, поэтому игра закончилась." +
                            "\nЖдите подключения другого бота.");
                    }
                }
                bot_groups.Remove(bot.Group);
            }
            else
            {
                botsWithoutGroup.Remove(bot);
            }
            client_socket.Shutdown(SocketShutdown.Both);
            client_socket.Close();
            GroupUsersIfPossible();
        }
    }
}