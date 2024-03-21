using System.Linq;
using Content.Shared.Body.Part;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid.Markings;

[NetSerializable, Serializable]
public sealed partial class AddMarkingFunction : BaseMarkingFunction
{
    [DataField(required: true)]
    public string BodyPart = default!;

    [DataField]
    public Dictionary<string, List<SpriteSpecifier>> Sprites { get; private set; } = default!;

    public override void AddMarking(
        EntityUid uid,
        Marking markingObject,
        IEnumerable<BaseContainer> bodyPartContainers,
        IPrototypeManager protoManager,
        IEntityManager entityManager
    )
    {
        var prototype = protoManager.Index<MarkingPrototype>(markingObject.MarkingId);

        var slotPartID = $"body_part_slot_{BodyPart}";
        var slotOrganID = $"body_organ_slot_{BodyPart}";

        BaseContainer? slot;

        if (bodyPartContainers.Any(x => slotPartID == x.ID))
            slot = bodyPartContainers.First(x => x.ID == slotPartID);
        else if (bodyPartContainers.Any(x => slotOrganID == x.ID))
            slot = bodyPartContainers.First(x => x.ID == slotOrganID);
        else if (bodyPartContainers.Any(x => BodyPart == x.ID)) // Try using the raw name, for special cases like the root part.
            slot = bodyPartContainers.First(x => x.ID == BodyPart);
        else
            return;

        if (slot.ContainedEntities.Count == 0)
            return;

        var part = slot.ContainedEntities[0];

        if (!entityManager.TryGetComponent<BodyPartVisualiserComponent>(part, out var bodyPartVisual))
            return;

        bodyPartVisual.Markings.AddBack(prototype.MarkingCategory, markingObject);
    }

    public override int GetSpriteCount()
    {
        var totalLayers = 0;

        foreach (var (_, sprites) in Sprites)
        {
            totalLayers = Math.Max(sprites.Count, totalLayers);
        }

        return totalLayers;
    }

    public override Dictionary<string, List<SpriteSpecifier>> GetSprites()
    {
        return Sprites;
    }
}
