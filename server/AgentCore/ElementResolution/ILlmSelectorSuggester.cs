using System.Threading.Tasks;
using Microsoft.Playwright;

namespace AgentCore.ElementResolution
{
    public interface ILlmSelectorSuggester
    {
        Task<string> SuggestSelectorAsync(IPage page, string logicalName);
    }
}