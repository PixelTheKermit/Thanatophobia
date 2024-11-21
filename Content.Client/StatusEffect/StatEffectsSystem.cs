using Content.Shared.StatusEffect;
using Robust.Client.GameObjects;

namespace Content.Client.StatusEffect;

/// <inheritdoc/>
public sealed partial class StatusEffectsSystem : SharedStatusEffectsSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusEffectsComponent, AnimationCompletedEvent>(RelayEvent);
    }

    /// <summary>
    /// Used to relay an event that an entity recieved into it's effects so that the event can be modified by the effects.
    /// </summary>
    private void RelayEvent<TEvent>(EntityUid uid, StatusEffectsComponent comp, TEvent args)
    {
        var relayedArgs = new StatusEffectRelayEvent<TEvent>(args, uid);

        if (comp.StatusContainer == null)
            return;

        foreach (var effect in comp.StatusContainer.ContainedEntities)
        {
            RaiseLocalEvent(effect, relayedArgs);
        }
    }
}
