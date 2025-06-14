using System.Text.Json;

namespace AgentCore.Models
{
    public class TaskPlan
    {
        public List<TaskStep> Tasks { get; set; }
    }

    public class TaskStep
    {
        public string Action { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public Identification Identification { get; set; }
        public JsonElement? Value { get; set; }
        public int? TimeInSeconds { get; set; }
    }

    public class Identification
    {
        public string Type { get; set; }  // e.g. "xpath", "css", "text"
        public string Value { get; set; }
    }
}
