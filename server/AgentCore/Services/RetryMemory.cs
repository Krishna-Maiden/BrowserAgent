using Microsoft.Extensions.Logging;
using AgentCore.Models;
using AgentCore.Automation;

namespace AgentCore.Services
{
    public class RetryMemory
    {
        private readonly Dictionary<string, List<TaskStep>> _memory = new();
        private readonly ILogger<RetryMemory> _logger;

        public RetryMemory(ILogger<RetryMemory> logger)
        {
            _logger = logger;
        }

        public void Store(string task, List<TaskStep> steps) => _memory[task] = steps;

        public List<TaskStep>? Get(string task) =>
            _memory.TryGetValue(task, out var steps) ? steps : null;

        public void Retry(string task, PlaywrightExecutor executor)
        {
            if (_memory.TryGetValue(task, out var steps))
            {
                _logger.LogInformation($"Retrying task: {task}");
                executor.ExecutePlanAsync(steps).Wait();
            }
        }
    }
}