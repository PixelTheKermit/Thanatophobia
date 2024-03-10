using Content.Shared.Body.Components;
using Content.Shared.Body.Part;

namespace Content.Shared.Humanoid
{
    public static class HumanoidVisualLayersExtension
    {
        public static bool HasSexMorph(string layer)
        {
            return layer switch
            {
                "chest" => true,
                "head" => true,
                _ => false
            };
        }

        public static string GetSexMorph(string layer, Sex sex, string id)
        {
            if (!HasSexMorph(layer) || sex == Sex.Unsexed)
                return id;

            return $"{id}{sex}";
        }

        /// <summary>
        ///     Sublayers. Any other layers that may visually depend on this layer existing.
        ///     For example, the head has layers such as eyes, hair, etc. depending on it.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns>Enumerable of layers that depend on that given layer. Empty, otherwise.</returns>
        /// <remarks>This could eventually be replaced by a body system implementation.</remarks>
        public static IEnumerable<string> Sublayers(string layer) // TODO: Pixel: Oh god fucking damnit what did I sign myself up for?
        {
            switch (layer)
            {
                case "head":
                    yield return "head";
                    yield return "eyes";
                    yield return "headSide";
                    yield return "headTop";
                    yield return "hair";
                    yield return "facialHair";
                    yield return "snout";
                    break;
                case "l-arm":
                    yield return "l-arm";
                    yield return "l-hand";
                    break;
                case "r-arm":
                    yield return "r-arm";
                    yield return "r-hand";
                    break;
                case "l-leg":
                    yield return "l-leg";
                    yield return "l-foot";
                    break;
                case "r-leg":
                    yield return "r-leg";
                    yield return "r-foot";
                    break;
                case "chest":
                    yield return "chest";
                    yield return "tail";
                    break;
                default:
                    yield break;
            }
        }

        public static string? ToHumanoidLayers(this BodyPartComponent part)
        {
            switch (part.PartType)
            {
                case BodyPartType.Other:
                    break;
                case BodyPartType.Torso:
                    return "chest";
                case BodyPartType.Tail:
                    return "tail";
                case BodyPartType.Head:
                    // use the Sublayers method to hide the rest of the parts,
                    // if that's what you're looking for
                    return "head";
                case BodyPartType.Arm:
                    switch (part.Symmetry)
                    {
                        case BodyPartSymmetry.None:
                            break;
                        case BodyPartSymmetry.Left:
                            return "l-arm";
                        case BodyPartSymmetry.Right:
                            return "r-arm";
                    }

                    break;
                case BodyPartType.Hand:
                    switch (part.Symmetry)
                    {
                        case BodyPartSymmetry.None:
                            break;
                        case BodyPartSymmetry.Left:
                            return "l-hand";
                        case BodyPartSymmetry.Right:
                            return "r-hand";
                    }

                    break;
                case BodyPartType.Leg:
                    switch (part.Symmetry)
                    {
                        case BodyPartSymmetry.None:
                            break;
                        case BodyPartSymmetry.Left:
                            return "l-leg";
                        case BodyPartSymmetry.Right:
                            return "r-leg";
                    }

                    break;
                case BodyPartType.Foot:
                    switch (part.Symmetry)
                    {
                        case BodyPartSymmetry.None:
                            break;
                        case BodyPartSymmetry.Left:
                            return "l-foot";
                        case BodyPartSymmetry.Right:
                            return "r-foot";
                    }

                    break;
            }

            return null;
        }
    }
}
