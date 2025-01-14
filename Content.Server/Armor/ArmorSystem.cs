using Content.Shared.Armor;
using Robust.Shared.Prototypes;
using Content.Shared.Damage.Prototypes;

namespace Content.Server.Armor;

/// <inheritdoc/>
public sealed class ArmorSystem : SharedArmorSystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;

    public override void Initialize()
    {
        base.Initialize();

    }
}
