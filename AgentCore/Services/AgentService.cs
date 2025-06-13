using AgentCore.AI;
using AgentCore.Automation;
using AgentCore.Models;

namespace AgentCore.Services
{
    public class AgentService
    {
        private readonly LlmProcessor _llmProcessor;
        private readonly PlaywrightExecutor _executor;
        private readonly RetryMemory _memory;

        public AgentService(LlmProcessor llmProcessor, PlaywrightExecutor executor, RetryMemory memory)
        {
            _llmProcessor = llmProcessor;
            _executor = executor;
            _memory = memory;
        }

        public async Task RunUserTask(string userTask)
        {
            var cached = _memory.Get(userTask);
            if (cached != null)
            {
                await _executor.ExecutePlanAsync(cached);
                return;
            }

            var plan = await _llmProcessor.GeneratePlan(userTask);
            _memory.Store(userTask, plan);
            await _executor.ExecutePlanAsync(plan);
        }

        public void Retry(string userTask)
        {
            _memory.Retry(userTask, _executor);
        }
    }
}