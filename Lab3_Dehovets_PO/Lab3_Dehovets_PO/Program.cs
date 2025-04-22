using System.Diagnostics;

class TaskItem
{
    public int Id { get; set; }
    public int Duration { get; set; }
    public DateTime EnqueuedTime { get; set; }
}

class ThreadQueue
{
    private readonly Queue<TaskItem> queue = new();
    private readonly int capacity;
    private readonly object lockObj = new();
    public int MaxFillTimeMs { get; private set; }
    public int MinFillTimeMs { get; private set; } = int.MaxValue;
    private DateTime? fullStartTime;

    public ThreadQueue(int capacity)
    {
        this.capacity = capacity;
    }

    public bool TryEnqueue(TaskItem item)
    {
        lock (lockObj)
        {
            if (queue.Count < capacity)
            {
                queue.Enqueue(item);
                if (queue.Count == capacity && fullStartTime == null)
                {
                    fullStartTime = DateTime.Now;
                }
                Monitor.Pulse(lockObj);
                return true;
            }
            else
            {
                if (fullStartTime.HasValue)
                {
                    int fillDuration = (int)(DateTime.Now - fullStartTime.Value).TotalMilliseconds;
                    MaxFillTimeMs = Math.Max(MaxFillTimeMs, fillDuration);
                    MinFillTimeMs = Math.Min(MinFillTimeMs, fillDuration);
                    fullStartTime = null;
                }
                return false;
            }
        }
    }

    public TaskItem Dequeue()
    {
        lock (lockObj)
        {
            while (queue.Count == 0)
            {
                Monitor.Wait(lockObj);
            }
            return queue.Dequeue();
        }
    }

    public int Count => queue.Count;

    public void ResetMinFillTime()
    {
        if (MinFillTimeMs == int.MaxValue)
        {
            MinFillTimeMs = 0;
        }
    }
}

class CustomThreadPool
{
    private readonly ThreadQueue queue1 = new(10);
    private readonly ThreadQueue queue2 = new(10);
    private readonly List<Thread> workers = new();
    private volatile bool isRun = true;
    private volatile bool isPaused = false;
    private readonly object pauseLock = new();
    private int taskCounter = 0;
    private int rejectedTasks = 0;
    private readonly Random random = new();
    private readonly List<double> executionTimes = new();
    private readonly List<double> waitTimes = new();

    public void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            int queueIndex = i % 2 + 1;
            var thread = new Thread(() => Worker(queueIndex));
            thread.Start();
            workers.Add(thread);
        }
    }

    private void Worker(int queueIndex)
    {
        while (isRun)
        {
            lock (pauseLock)
            {
                while (isPaused)
                {
                    Monitor.Wait(pauseLock);
                }
            }

            TaskItem task = queueIndex == 1 ? queue1.Dequeue() : queue2.Dequeue();
            if (task != null)
            {
                var wait = (DateTime.Now - task.EnqueuedTime).TotalMilliseconds;
                lock (waitTimes) waitTimes.Add(wait);
                var sw = Stopwatch.StartNew();
                Console.WriteLine($"Завдання #{task.Id} розпочато у черзі {queueIndex} на {task.Duration}s (очікувало {wait:F0}мс)");
                Thread.Sleep(task.Duration * 1000);
                sw.Stop();
                lock (executionTimes) executionTimes.Add(sw.Elapsed.TotalMilliseconds);
                Console.WriteLine($"Завдання #{task.Id} завершено у черзі {queueIndex} після {sw.Elapsed.TotalSeconds:F2}s");
            }
        }
    }

    public void EnqueueTask()
    {
        var task = new TaskItem
        {
            Id = Interlocked.Increment(ref taskCounter),
            Duration = random.Next(4, 11),
            EnqueuedTime = DateTime.Now
        };

        bool add = false;

        if (queue1.Count < queue2.Count)
            add = queue1.TryEnqueue(task);
        else if (queue2.Count < queue1.Count)
            add = queue2.TryEnqueue(task);
        else
            add = queue1.TryEnqueue(task) || queue2.TryEnqueue(task);

        if (!add)
        {
            Interlocked.Increment(ref rejectedTasks);
            Console.WriteLine($"Завдання #{task.Id} відхилено");
        }
    }

    public void Pause()
    {
        lock (pauseLock)
        {
            isPaused = true;
            Console.WriteLine("Пул потоків призупинено.");
        }
    }

    public void Resume()
    {
        lock (pauseLock)
        {
            isPaused = false;
            Monitor.PulseAll(pauseLock);
            Console.WriteLine("Пул потоків відновлено.");
        }
    }

    public void TogglePauseResume()
    {
        lock (pauseLock)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Stop()
    {
        isRun = false;
        lock (queue1)
            Monitor.PulseAll(queue1);
        lock (queue2)
            Monitor.PulseAll(queue2);
        lock (pauseLock)
            Monitor.PulseAll(pauseLock);

        foreach (var worker in workers)
        {
            worker.Join();
        }
    }

    public void PrintStats()
    {
        queue1.ResetMinFillTime();
        queue2.ResetMinFillTime();

        Console.WriteLine("\nСтатистика:");
        Console.WriteLine($"Відхилені завдання: {rejectedTasks}");
        Console.WriteLine($"Макс. час заповнення черги 1: {queue1.MaxFillTimeMs}мс");
        Console.WriteLine($"Мін. час заповнення черги 1: {queue1.MinFillTimeMs}мс");
        Console.WriteLine($"Макс. час заповнення черги 2: {queue2.MaxFillTimeMs}мс");
        Console.WriteLine($"Мін. час заповнення черги 2: {queue2.MinFillTimeMs}мс");
        Console.WriteLine($"Середній час очікування: {waitTimes.DefaultIfEmpty(0).Average():F2} мс");
        Console.WriteLine($"Середній час виконання: {executionTimes.DefaultIfEmpty(0).Average():F2} мс");
    }
}

class Program
{
    static void Main()
    {
        var pool = new CustomThreadPool();
        pool.Start();

        var generatorThreads = new List<Thread>();

        for (int i = 0; i < 3; i++)
        {
            var t = new Thread(() =>
            {
                for (int j = 0; j < 20; j++)
                {
                    pool.EnqueueTask();
                    Thread.Sleep(new Random().Next(400, 800));
                }
            });
            generatorThreads.Add(t);
            t.Start();
        }

        var keyboardListener = new Thread(() =>
        {
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Spacebar)
                {
                    pool.TogglePauseResume();
                }
            }
        });
        keyboardListener.IsBackground = true;
        keyboardListener.Start();

        foreach (var t in generatorThreads)
            t.Join();

        Thread.Sleep(10000);
        pool.Stop();
        pool.PrintStats();
    }
}