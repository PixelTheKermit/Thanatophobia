using System.Linq;
using Content.Shared.Body.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits;
public sealed partial class TraitReplaceBodyPartsFunction : BaseTraitFunction
{
    [DataField]
    public List<(string container, ProtoId<EntityPrototype>? protoId)> Replace = new();

    public override void AddTrait(EntityUid uid, TraitPrototype traitProto, IPrototypeManager protoManager, IEntityManager entityManager)
    {
        var sysManager = IoCManager.Resolve<IEntitySystemManager>();
        var bodySystem = sysManager.GetEntitySystem<SharedBodySystem>();
        var containerSystem = sysManager.GetEntitySystem<SharedContainerSystem>();

        var bodyContainers = bodySystem.GetBodyWithOrganContainers(uid).ToList();

        foreach (var (containerID, proto) in Replace)
        {
            if (!bodyContainers.Any(part => part.ID == containerID))
                continue;

            var container = bodyContainers.Find(part => part.ID == containerID)!;

            if (container.ContainedEntities.Count > 0)
            {
                entityManager.QueueDeleteEntity(container.ContainedEntities[0]);
                containerSystem.Remove(container.ContainedEntities[0], container);
            }

            if (proto != null)
                containerSystem.Insert(entityManager.Spawn(proto), container);
        }
    }
}
