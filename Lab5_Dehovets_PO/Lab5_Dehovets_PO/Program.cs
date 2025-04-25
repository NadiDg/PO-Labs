using System.Net;
using System.Net.Sockets;

class SimpleHttpServer
{
    private const int Port = 8080;
    private static readonly string BaseDirectory = Directory.GetCurrentDirectory();

    static async Task Main()
    {
        TcpListener server = new TcpListener(IPAddress.Any, Port);
        server.Start();
        Console.WriteLine($"HTTP сервер запущено на http://localhost:{Port}/");

        while (true)
        {
            TcpClient client = await server.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        {
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

            string requestLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(requestLine))
                return;

            Console.WriteLine($"Запит: {requestLine}");

            string[] tokens = requestLine.Split(' ');
            if (tokens.Length != 3 || tokens[0] != "GET")
            {
                await SendResponseAsync(writer, "400 Bad Request", "text/plain", "Непідтримуваний запит");
                return;
            }

            string urlPath = tokens[1].Trim('/');
            string filePath;
        }
    }
    
    static async Task SendResponseAsync()
    {
     
    }
    
    static void GetMimeType(string filePath)
    {
       
    }
}
