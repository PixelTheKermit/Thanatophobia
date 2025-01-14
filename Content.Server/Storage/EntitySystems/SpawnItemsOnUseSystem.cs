using Content.Server.Administration.Logs;
using Content.Server.Storage.Components;
using Content.Shared.Database;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;
using static Content.Shared.Storage.EntitySpawnCollection;

namespace Content.Server.Storage.EntitySystems
{
    public sealed class SpawnItemsOnUseSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly SharedHandsSystem _hands = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SpawnItemsOnUseComponent, UseInHandEvent>(OnUseInHand);
        }

        private void OnUseInHand(EntityUid uid, SpawnItemsOnUseComponent component, UseInHandEvent args)
        {
            if (args.Handled)
                return;

            // If starting with zero or less uses, this component is a no-op
            if (component.Uses <= 0)
                return;

            var coords = Transform(args.User).Coordinates;
            var spawnEntities = GetSpawns(component.Items, _random);
            EntityUid? entityToPlaceInHands = null;

            foreach (var proto in spawnEntities)
            {
                entityToPlaceInHands = Spawn(proto, coords);
                _adminLogger.Add(LogType.EntitySpawn, LogImpact.Low, $"{ToPrettyString(args.User)} used {ToPrettyString(uid)} which spawned {ToPrettyString(entityToPlaceInHands.Value)}");
            }

            if (component.Sound != null)
            {
                _audio.PlayPvs(component.Sound, uid);
            }

            component.Uses--;

            // Delete entity only if component was successfully used
            if (component.Uses <= 0)
            {
                args.Handled = true;
                EntityManager.DeleteEntity(uid);
            }

            if (entityToPlaceInHands != null)
            {
                _hands.PickupOrDrop(args.User, entityToPlaceInHands.Value);
            }
        }
    }
}
