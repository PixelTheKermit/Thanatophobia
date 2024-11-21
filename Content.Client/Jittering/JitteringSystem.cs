using System.Numerics;
using Content.Shared.Jittering;
using Content.Shared.StatusEffect;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Random;

namespace Content.Client.Jittering
{
    public sealed class JitteringSystem : SharedJitteringSystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly AnimationPlayerSystem _animationPlayer = default!;

        private readonly float[] _sign = { -1, 1 };
        private readonly string _jitterAnimationKey = "jittering";

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<JitteringComponent, ComponentStartup>(OnStartup);
            SubscribeNetworkEvent<ClientStatusEffectModifiedEvent>(OnEffectModified);
            SubscribeLocalEvent<JitteringComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<JitteringComponent, AnimationCompletedEvent>(OnAnimationCompleted);
            SubscribeLocalEvent<JitteringComponent, StatusEffectRelayEvent<AnimationCompletedEvent>>(OnAnimationCompleted);
        }

        private void OnStartup(EntityUid uid, JitteringComponent jittering, ComponentStartup args)
        {
            if (!TryComp<StatusEffectComponent>(uid, out var statusEffectComp)
            || statusEffectComp.Owner == null)
            {
                StartJitter(uid, jittering);
                return;
            }

            StartJitter(statusEffectComp.Owner.Value, jittering);
        }

        private void OnEffectModified(ClientStatusEffectModifiedEvent args)
        {
            if (!TryComp<JitteringComponent>(GetEntity(args.Effect), out var jitterComp)
            || !TryComp<StatusEffectComponent>(GetEntity(args.Effect), out var statusEffectComp)
            || statusEffectComp.NetOwner == null)
                return;

            StartJitter(GetEntity(statusEffectComp.NetOwner.Value), jitterComp);
        }

        private void StartJitter(EntityUid target, JitteringComponent jittering)
        {
            if (!TryComp(target, out SpriteComponent? sprite))
                return;

            EnsureComp<AnimationPlayerComponent>(target);

            if (!_animationPlayer.HasRunningAnimation(target, _jitterAnimationKey))
            {
                jittering.StartOffset = sprite.Offset;
                _animationPlayer.Play(target, GetAnimation(jittering, sprite), _jitterAnimationKey);
            }
        }

        private void OnShutdown(EntityUid uid, JitteringComponent jittering, ComponentShutdown args)
        {
            if (TryComp<StatusEffectComponent>(uid, out var statusEffectComp)
            && statusEffectComp.Owner != null)
            {
                var ownerUid = statusEffectComp.Owner.Value;

                if (TryComp(ownerUid, out AnimationPlayerComponent? animationPlayer))
                    _animationPlayer.Stop(ownerUid, animationPlayer, _jitterAnimationKey);

                if (TryComp(ownerUid, out SpriteComponent? sprite))
                    sprite.Offset = jittering.StartOffset;
            }
            else
            {
                if (TryComp(uid, out AnimationPlayerComponent? animationPlayer))
                    _animationPlayer.Stop(uid, animationPlayer, _jitterAnimationKey);

                if (TryComp(uid, out SpriteComponent? sprite))
                    sprite.Offset = jittering.StartOffset;
            }
        }

        private void OnAnimationCompleted(EntityUid uid, JitteringComponent jittering, AnimationCompletedEvent args)
        {
            if (args.Key != _jitterAnimationKey)
                return;

            if (HasComp<AnimationPlayerComponent>(uid)
            && TryComp(uid, out SpriteComponent? sprite))
                _animationPlayer.Play(uid, GetAnimation(jittering, sprite), _jitterAnimationKey);
        }

        private void OnAnimationCompleted(EntityUid uid, JitteringComponent jittering, StatusEffectRelayEvent<AnimationCompletedEvent> args)
        {
            if (args.Args.Key != _jitterAnimationKey)
                return;

            if (HasComp<AnimationPlayerComponent>(args.Victim)
            && TryComp(args.Victim, out SpriteComponent? sprite))
                _animationPlayer.Play(args.Victim, GetAnimation(jittering, sprite), _jitterAnimationKey);
        }

        private Animation GetAnimation(JitteringComponent jittering, SpriteComponent sprite)
        {
            var amplitude = MathF.Min(4f, jittering.Amplitude / 100f + 1f) / 10f;
            var offset = new Vector2(_random.NextFloat(amplitude / 4f, amplitude),
                _random.NextFloat(amplitude / 4f, amplitude / 3f));

            offset.X *= _random.Pick(_sign);
            offset.Y *= _random.Pick(_sign);

            if (Math.Sign(offset.X) == Math.Sign(jittering.LastJitter.X)
                || Math.Sign(offset.Y) == Math.Sign(jittering.LastJitter.Y))
            {
                // If the sign is the same as last time on both axis we flip one randomly
                // to avoid jitter staying in one quadrant too much.
                if (_random.Prob(0.5f))
                    offset.X *= -1;
                else
                    offset.Y *= -1;
            }

            var length = 0f;
            // avoid dividing by 0 so animations don't try to be infinitely long
            if (jittering.Frequency > 0)
                length = 1f / jittering.Frequency;

            jittering.LastJitter = offset;

            return new Animation()
            {
                Length = TimeSpan.FromSeconds(length),
                AnimationTracks =
                {
                    new AnimationTrackComponentProperty()
                    {
                        ComponentType = typeof(SpriteComponent),
                        Property = nameof(SpriteComponent.Offset),
                        KeyFrames =
                        {
                            new AnimationTrackProperty.KeyFrame(sprite.Offset, 0f),
                            new AnimationTrackProperty.KeyFrame(jittering.StartOffset + offset, length),
                        }
                    }
                }
            };
        }
    }
}
