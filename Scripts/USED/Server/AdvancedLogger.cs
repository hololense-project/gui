using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

public class AdvancedLogger
{
    private readonly string _logFilePath;
    private readonly ConcurrentQueue<string> _logQueue;
    private readonly object _fileLock = new();

    public AdvancedLogger(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        _logFilePath = Path.Combine(directoryPath, $"log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        _logQueue = new ConcurrentQueue<string>();
    }

    public AdvancedLogger Log(string message)
    {
        string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}";
        _logQueue.Enqueue(logEntry);
        return this;
    }

    public async Task LogAsync(string message)
    {
        Log(message);
        await FlushLogsAsync();
    }

    public void FlushLogs()
    {
        while (_logQueue.TryDequeue(out string logEntry))
        {
            WriteToFile(logEntry);
        }
    }

    public async Task FlushLogsAsync()
    {
        while (_logQueue.TryDequeue(out string logEntry))
        {
            await Task.Run(() => WriteToFile(logEntry));
        }
    }

    private void WriteToFile(string logEntry)
    {
        lock (_fileLock)
        {
            using StreamWriter writer = new StreamWriter(_logFilePath, true);
            writer.WriteLine(logEntry);
        }
    }
}
