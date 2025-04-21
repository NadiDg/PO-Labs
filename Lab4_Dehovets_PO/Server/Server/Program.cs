using System.Net;
using System.Net.Sockets;

class Server
{
    static int clientCounter;
    static object counterLock = new();

    static void Main()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 9000);
        listener.Start();
        Console.WriteLine("Сервер запущено...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            int clientId;
            lock (counterLock) { clientId = ++clientCounter; }
            Console.WriteLine($"Клієнт {clientId}: підключився.");
            Thread thread = new Thread(() => HandleClient(client, clientId));
            thread.Start();
        }
    }

    static void HandleClient(TcpClient client, int clientId)
    {
        Console.WriteLine($"Клієнт {clientId} відключився");
        client.Close();
    }
}