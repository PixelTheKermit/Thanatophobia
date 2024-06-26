using System.Linq;
using Content.Client.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.Humanoid;

public sealed class HumanoidAppearanceSystem : SharedHumanoidAppearanceSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MarkingManager _markingManager = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HumanoidAppearanceComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, HumanoidAppearanceComponent component, ref AfterAutoHandleStateEvent args)
    {
        UpdateSprite(uid, component, Comp<SpriteComponent>(uid));
    }

    public void UpdateSprite(EntityUid uid, HumanoidAppearanceComponent component, SpriteComponent sprite)
    {
        UpdatePartVisuals(uid, component, sprite);
    }

    private static bool IsHidden(HumanoidAppearanceComponent humanoid, string layer)
        => humanoid.HiddenLayers.Contains(layer) || humanoid.PermanentlyHidden.Contains(layer);

    private void UpdatePartVisuals(EntityUid uid, HumanoidAppearanceComponent component, SpriteComponent spriteComp)
    {
        foreach (var layer in component.PartLayers)
        {
            if (spriteComp.LayerMapTryGet(layer, out var index))
                spriteComp.RemoveLayer(index);
        }
        component.PartLayers.Clear();

        foreach (var part in component.Parts)
        {
            foreach (var layer in part.Sprites)
            {
                if (!IsHidden(component, layer.Key))
                {
                    if (!spriteComp.LayerMapTryGet(layer.Key, out var baseIndex))
                        continue;

                    var index = baseIndex;

                    for (var i = 0; i < layer.Value.Count; i++)
                    {
                        spriteComp.AddBlankLayer(index);
                        var layerId = $"{layer.Key}-part-{index}";
                        while (spriteComp.LayerMapTryGet(layerId, out var _))
                        {
                            index++;
                            layerId = $"{layer.Key}-part-{index}";
                        }
                        spriteComp.LayerMapSet(layerId, index);
                        component.PartLayers.Add(layerId);

                        var colour = Color.White;
                        if (i < part.Colours.Count)
                            colour = part.Colours[i];

                        if (layer.Value[i] != null)
                            SetPartVisual(spriteComp, index, layer.Value[i]!, colour);

                        index++;
                    }
                }
            }
        }
    }

    private void SetPartVisual(SpriteComponent spriteComp, int index, SpriteSpecifier sprite, Color colour)
    {
        spriteComp.LayerSetColor(index, colour);
        spriteComp.LayerSetSprite(index, sprite);
    }

    public override void SetSkinColor(EntityUid uid, Color skinColor, bool sync = true, bool verify = true, HumanoidAppearanceComponent? humanoid = null)
    {
        if (!Resolve(uid, ref humanoid) || humanoid.SkinColor == skinColor)
            return;

        base.SetSkinColor(uid, skinColor, false, verify, humanoid);

        if (!TryComp(uid, out SpriteComponent? sprite))
            return;
    }

    protected override void SetLayerVisibility(
        EntityUid uid,
        HumanoidAppearanceComponent humanoid,
        string layer,
        bool visible,
        bool permanent,
        ref bool dirty)
    {
    }
}
