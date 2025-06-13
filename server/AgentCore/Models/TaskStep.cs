
using System.Text.Json;

namespace AgentCore.Models
{
    public class TaskStep
    {
        public string Action { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public Identification Identification { get; set; }

        // Use JsonElement for flexible structure, or a specific class if login format is fixed
        public JsonElement? Value { get; set; }

        public int? TimeInSeconds { get; set; }
    }


    public class ElementIdentification
    {
        public string Type { get; set; } // e.g., "xpath", "css"
        public string Value { get; set; }
    }

    public class TaskPlan
    {
        public List<TaskStep> Tasks { get; set; }
    }
}
