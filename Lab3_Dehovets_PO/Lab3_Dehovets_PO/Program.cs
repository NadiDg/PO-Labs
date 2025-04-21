
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
            if (queue.Count > 0)
                return queue.Dequeue();
            else
                return null;
        }
    }

    public int Count => queue.Count;

  
}



class Program
{
    static void Main()
    {
        
    }
}
