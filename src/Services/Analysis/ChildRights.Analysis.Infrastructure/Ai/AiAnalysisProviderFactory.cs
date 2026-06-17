using ChildRights.Analysis.Application.Abstractions;
using ChildRights.Analysis.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ChildRights.Analysis.Infrastructure.Ai;

/// <summary>
/// Strategy selector: resolves an <see cref="IAiAnalysisProvider"/> by name, falling
/// back to the configured default model and finally to whatever is registered.
/// </summary>
internal sealed class AiAnalysisProviderFactory(
    IEnumerable<IAiAnalysisProvider> providers,
    IOptions<AiOptions> options) : IAiAnalysisProviderFactory
{
    private readonly Dictionary<string, IAiAnalysisProvider> _providers =
        providers.ToDictionary(provider => provider.ModelName, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> AvailableModels => _providers.Keys.ToList();

    public IAiAnalysisProvider Resolve(string? modelName = null)
    {
        var requested = string.IsNullOrWhiteSpace(modelName) ? options.Value.DefaultModel : modelName;

        if (_providers.TryGetValue(requested, out var provider))
        {
            return provider;
        }

        if (_providers.TryGetValue(options.Value.DefaultModel, out var defaultProvider))
        {
            return defaultProvider;
        }

        return _providers.Values.First();
    }
}
