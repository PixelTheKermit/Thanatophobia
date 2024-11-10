using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Prototypes;
using Content.Shared.StatusEffect;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.StatusEffect;
public sealed partial class StatusEffectsSystem : SharedStatusEffectsSystem
{
    [Dependency] private readonly IConsoleHost _consoleHost = default!;
    [Dependency] private readonly SharedStatusEffectsSystem _sharedSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusEffectsComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<StatusEffectComponent, StatusEffectRelayEvent<StatusEffectUpdateEvent>>(EffectUpdate);
        SubscribeLocalEvent<AdjustEffectStrengthEffectComponent, StatusEffectActivateEvent>(AdjustStrengthEffect);

        _consoleHost.RegisterCommand("addeffect",
            Loc.GetString("add-effect-command"),
            "addeffect <uid> <effect ID> <strength> <timer>",
            AddEffectCommand,
            StatusCommandCompletion);

        InitializeInflictor();
    }

    private void OnShutdown(EntityUid uid, StatusEffectsComponent component, ComponentShutdown args)
    {
        if (component.StatusContainer == null)
            return;

        foreach (var effectUid in component.StatusContainer.ContainedEntities)
        {
            QueueDel(effectUid);
        }
    }

    private void EffectUpdate(EntityUid uid, StatusEffectComponent comp, StatusEffectRelayEvent<StatusEffectUpdateEvent> args)
    {
        if (!comp.IsTimed)
            return;

        var curTime = Timing.CurTime;

        if (curTime > comp.Length)
        {
            RaiseLocalEvent(uid, new StatusEffectTimeoutEvent(args.Victim));
            QueueDel(uid);
        }
    }

    private void AdjustStrengthEffect(EntityUid uid, AdjustEffectStrengthEffectComponent comp, StatusEffectActivateEvent args)
    {
        if (!TryComp<StatusEffectComponent>(uid, out var effectComp))
            return;

        var newStrength = (int) (effectComp.OverallStrength * comp.Multipler + comp.AddedOn);

        ModifyEffect(uid, newStrength, null, StatusEffectApplicationType.Override, effectComp);
    }

    [AdminCommand(AdminFlags.Fun)]
    private void AddEffectCommand(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2)
        {
            shell.WriteError("Too few arguments, arguments that can be used (in order) are: Entity Uid, Effect Prototype, optionally the strength of the effect and the length in seconds.");
            return;
        }

        if (!EntityUid.TryParse(args[0], out var uid) || !HasComp<StatusEffectsComponent>(uid))
        {
            shell.WriteError("Entity either doesn't exist or cannot have effects.");
            return;
        }

        if (!PrototypeManager.TryIndex<EntityPrototype>(args[1], out var effectPrototype) || !effectPrototype.HasComponent<StatusEffectComponent>())
        {
            shell.WriteError("Prototype either does not exist, isn't an entity, or doesn't have StatusEffectComponent.");
            return;
        }

        var strength = 1;
        TimeSpan? length = null;

        if (args.TryGetValue(2, out var strStrength) && int.TryParse(strStrength, out var newStrength))
            strength = newStrength;

        if (args.TryGetValue(3, out var strLength) && float.TryParse(strLength, out var newLength))
            length = TimeSpan.FromSeconds(newLength);

        _sharedSystem.ApplyEffect(uid, effectPrototype.ID, 0, strength, length, StatusEffectApplicationType.Add);
    }

    private CompletionResult StatusCommandCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHint("<uid>");

        if (args.Length == 2)
            return CompletionResult.FromHint("<protoID>");

        if (args.Length == 3)
            return CompletionResult.FromHint("<strength>");

        if (args.Length == 4)
            return CompletionResult.FromHint("<length>");

        return CompletionResult.Empty;
    }
}
