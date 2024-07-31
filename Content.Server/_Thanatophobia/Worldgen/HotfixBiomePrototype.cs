using System.Numerics;
using Content.Server.Worldgen.Prototypes;
using Content.Shared.Storage;
using Robust.Shared.Prototypes;

namespace Content.Server.Thanatophobia.Worldgen;

[Prototype("hotfixBiome")]
public sealed partial class HotfixBiomePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public Dictionary<ProtoId<NoiseChannelPrototype>, List<Vector2>> NoiseRanges = default!;

    [DataField(required: true)]
    public List<EntitySpawnEntry> Debris = default!;
}
