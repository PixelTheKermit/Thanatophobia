namespace Content.Shared.Thanatophobia.Traits;

[RegisterComponent]
public sealed partial class SizeAdjustmentTraitComponent : Component
{
    [DataField(required: true)]
    public float Height;
}
