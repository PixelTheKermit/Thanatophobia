using Content.Client.StatusEffect;
using Content.Shared.Drunk;
using Content.Shared.StatusEffect;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.Drunk;

public sealed class DrunkOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntitySystemManager _sysMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    private readonly ShaderInstance _drunkShader;

    public float CurrentBoozePower = 0.0f;

    private const float VisualThreshold = 10.0f;
    private const float PowerDivisor = 250.0f;

    private float _visualScale = 0;

    public DrunkOverlay()
    {
        IoCManager.InjectDependencies(this);
        _drunkShader = _prototypeManager.Index<ShaderPrototype>("Drunk").InstanceUnique();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        var playerEntity = _playerManager.LocalEntity;

        if (playerEntity == null)
            return;

        var statusSys = _sysMan.GetEntitySystem<StatusEffectsSystem>();

        var timeLeft = 0f;
        var total = 0;

        foreach (var effect in statusSys.GetStatusEffectsWithComponent<DrunkComponent>(playerEntity.Value))
        {
            var statusComp = _entityManager.GetComponent<StatusEffectComponent>(effect);
            timeLeft += (float) Math.Max((statusComp.Length - _timing.CurTime).TotalSeconds, 0);
            total++;
        }

        // We cannot divide by 0.
        if (total <= 0)
            return;

        CurrentBoozePower += 8f * (0.5f * timeLeft - CurrentBoozePower) * args.DeltaSeconds / (timeLeft + 1);
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_entityManager.TryGetComponent(_playerManager.LocalEntity, out EyeComponent? eyeComp))
            return false;

        if (args.Viewport.Eye != eyeComp.Eye)
            return false;

        _visualScale = BoozePowerToVisual(CurrentBoozePower);
        return _visualScale > 0;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;
        _drunkShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _drunkShader.SetParameter("boozePower", _visualScale);
        handle.UseShader(_drunkShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }

    /// <summary>
    ///     Converts the # of seconds the drunk effect lasts for (booze power) to a percentage
    ///     used by the actual shader.
    /// </summary>
    /// <param name="boozePower"></param>
    private float BoozePowerToVisual(float boozePower)
    {
        // Clamp booze power when it's low, to prevent really jittery effects
        if (boozePower < 50f)
        {
            return 0;
        }
        else
        {
            return Math.Clamp((boozePower - VisualThreshold) / PowerDivisor, 0.0f, 1.0f);
        }
    }
}
