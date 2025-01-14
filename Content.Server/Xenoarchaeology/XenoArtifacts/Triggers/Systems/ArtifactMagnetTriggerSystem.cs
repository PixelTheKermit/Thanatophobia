using System.Linq;
using Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;
using Content.Shared.Clothing;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Systems;

/// <summary>
/// This handles artifacts that are activated by magnets, both salvage and magboots.
/// </summary>
public sealed class ArtifactMagnetTriggerSystem : EntitySystem
{
    [Dependency] private readonly ArtifactSystem _artifact = default!;

    private readonly List<EntityUid> _toActivate = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!EntityQuery<ArtifactMagnetTriggerComponent>().Any())
            return;

        _toActivate.Clear();

        //assume that there's more instruments than artifacts
        var query = EntityQueryEnumerator<MagbootsComponent, TransformComponent>();
        while (query.MoveNext(out _, out var magboot, out var magXform))
        {
            if (!magboot.On)
                continue;

            var artiQuery = EntityQueryEnumerator<ArtifactMagnetTriggerComponent, TransformComponent>();
            while (artiQuery.MoveNext(out var artifactUid, out var trigger, out var xform))
            {
                if (!magXform.Coordinates.TryDistance(EntityManager, xform.Coordinates, out var distance))
                    continue;

                if (distance > trigger.Range)
                    continue;

                _toActivate.Add(artifactUid);
            }
        }

        foreach (var a in _toActivate)
        {
            _artifact.TryActivateArtifact(a);
        }
    }
}
