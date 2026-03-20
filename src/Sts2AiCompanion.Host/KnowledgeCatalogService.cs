using Sts2ModKit.Core.Configuration;
using Sts2ModKit.Core.Knowledge;
using FoundationKnowledgeCatalogService = Sts2AiCompanion.Foundation.Knowledge.KnowledgeCatalogService;

namespace Sts2AiCompanion.Host;

public sealed class KnowledgeCatalogService
{
    private readonly FoundationKnowledgeCatalogService _inner;

    public KnowledgeCatalogService(ScaffoldConfiguration configuration, string workspaceRoot)
        : this(new FoundationKnowledgeCatalogService(configuration, workspaceRoot))
    {
    }

    internal KnowledgeCatalogService(FoundationKnowledgeCatalogService inner)
    {
        _inner = inner;
    }

    public StaticKnowledgeCatalog CurrentCatalog => _inner.CurrentCatalog;

    public StaticKnowledgeCatalog ReloadIfChanged()
    {
        return _inner.ReloadIfChanged();
    }

    public KnowledgeSlice BuildSlice(CompanionRunState runState, int maxEntries, int maxBytes)
    {
        return _inner.BuildSlice(runState.ToFoundation(), maxEntries, maxBytes).ToHost();
    }
}
