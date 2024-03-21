using System.Linq;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Humanoid.Markings
{
    [DataDefinition]
    [Serializable, NetSerializable]
    public sealed partial class Marking : IEquatable<Marking>, IComparable<Marking>, IComparable<string>
    {
        [DataField]
        public List<Color> MarkingColors = new();

        [DataField]
        public NetEntity? OwnerPart = null;

        private Marking()
        {
        }

        public Marking(string markingId,
            List<Color> markingColors)
        {
            MarkingId = markingId;
            MarkingColors = markingColors;
        }

        public Marking(string markingId,
            IReadOnlyList<Color> markingColors)
            : this(markingId, new List<Color>(markingColors))
        {
        }

        public Marking(string markingId, int colorCount)
        {
            MarkingId = markingId;
            List<Color> colors = new();

            for (var i = 0; i < colorCount; i++)
                colors.Add(Color.White);

            MarkingColors = colors;
        }

        public Marking(Marking other)
        {
            MarkingId = other.MarkingId;
            MarkingColors = new(other.MarkingColors);
            Visible = other.Visible;
            Forced = other.Forced;
        }

        /// <summary>
        ///     ID of the marking prototype.
        /// </summary>
        [DataField("markingId", required: true)]
        public string MarkingId { get; private set; } = default!;

        /// <summary>
        ///     If this marking is currently visible.
        /// </summary>
        [DataField("visible")]
        public bool Visible = true;

        /// <summary>
        ///     If this marking should be forcefully applied, regardless of points.
        /// </summary>
        [ViewVariables]
        public bool Forced;

        public void SetColor(int colorIndex, Color color)
        {
            MarkingColors[colorIndex] = color;
        }

        public void SetColor(Color color)
        {
            for (var i = 0; i < MarkingColors.Count; i++)
            {
                MarkingColors[i] = color;
            }
        }

        public int CompareTo(Marking? marking)
        {
            if (marking == null)
            {
                return 1;
            }

            return string.Compare(MarkingId, marking.MarkingId, StringComparison.Ordinal);
        }

        public int CompareTo(string? markingId)
        {
            if (markingId == null)
                return 1;

            return string.Compare(MarkingId, markingId, StringComparison.Ordinal);
        }

        public bool Equals(Marking? other)
        {
            if (other == null)
            {
                return false;
            }
            return MarkingId.Equals(other.MarkingId)
                && MarkingColors.SequenceEqual(other.MarkingColors)
                && Visible.Equals(other.Visible)
                && Forced.Equals(other.Forced);
        }

        // VERY BIG TODO: TURN THIS INTO JSONSERIALIZER IMPLEMENTATION


        // look this could be better but I don't think serializing
        // colors is the correct thing to do
        //
        // this is still janky imo but serializing a color and feeding
        // it into the default JSON serializer (which is just *fine*)
        // doesn't seem to have compatible interfaces? this 'works'
        // for now but should eventually be improved so that this can,
        // in fact just be serialized through a convenient interface
        public new string ToString()
        {
            // reserved character
            string sanitizedName = MarkingId.Replace('@', '_');
            List<string> colorStringList = new();

            foreach (var color in MarkingColors)
                colorStringList.Add(color.ToHex());

            return $"{sanitizedName}@{string.Join(',', colorStringList)}";
        }

        public static Marking? ParseFromDbString(string input)
        {
            if (input.Length == 0)
                return null;

            var split = input.Split('@');

            if (split.Length != 2)
                return null;

            List<Color> colorList = new();

            foreach (var color in split[1].Split(','))
                colorList.Add(Color.FromHex(color));

            return new Marking(split[0], colorList);
        }
    }
}
