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
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Thanatophobia.Traits;

public sealed partial class SharedReplaceBodyPartTraitSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReplaceBodyPartTraitComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, ReplaceBodyPartTraitComponent comp, ComponentStartup args)
    {
        var bodyParts = _bodySystem.GetBodyContainers(uid).ToList();

        foreach (var (containerID, proto) in comp.Replace)
        {
            if (!bodyParts.Any(part => part.ID == containerID))
                continue;

            var container = bodyParts.Find(part => part.ID == containerID)!;

            if (container.ContainedEntities.Count > 0)
            {
                QueueDel(container.ContainedEntities[0]);
                _containerSystem.Remove(container.ContainedEntities[0], container);
            }

            if (proto != null)
                _containerSystem.Insert(Spawn(proto), container);
        }
    }
}
