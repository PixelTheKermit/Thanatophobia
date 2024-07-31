
using System.Numerics;
using Content.Server.Worldgen.Prototypes;
using Content.Shared.Storage;
using Robust.Shared.Prototypes;

namespace Content.Server.Thanatophobia.Worldgen;

[RegisterComponent]
public sealed partial class HotfixBiomeComponent : Component
{
    [DataField(required: true)]
    public List<ProtoId<HotfixBiomePrototype>> Biomes = default!;
}
