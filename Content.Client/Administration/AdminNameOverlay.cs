using System.Numerics;
using Content.Client.Administration.Systems;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;

namespace Content.Client.Administration
{
    internal sealed class AdminNameOverlay : Overlay
    {
        [Dependency] private readonly IPrototypeManager _protoManager = default!;

        private readonly AdminSystem _system;
        private readonly IEntityManager _entityManager;
        private readonly IEyeManager _eyeManager;
        private readonly EntityLookupSystem _entityLookup;
        private readonly Font _font;

        public AdminNameOverlay(AdminSystem system, IEntityManager entityManager, IEyeManager eyeManager, IResourceCache resourceCache, EntityLookupSystem entityLookup)
        {
            IoCManager.InjectDependencies(this);

            _system = system;
            _entityManager = entityManager;
            _eyeManager = eyeManager;
            _entityLookup = entityLookup;
            ZIndex = 200;

            var normalFont = _protoManager.Index<FontPrototype>("Default");
            _font = new VectorFont(resourceCache.GetResource<FontResource>(normalFont.Path), 10);
        }

        public override OverlaySpace Space => OverlaySpace.ScreenSpace;

        protected override void Draw(in OverlayDrawArgs args)
        {
            var viewport = args.WorldAABB;

            foreach (var playerInfo in _system.PlayerList)
            {
                var entity = _entityManager.GetEntity(playerInfo.NetEntity);

                // Otherwise the entity can not exist yet
                if (entity == null || !_entityManager.EntityExists(entity))
                {
                    continue;
                }

                // if not on the same map, continue
                if (_entityManager.GetComponent<TransformComponent>(entity.Value).MapID != _eyeManager.CurrentMap)
                {
                    continue;
                }

                var aabb = _entityLookup.GetWorldAABB(entity.Value);

                // if not on screen, continue
                if (!aabb.Intersects(in viewport))
                {
                    continue;
                }

                var lineoffset = new Vector2(0f, 11f);
                var screenCoordinates = _eyeManager.WorldToScreen(aabb.Center +
                                                                  new Angle(-_eyeManager.CurrentEye.Rotation).RotateVec(
                                                                      aabb.TopRight - aabb.Center)) + new Vector2(1f, 7f);
                if (playerInfo.Antag)
                {
                    args.ScreenHandle.DrawString(_font, screenCoordinates + (lineoffset * 2), "ANTAG", Color.OrangeRed);
                }
                args.ScreenHandle.DrawString(_font, screenCoordinates+lineoffset, playerInfo.Username, playerInfo.Connected ? Color.Yellow : Color.White);
                args.ScreenHandle.DrawString(_font, screenCoordinates, playerInfo.CharacterName, playerInfo.Connected ? Color.Aquamarine : Color.White);
            }
        }
    }
}
