using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Server.Classes
{
    class Bot {

        //поля
        private List<Bot> group;
        private int id;
        private string name;
        private Socket socket;
        private static int counter = 0;
        //свойства
        public List<Bot> Group { get { return group; } set { group = value; } }
        public string Name { get { return name; } }
        public Socket Socket { get { return socket; } }
        public Bot(Socket s)
        {
            id = counter;
            socket = s;
            name = "Bot_" + id;
            counter++;
        }
    }
}
