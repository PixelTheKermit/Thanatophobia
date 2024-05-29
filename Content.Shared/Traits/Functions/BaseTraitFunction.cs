using Robust.Shared.Prototypes;

namespace Content.Shared.Traits;

[ImplicitDataDefinitionForInheritors]
public abstract partial class BaseTraitFunction
{
    /// <summary>
    /// Used for functions that may change the character's appearance, like marking functions.
    /// </summary>
    public virtual bool DynamicUpdates => false;

    /// <summary>
    /// A function that adds the trait onto the entity
    /// </summary>
    /// <param name="uid">Humanoid mob uid.</param>
    /// <param name="traitProto">The trait prototype about to be appied.</param>
    /// <param name="protoManager">For easily accessing the prototype manager.</param>
    /// <param name="entityManager">For easily accessing the entity manager.</param>
    public abstract void AddTrait(
        EntityUid uid,
        TraitPrototype traitProto,
        IPrototypeManager protoManager,
        IEntityManager entityManager
    );
}
