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
    /// <summary>
    /// A function that adds the marking onto the entity
    /// </summary>
    /// <param name="uid">Humanoid mob uid.</param>
    /// <param name="markingObject">The marking object about to be appied.</param>
    /// <param name="bodyPartContainers">All the body part containers in the humanoid.</param>
    /// <param name="protoManager">For easily accessing the prototype manager.</param>
    /// <param name="entityManager">For easily accessing the entity manager.</param>
    public abstract void AddMarking(
        EntityUid uid,
        Marking markingObject,
        IEnumerable<BaseContainer> bodyPartContainers,
        IPrototypeManager protoManager,
        IEntityManager entityManager
    );

    public abstract int GetSpriteCount();
    public virtual Dictionary<string, List<SpriteSpecifier?>> GetSprites()
    {
        return new();
    }
}
