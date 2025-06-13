using AgentCore.AI;
using AgentCore.Automation;
using AgentCore.Models;

namespace AgentCore.Services
{
    public class AgentService
    {
        private readonly LlmProcessor _llmProcessor;
        private readonly PlaywrightExecutor _executor;

        public AgentService(LlmProcessor llmProcessor, PlaywrightExecutor executor)
        {
            _llmProcessor = llmProcessor;
            _executor = executor;
        }

        public async Task RunUserTask(string userTask)
        {
            var plan = await _llmProcessor.GeneratePlan(userTask);
            await _executor.ExecutePlanAsync(plan);
        }
    }
}