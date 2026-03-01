namespace NexusCopy.Services;

using NexusCopy.Core.Interfaces;
using NexusCopy.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Service for managing copy job history and log files.
/// </summary>
public class LogService : ILogService
{
    private readonly string _appDataPath;
    private readonly string _logsDirectory;
    private readonly string _historyFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the LogService class.
    /// </summary>
    /// <param name="appDataPath">The application data directory path.</param>
    public LogService(string? appDataPath = null)
    {
        _appDataPath = appDataPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NexusCopy");

        // Ensure directories exist
        Directory.CreateDirectory(_appDataPath);
        _logsDirectory = Path.Combine(_appDataPath, "logs");
        Directory.CreateDirectory(_logsDirectory);

        _historyFilePath = Path.Combine(_appDataPath, "history.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <inheritdoc />
    public async Task SaveJobAsync(CopyJob job)
    {
        await _semaphore.WaitAsync();
        try
        {
            var history = await LoadHistoryAsync();
            
            // Add or update the job
            var existingIndex = history.FindIndex(j => j.Id == job.Id);
            if (existingIndex >= 0)
            {
                history[existingIndex] = job;
            }
            else
            {
                history.Insert(0, job); // Insert at beginning for chronological order
            }

            // Keep only the last 100 jobs
            if (history.Count > 100)
            {
                history = history.Take(100).ToList();
            }

            await SaveHistoryAsync(history);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CopyJob>> GetJobHistoryAsync(int maxCount = 100)
    {
        await _semaphore.WaitAsync();
        try
        {
            var history = await LoadHistoryAsync();
            return history.Take(maxCount).ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<CopyJob?> GetJobAsync(Guid id)
    {
        var history = await GetJobHistoryAsync();
        return history.FirstOrDefault(j => j.Id == id);
    }

    /// <inheritdoc />
    public async Task ClearHistoryAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            await SaveHistoryAsync(new List<CopyJob>());
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public string CreateLogFilePath(Guid jobId)
    {
        return Path.Combine(_logsDirectory, $"job-{jobId}.log");
    }

    private async Task<List<CopyJob>> LoadHistoryAsync()
    {
        try
        {
            if (!File.Exists(_historyFilePath))
            {
                return new List<CopyJob>();
            }

            var json = await File.ReadAllTextAsync(_historyFilePath);
            var data = JsonSerializer.Deserialize<HistoryData>(json, _jsonOptions);
            return data?.Jobs?.ToList() ?? new List<CopyJob>();
        }
        catch (Exception ex)
        {
            // Log error and return empty list
            Console.WriteLine($"Error loading job history: {ex.Message}");
            return new List<CopyJob>();
        }
    }

    private async Task SaveHistoryAsync(IReadOnlyList<CopyJob> jobs)
    {
        var data = new HistoryData { Jobs = jobs };
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        await File.WriteAllTextAsync(_historyFilePath, json);
    }

    /// <summary>
    /// Disposes the service.
    /// </summary>
    public void Dispose()
    {
        _semaphore.Dispose();
    }
}

/// <summary>
/// JSON data structure for history file.
/// </summary>
internal class HistoryData
{
    public IReadOnlyList<CopyJob> Jobs { get; set; } = Array.Empty<CopyJob>();
}
