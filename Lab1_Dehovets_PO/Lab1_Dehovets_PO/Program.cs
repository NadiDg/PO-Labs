using System.Diagnostics;

class Program
{
    static Random globalRand = new();
    static object randLock = new();

    static int[,] GenerateMatrix(int size)
    {
        int[,] matrix = new int[size, size];

        lock (randLock)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    matrix[i, j] = globalRand.Next(-1000, 1000);
                }
            }
        }
        return matrix;
    }
    static void Subtraction(int[,] a, int[,] b)
    {
        int size = a.GetLength(0);
        int[,] result = new int[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                result[i, j] = a[i, j] - b[i, j];
            }
        }
    }

    static void Main()
    {
        int n = int.Parse(Console.ReadLine());
        
        int[,] matrixA = GenerateMatrix(n);
        int[,] matrixB = GenerateMatrix(n);
        
        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();
        Subtraction(matrixA, matrixB);
        stopwatch.Stop();
        Console.WriteLine($"Час виконання віднімання послідовно: {stopwatch.ElapsedMilliseconds} мс");

    }
}
