using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client.NPC.HTN;

public sealed class HTNOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    private readonly IEntityManager _entManager = default!;
    private readonly Font _font = default!;
    private SharedTransformSystem? _xformSystem = null;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public HTNOverlay(IEntityManager entManager, IResourceCache resourceCache)
    {
        IoCManager.InjectDependencies(this);

        _entManager = entManager;

        var normalFont = _protoManager.Index<FontPrototype>("Default");

        _font = new VectorFont(resourceCache.GetResource<FontResource>(normalFont.Path), 10);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.ViewportControl == null)
            return;

        _xformSystem ??= _entManager.SystemOrNull<SharedTransformSystem>();
        if (_xformSystem is null)
            return;

        var handle = args.ScreenHandle;

        foreach (var (comp, xform) in _entManager.EntityQuery<HTNComponent, TransformComponent>(true))
        {
            if (string.IsNullOrEmpty(comp.DebugText) || xform.MapID != args.MapId)
                continue;

            var worldPos = _xformSystem.GetWorldPosition(xform);

            if (!args.WorldAABB.Contains(worldPos))
                continue;

            var screenPos = args.ViewportControl.WorldToScreen(worldPos);
            handle.DrawString(_font, screenPos + new Vector2(0, 10f), comp.DebugText, Color.White);
        }
    }
}
