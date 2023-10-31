using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Server.Classes
{
    public enum Color { Red, Black};
    class Bot {

        //поля
        private List<Bot> group;
        private int id;
        private string name;
        private Socket socket;
        private static int counter = 0;
        private Color color;
        
        //свойства
        public List<Bot> Group { get { return group; } set { group = value; } }
        public Color Color { get { return color; } set { color = value; } }
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
