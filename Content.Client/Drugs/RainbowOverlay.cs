using Content.Client.StatusEffect;
using Content.Shared.Drugs;
using Content.Shared.StatusEffect;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Serilog;

namespace Content.Client.Drugs;

public sealed class RainbowOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntitySystemManager _sysMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    private readonly ShaderInstance _rainbowShader;

    public float Intoxication = 0.0f;

    private const float VisualThreshold = 10.0f;
    private const float PowerDivisor = 250.0f;

    private float EffectScale => Math.Clamp((Intoxication - VisualThreshold) / PowerDivisor, 0.0f, 1.0f);

    public RainbowOverlay()
    {
        IoCManager.InjectDependencies(this);
        _rainbowShader = _prototypeManager.Index<ShaderPrototype>("Rainbow").InstanceUnique();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        var playerEntity = _playerManager.LocalEntity;

        if (playerEntity == null)
            return;

        var statusSys = _sysMan.GetEntitySystem<StatusEffectsSystem>();

        var timeLeft = 0f;
        var total = 0;

        foreach (var effect in statusSys.GetStatusEffectsWithComponent<SeeingRainbowsComponent>(playerEntity.Value))
        {
            var statusComp = _entityManager.GetComponent<StatusEffectComponent>(effect);
            timeLeft += (float) Math.Max((statusComp.Length - _timing.CurTime).TotalSeconds, 0);
            total++;
        }

        // We cannot divide by 0.
        if (total <= 0)
            return;

        timeLeft /= total;
        Intoxication += (timeLeft - Intoxication) * args.DeltaSeconds / 16f;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_entityManager.TryGetComponent(_playerManager.LocalEntity, out EyeComponent? eyeComp))
            return false;

        if (args.Viewport.Eye != eyeComp.Eye)
            return false;

        return EffectScale > 0;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;
        _rainbowShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _rainbowShader.SetParameter("effectScale", EffectScale);
        handle.UseShader(_rainbowShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
