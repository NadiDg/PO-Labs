using System.Diagnostics;

class Program{

    static int[] GenerateArray(int size)
    {
        Random rand = new Random();
        int[] array = new int[size];
        for (int i = 0; i < size; i++)
        {
            array[i] = rand.Next(1, 10000);
        }
        return array;
    }
    
    static void SumEven(int[] array)
    {
        long sum = 0;
        foreach (var num in array)
        {
            if (num % 2 == 0)
                sum += num;
        }
    }

    static void MinEven(int[] array)
    {
        int minEven = int.MaxValue;
        foreach (var num in array)
        {
            if (num % 2 == 0 && num < minEven)
                minEven = num;
        }
    }
    
    static void SumEvenLock(int[] array, int threadCount)
    {
        long sum = 0;
        object lockObj = new object();
        int chunkSize = array.Length / threadCount;
        Thread[] threads = new Thread[threadCount];

        for (int t = 0; t < threadCount; t++)
        {
            int start = t * chunkSize;
            int end = (t == threadCount - 1) ? array.Length : start + chunkSize;

            threads[t] = new Thread(() =>
            {
                long localSum = 0;
                for (int i = start; i < end; i++)
                {
                    if (array[i] % 2 == 0)
                        localSum += array[i];
                }
                lock (lockObj) { sum += localSum; }
            });
            threads[t].Start();
        }
        foreach (var thread in threads) thread.Join();
    }
    
    static void MinEvenLock(int[] array, int threadCount)
    {
        int minEven = int.MaxValue;
        object lockObj = new object();
        int chunkSize = array.Length / threadCount;
        Thread[] threads = new Thread[threadCount];

        for (int t = 0; t < threadCount; t++)
        {
            int start = t * chunkSize;
            int end = (t == threadCount - 1) ? array.Length : start + chunkSize;

            threads[t] = new Thread(() =>
            {
                int localMin = int.MaxValue;
                for (int i = start; i < end; i++)
                {
                    if (array[i] % 2 == 0 && array[i] < localMin)
                        localMin = array[i];
                }
                lock (lockObj) { if (localMin < minEven) minEven = localMin; }
            });
            threads[t].Start();
        }
        foreach (var thread in threads) thread.Join();
    }

    static void Main()
    {
        Console.Write("Введіть розмір масиву: ");
        int size = int.Parse(Console.ReadLine());
        Console.Write("Введіть кількість потоків: ");
        int threadCount = int.Parse(Console.ReadLine());
        
        int[] array = GenerateArray(size);

        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();
        SumEven(array);
        stopwatch.Stop();
        Console.WriteLine($"Пошук суми парних елементів (без використання паралелізації): {stopwatch.ElapsedMilliseconds} мс");

        stopwatch.Start();
        MinEven(array);
        stopwatch.Stop();
        Console.WriteLine($"Пошук найменшого парного (без використання паралелізації): {stopwatch.ElapsedMilliseconds} мс");
        
        stopwatch.Restart();
        SumEvenLock(array, threadCount);
        stopwatch.Stop();
        Console.WriteLine($"Пошук суми парних (з блокуванням): {stopwatch.ElapsedMilliseconds} мс");

        stopwatch.Restart();
        MinEvenLock(array, threadCount);
        stopwatch.Stop();
        Console.WriteLine($"Пошук найменшого парного (з блокуванням): {stopwatch.ElapsedMilliseconds} мс");
        
        
    }
}