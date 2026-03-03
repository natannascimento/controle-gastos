namespace ControleGastos.API.Errors;

public class ErrorEnvelope
{
    public string Type { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
}
