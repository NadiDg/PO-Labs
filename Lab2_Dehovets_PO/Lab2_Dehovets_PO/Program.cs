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
    
    static void Main()
    {
        Console.Write("Введіть розмір масиву: ");
        int size = int.Parse(Console.ReadLine());

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
        
        
    }
}