using System.Numerics;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Pulling;
using Content.Shared.Pulling.Components;
using Content.Shared.Tag;
using Robust.Shared.Network;

namespace Content.Shared.Thanatophobia.Traits;

public sealed partial class SharedAddTagTraitSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AddTagTraitComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, AddTagTraitComponent comp, ComponentStartup args)
    {
        foreach (var tag in comp.Add)
        {
            _tagSystem.AddTag(uid, tag);
        }

        foreach (var tag in comp.Remove)
        {
            _tagSystem.RemoveTag(uid, tag);
        }
    }
}
