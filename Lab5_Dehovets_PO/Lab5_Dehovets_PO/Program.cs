using System.Net;
using System.Net.Sockets;
using System.Text;


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

            if (urlPath.StartsWith("page="))
            {
                string pageNumber = urlPath.Substring(5);  // Видаляємо "page="
                filePath = Path.Combine(BaseDirectory, $"page{pageNumber}.html");
            }
            else if (string.IsNullOrEmpty(urlPath))
            {
                filePath = Path.Combine(BaseDirectory, "index.html");
            }
            else
            {
                filePath = Path.Combine(BaseDirectory, urlPath);
            }

            if (File.Exists(filePath))
            {
                string content = await File.ReadAllTextAsync(filePath);
                await SendResponseAsync(writer, "200 OK", GetMimeType(filePath), content);
            }
            else
            {
                await SendResponseAsync(writer, "404 Not Found", "text/plain", "Сторінку не знайдено");
            }
        }

        client.Close();
    }

    static async Task SendResponseAsync(StreamWriter writer, string status, string contentType, string content)
    {
        await writer.WriteLineAsync($"HTTP/1.1 {status}");
        await writer.WriteLineAsync($"Content-Type: {contentType}; charset=utf-8");
        await writer.WriteLineAsync($"Content-Length: {Encoding.UTF8.GetByteCount(content)}");
        await writer.WriteLineAsync("Connection: close");
        await writer.WriteLineAsync();
        await writer.WriteAsync(content);
    }

    static string GetMimeType(string filePath)
    {
        return Path.GetExtension(filePath).ToLower() switch
        {
            ".html" => "text/html",
            ".htm" => "text/html",
            ".txt" => "text/plain",
            ".css" => "text/css",
            ".js" => "application/javascript",
            _ => "application/octet-stream"
        };
    }
}
