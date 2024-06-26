using System.Linq;
using Content.Shared.Body.Part;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid.Markings;

[NetSerializable, Serializable]
public sealed partial class ReplaceBodyPartsMarkingFunction : BaseMarkingFunction
{
    [DataField]
    public Dictionary<string, Dictionary<string, List<SpriteSpecifier?>>> Sprites { get; private set; } = default!;

    public override void AddMarking(
        EntityUid uid,
        Marking markingObject,
        IEnumerable<BaseContainer> bodyPartContainers,
        IPrototypeManager protoManager,
        IEntityManager entityManager
    )
    {
        var prototype = protoManager.Index<MarkingPrototype>(markingObject.MarkingId);

        foreach (var (slotName, protoSprites) in Sprites)
        {
            var slotPartID = $"body_part_slot_{slotName}";
            var slotOrganID = $"body_organ_slot_{slotName}";

            BaseContainer? slot;

            if (bodyPartContainers.Any(x => slotPartID == x.ID))
                slot = bodyPartContainers.First(x => x.ID == slotPartID);
            else if (bodyPartContainers.Any(x => slotOrganID == x.ID))
                slot = bodyPartContainers.First(x => x.ID == slotOrganID);
            else if (bodyPartContainers.Any(x => slotName == x.ID)) // Try using the raw name, for special cases like the root part.
                slot = bodyPartContainers.First(x => x.ID == slotName);
            else
                continue;

            if (slot.ContainedEntities.Count == 0)
                continue;

            var part = slot.ContainedEntities[0];

            if (!entityManager.TryGetComponent<BodyPartVisualiserComponent>(part, out var bodyPartVisual) || bodyPartVisual.IsReplaceable == false)
                continue;

            bodyPartVisual.CustomSprites = new();
            foreach (var (markingLayer, sprites) in protoSprites)
            {
                bodyPartVisual.CustomSprites.Sprites[markingLayer] = new();
                var colours = new List<Color>();
                for (var i = 0; i < sprites.Count; i++)
                {
                    var colour = Color.White;

                    if (markingObject.MarkingColors.Count > i)
                        colour = markingObject.MarkingColors[i];

                    colours.Add(colour);

                    bodyPartVisual.CustomSprites.Sprites[markingLayer] = sprites;
                }
            }
        }
    }

    public override int GetSpriteCount()
    {
        var totalLayers = 0;

        foreach (var (_, layers) in Sprites)
        {
            foreach (var (_, sprites) in layers)
                totalLayers = Math.Max(sprites.Count, totalLayers);
        }

        return totalLayers;
    }
}
