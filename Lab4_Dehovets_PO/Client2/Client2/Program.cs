using System.Net.Sockets;
using System.Diagnostics;

class Client
{
    static void Main()
    {
        Stopwatch totalTimer = Stopwatch.StartNew();

        TcpClient client = new TcpClient();
        Stopwatch connectTimer = Stopwatch.StartNew();
        client.Connect("127.0.0.1", 9000);
        connectTimer.Stop();

        Console.WriteLine($"Підключено за {connectTimer.ElapsedMilliseconds} мс");

        NetworkStream stream = client.GetStream();

        int size = 500;
        int threadsCount = 4;
        Random rand = new Random();
        double[] matrix1 = new double[size * size];
        double[] matrix2 = new double[size * size];

        for (int i = 0; i < matrix1.Length; i++)
        {
            matrix1[i] = rand.Next(-500, 501);
            matrix2[i] = rand.Next(-500, 501);
        }

        byte[] cmdBytes = new byte[10];
        System.Text.Encoding.UTF8.GetBytes("CMD:START").CopyTo(cmdBytes, 0);
        stream.Write(cmdBytes, 0, cmdBytes.Length);

        byte[] sizeBytes = BitConverter.GetBytes(size);
        byte[] threadsBytes = BitConverter.GetBytes(threadsCount);
        stream.Write(sizeBytes, 0, 4);
        stream.Write(threadsBytes, 0, 4);

        byte[] data1 = new byte[matrix1.Length * sizeof(double)];
        byte[] data2 = new byte[matrix2.Length * sizeof(double)];
        Buffer.BlockCopy(matrix1, 0, data1, 0, data1.Length);
        Buffer.BlockCopy(matrix2, 0, data2, 0, data2.Length);

        Stopwatch sendTimer = Stopwatch.StartNew();
        Console.WriteLine("Відправка матриці 1...");
        stream.Write(data1, 0, data1.Length);
        Console.WriteLine("Відправка матриці 2...");
        stream.Write(data2, 0, data2.Length);
        sendTimer.Stop();

        Console.WriteLine($"Матриці відправлено за {sendTimer.ElapsedMilliseconds} мс");

        Stopwatch receiveTimer = Stopwatch.StartNew();
        byte[] resultBytes = new byte[matrix1.Length * sizeof(double)];
        stream.Read(resultBytes, 0, resultBytes.Length);
        receiveTimer.Stop();

        Console.WriteLine($"Результат отримано за {receiveTimer.ElapsedMilliseconds} мс");

        client.Close();
        totalTimer.Stop();
        Console.WriteLine($"Загальний час: {totalTimer.ElapsedMilliseconds} мс");
    }
}
