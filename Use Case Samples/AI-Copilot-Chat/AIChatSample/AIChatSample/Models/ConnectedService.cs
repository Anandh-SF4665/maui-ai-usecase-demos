namespace AIChatSample.Models
{
    /// <summary>One row in the Connected Services card.</summary>
    public sealed class ConnectedService
    {
        public string Name        { get; init; } = string.Empty;
        public string IconSource  { get; init; } = string.Empty;
        public bool   IsConnected { get; init; }
        public string ActionText  { get; init; } = "Connect";
    }
}