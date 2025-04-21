using System.Net.Sockets;

class Client
{
    static void Main()
    {
        TcpClient client = new TcpClient();
        client.Connect("127.0.0.1", 9000);
        NetworkStream stream = client.GetStream();

        int size = 1000;
        Random rand = new Random();
        double[] matrix1 = new double[size * size];
        double[] matrix2 = new double[size * size];

        for (int i = 0; i < matrix1.Length; i++)
        {
            matrix1[i] = rand.Next(-500, 501);
            matrix2[i] = rand.Next(-500, 501);
        }

        byte[] sizeBytes = BitConverter.GetBytes(size);
        stream.Write(sizeBytes, 0, 4);

        byte[] data1 = new byte[matrix1.Length * sizeof(double)];
        byte[] data2 = new byte[matrix2.Length * sizeof(double)];
        Buffer.BlockCopy(matrix1, 0, data1, 0, data1.Length);
        Buffer.BlockCopy(matrix2, 0, data2, 0, data2.Length);

        stream.Write(data1, 0, data1.Length);
        stream.Write(data2, 0, data2.Length);

        byte[] resultBytes = new byte[matrix1.Length * sizeof(double)];
        stream.Read(resultBytes, 0, resultBytes.Length);

        client.Close();
    }
}