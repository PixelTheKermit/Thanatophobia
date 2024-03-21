using Content.Shared.Body.Part;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid.Markings;

[NetSerializable, Serializable]
[ImplicitDataDefinitionForInheritors]
public abstract partial class BaseMarkingFunction
{
    public abstract void AddMarking(
        EntityUid uid,
        Marking markingObject,
        IEnumerable<BaseContainer> bodyPartContainers,
        IPrototypeManager protoManager,
        IEntityManager entityManager
    );

    public abstract int GetSpriteCount();
    public virtual Dictionary<string, List<SpriteSpecifier>> GetSprites()
    {
        return new();
    }
}
