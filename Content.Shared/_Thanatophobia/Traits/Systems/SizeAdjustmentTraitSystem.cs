using System.Numerics;
using Robust.Shared.Network;

namespace Content.Shared.Thanatophobia.Traits;

public sealed class SizeAdjustmentTraitSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SizeAdjustmentTraitComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, SizeAdjustmentTraitComponent comp, ComponentStartup args)
    {
        if (_netManager.IsClient && !_entityManager.IsClientSide(uid))
            return;

        EnsureComp<ScaleVisualsComponent>(uid);
        if (!_appearanceSystem.TryGetData(uid, ScaleVisuals.Scale, out Vector2 oldScale))
            oldScale = Vector2.One;

        _appearanceSystem.SetData(uid, ScaleVisuals.Scale, oldScale * comp.Height);
    }
}
