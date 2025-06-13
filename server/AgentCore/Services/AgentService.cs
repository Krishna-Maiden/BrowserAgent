using System.Threading.Tasks;
using AgentCore.AI;
using AgentCore.Automation;
using AgentCore.Models;

namespace AgentCore.Services
{
    public class AgentService
    {
        private readonly LlmProcessor _llm;
        private readonly PlaywrightExecutor _executor;
        private readonly RetryMemory _memory;

        public AgentService(LlmProcessor llm, PlaywrightExecutor executor, RetryMemory memory)
        {
            _llm = llm;
            _executor = executor;
            _memory = memory;
        }

        public async Task RunUserTask(string task)
        {
            var plan = await _llm.GeneratePlanAsync(task);
            _memory.Store(task, plan.Tasks);
            await _executor.ExecutePlanAsync(plan.Tasks);
        }
    }
}