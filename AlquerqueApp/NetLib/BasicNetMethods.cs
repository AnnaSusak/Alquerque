using System;
using System.Text;
using System.Net.Sockets;

namespace Lib
{
    public class BasicNetMethods
    {
        /// <summary>
        /// Отправка текста по сети
        /// <param name="socket"></param>
        /// <param name="=text"></param>
        /// </summary>
        public static void SendDataToNet(Socket socket, string text)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(text);
            socket.Send(bytes);
        }
        public static string ReadDateFromNet(Socket socket)
        {
            //буфер приема
            byte[] data = new byte[256];
            int len = socket.Receive(data);
            return Encoding.Unicode.GetString(data, 0, len);
        }
    }
}