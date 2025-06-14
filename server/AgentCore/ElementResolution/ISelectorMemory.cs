namespace AgentCore.ElementResolution
{
    public interface ISelectorMemory
    {
        string GetSelector(string logicalName);
        void SaveSelector(string logicalName, string selector);
    }
}