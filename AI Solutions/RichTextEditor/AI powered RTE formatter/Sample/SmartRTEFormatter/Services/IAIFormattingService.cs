using SmartRTEFormatter;

namespace SmartRTEFormatter;

public interface IAIFormattingService
{
    Task<FormattingResult> FormatAsync(string content, CancellationToken cancellationToken = default);
}