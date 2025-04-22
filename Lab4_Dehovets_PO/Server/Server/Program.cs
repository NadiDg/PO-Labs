using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

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
        Stopwatch totalTimer = Stopwatch.StartNew();

        try
        {
            NetworkStream stream = client.GetStream();
            
            byte[] cmdBuffer = new byte[10];
            stream.Read(cmdBuffer, 0, cmdBuffer.Length);
            string command = System.Text.Encoding.UTF8.GetString(cmdBuffer).Trim('\0');

            if (command != "CMD:START")
            {
                Console.WriteLine($"Клієнт {clientId}: Невідома команда.");
                client.Close();
                return;
            }

            Stopwatch receiveTimer = Stopwatch.StartNew();
            
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int size = BitConverter.ToInt32(buffer, 0);
            
            stream.Read(buffer, 0, 4);
            int threadsCount = BitConverter.ToInt32(buffer, 0);
            
            double[] matrix1 = new double[size * size];
            byte[] matrix1Bytes = new byte[matrix1.Length * sizeof(double)];
            stream.Read(matrix1Bytes, 0, matrix1Bytes.Length);
            Buffer.BlockCopy(matrix1Bytes, 0, matrix1, 0, matrix1Bytes.Length);
            
            double[] matrix2 = new double[size * size];
            byte[] matrix2Bytes = new byte[matrix2.Length * sizeof(double)];
            stream.Read(matrix2Bytes, 0, matrix2Bytes.Length);
            Buffer.BlockCopy(matrix2Bytes, 0, matrix2, 0, matrix2Bytes.Length);

            receiveTimer.Stop();
            Console.WriteLine($"Клієнт {clientId}: Матриці отримані за {receiveTimer.ElapsedMilliseconds} мс");

            Console.WriteLine($"Клієнт {clientId}: Обчислення різниці у {threadsCount} потоках...");
            Stopwatch calcTimer = Stopwatch.StartNew();

            double[] result = new double[size * size];
            Thread[] threads = new Thread[threadsCount];
            int chunkSize = result.Length / threadsCount;

            for (int t = 0; t < threadsCount; t++)
            {
                int start = t * chunkSize;
                int end = (t == threadsCount - 1) ? result.Length : start + chunkSize;

                threads[t] = new Thread(() =>
                {
                    for (int i = start; i < end; i++)
                        result[i] = matrix1[i] - matrix2[i];
                });
                threads[t].Start();
            }

            foreach (var thread in threads)
                thread.Join();

            calcTimer.Stop();
            Console.WriteLine($"Клієнт {clientId}: Різниця обчислена за {calcTimer.ElapsedMilliseconds} мс");

            Stopwatch sendTimer = Stopwatch.StartNew();
            byte[] resultBytes = new byte[result.Length * sizeof(double)];
            Buffer.BlockCopy(result, 0, resultBytes, 0, resultBytes.Length);
            stream.Write(resultBytes, 0, resultBytes.Length);
            sendTimer.Stop();

            Console.WriteLine($"Клієнт {clientId}: Результат надіслано за {sendTimer.ElapsedMilliseconds} мс");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Клієнт {clientId}: Помилка: {ex.Message}");
        }
        finally
        {
            totalTimer.Stop();
            Console.WriteLine($"Клієнт {clientId}: Загальний час: {totalTimer.ElapsedMilliseconds} мс");
            client.Close();
            Console.WriteLine($"Клієнт {clientId}: Відключився");
        }
    }
}


