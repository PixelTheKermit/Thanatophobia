using System.Numerics;
using Content.Server.Worldgen.Prototypes;

namespace Content.Server.Thanatophobia.TPWorldGen;

public sealed partial class ScaleFactorCoordinateProcess : NoiseCoordinateProcess
{
    [DataField("scaleFactor")]
    public float ScaleFactor = 1f;

    public override Vector2 Process(Vector2 input)
    {
        var x = (float) Math.Floor(input.X / ScaleFactor);
        var y = (float) Math.Floor(input.Y / ScaleFactor);

        if (x < 0)
            x -= 1;

        if (y < 0)
            y -= 1;

        var output = new Vector2(x, y);

        return output;
    }
}
