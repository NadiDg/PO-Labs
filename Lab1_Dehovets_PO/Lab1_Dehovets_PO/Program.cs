
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
    

    static void Main()
    {
        

    }
}
