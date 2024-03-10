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

        foreach (var layer in component.Parts)
        {
            if (spriteComp.LayerMapTryGet(layer, out var index))
                spriteComp[index].Visible = false;
        }

        foreach (var layer in component.PartLayers)
        {
            if (spriteComp.LayerMapTryGet(layer, out var index))
                spriteComp.RemoveLayer(index);
        }

        component.PartLayers.Clear();

        foreach (var (part, visualisers) in component.Parts)
        {
            if (!spriteComp.LayerMapTryGet(part, out var baseIndex))
                continue;

            spriteComp[baseIndex].Visible = true;

            if (!IsHidden(component, part))
            {
                var index = baseIndex;
                foreach (var sprite in visualisers)
                {
                    spriteComp.AddBlankLayer(index);
                    var layerId = $"{part}-part-{index}";
                    spriteComp.LayerMapSet(layerId, index);
                    component.PartLayers.Add(layerId);
                    SetPartVisual(spriteComp, index, sprite);

                    index += 1;
                }
            }
        }
    }

    private void SetPartVisual(SpriteComponent spriteComp, int index, BodyPartVisualiserSprite sprite)
    {
        if (sprite.Sprite == null)
            return;

        if (sprite.Colour != null)
            spriteComp.LayerSetColor(index, sprite.Colour.Value);

        spriteComp.LayerSetSprite(index, sprite.Sprite);
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
