
using System.Collections.Generic;
using System.Linq;

namespace AgentCore.Selector
{
    public class SelectorResolver
    {
        public static List<string> ResolveAll(TaskStep step)
        {
            var selectors = new List<string>();

            if (step.Identification != null)
            {
                if (!string.IsNullOrEmpty(step.Identification.Value))
                    selectors.Add(step.Identification.Value);

                // Fallbacks based on type
                if (step.Identification.Type?.ToLower() == "text")
                {
                    selectors.Add($"xpath=//*[contains(text(), '{step.Identification.Value}')]");
                    selectors.Add($"css=*:contains('{step.Identification.Value}')");
                }
                else if (step.Identification.Type?.ToLower() == "id")
                {
                    selectors.Add($"#{step.Identification.Value}");
                    selectors.Add($"[id='{step.Identification.Value}']");
                }
                else if (step.Identification.Type?.ToLower() == "name")
                {
                    selectors.Add($"[name='{step.Identification.Value}']");
                }
            }

            return selectors.Distinct().ToList();
        }
    }
}
