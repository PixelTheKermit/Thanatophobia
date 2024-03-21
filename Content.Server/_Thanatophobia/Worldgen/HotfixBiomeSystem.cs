using System.Linq;
using Content.Server.Worldgen.Components;
using Content.Server.Worldgen.Systems;
using Content.Server.Worldgen.Systems.Debris;
using Content.Shared.Storage;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Thanatophobia.Worldgen;

public sealed partial class HotfixBiomeSystem : EntitySystem
{
    [Dependency] private readonly NoiseIndexSystem _noiseIndex = default!;
    [Dependency] private readonly TransformSystem _xformSystem = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HotfixBiomeComponent, TryGetPlaceableDebrisFeatureEvent>(OnTryPlaceDebris);
    }

    private void OnTryPlaceDebris(EntityUid uid, HotfixBiomeComponent comp, ref TryGetPlaceableDebrisFeatureEvent args)
    {
        if (args.DebrisProto != null)
            return;

        var worldControllers = EntityQueryEnumerator<WorldControllerComponent>();

        if (!worldControllers.MoveNext(out var worldUid, out var _))
            return;

        foreach (var biome in comp.Biomes)
        {
            if (!_protoManager.TryIndex(biome, out var proto))
            {
                Log.Warning($"{biome} is not a valid prototype. Please create a valid prototype for this biome.");
                continue;
            }

            var success = true;
            foreach (var (noise, ranges) in proto.NoiseRanges)
            {
                var value = _noiseIndex.Evaluate(worldUid, noise, args.Coords.ToMapPos(EntityManager, _xformSystem));
                var rangeSuccess = false;
                foreach (var range in ranges)
                {
                    if (range.X <= value && range.Y >= value)
                    {
                        rangeSuccess = true;
                        break;
                    }
                }

                if (!rangeSuccess)
                {
                    success = false;
                    break;
                }
            }

            if (success)
            {
                var debris = EntitySpawnCollection.GetSpawns(proto.Debris);
                if (debris.Count != 0)
                {
                    args.DebrisProto = _random.Pick(debris);
                    break;
                }
            }
        }
    }
}
