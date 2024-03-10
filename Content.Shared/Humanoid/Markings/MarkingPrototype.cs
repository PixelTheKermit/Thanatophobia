using System.Linq;
using Content.Shared.Body.Part;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid.Markings
{
    [Prototype("marking")]
    public sealed partial class MarkingPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = "uwu";

        public string Name { get; private set; } = default!;

        [DataField(required: true)]
        public SpriteSpecifier Icon = default!;

        [DataField]
        public bool ReplacesBodyParts = false;

        [DataField(required: true)]
        // TODO: Pixel: Marking categories gotta become prototypes or something.
        public string MarkingCategory { get; private set; } = default!;

        [DataField]
        public List<string>? SpeciesRestrictions { get; private set; }

        [DataField]
        public Sex? SexRestriction { get; private set; }

        [DataField]
        public bool ForcedColoring { get; private set; } = false;

        [DataField]
        public bool UseSkinColour { get; private set; } = false;

        [DataField]
        public MarkingColors Coloring { get; private set; } = new();


        [DataField]
        public Dictionary<string, Dictionary<string, List<BodyPartVisualiserSprite>>> Sprites { get; private set; } = default!;

        public Marking AsMarking()
        {
            return new Marking(ID, GetLayerCount());
        }

        public int GetLayerCount()
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
}
