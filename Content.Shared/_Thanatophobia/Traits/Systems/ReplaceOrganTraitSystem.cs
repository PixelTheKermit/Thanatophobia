using System.Linq;
using System.Numerics;
using Content.Shared.Body.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Pulling;
using Content.Shared.Pulling.Components;
using Content.Shared.Tag;
using Robust.Shared.Containers;
using Robust.Shared.Network;

namespace Content.Shared.Thanatophobia.Traits;

public sealed partial class SharedReplaceOrganTraitSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReplaceOrganTraitComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, ReplaceOrganTraitComponent comp, ComponentStartup args)
    {
        var bodyParts = _bodySystem.GetBodyContainers(uid).ToList();

        foreach (var (containerID, proto) in comp.Replace)
        {
            if (!bodyParts.Any(part => part.ID == containerID))
                continue;

            var container = bodyParts.Find(part => part.ID == containerID)!;

            if (container.ContainedEntities.Count > 0)
                Del(container.ContainedEntities[0]);

            if (proto != null)
                _containerSystem.Insert(Spawn(proto), container);
        }
    }
}
