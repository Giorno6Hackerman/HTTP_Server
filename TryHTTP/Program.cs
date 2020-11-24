using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace HTTPServer
{
    class Server
    {
        TcpListener Listener;
   
        // Запуск сервера
        public Server(int Port)
        {
            Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start();
   
            while (true)
            {
                // Новый клиент
                TcpClient Client = Listener.AcceptTcpClient();
                // Поток для нового клиента
                Thread Thread = new Thread(new ParameterizedThreadStart(ClientThread));
                Thread.Start(Client);
            }
        }
   
        static void ClientThread(Object StateInfo)
        {
            new Client((TcpClient) StateInfo);
        }
   
        // Остановка сервера
        ~Server()
        {
            if (Listener != null)
            {
                Listener.Stop();
            }
        }

        static void Main(string[] args)
        {
            new Server(80);
        }
    }
}
