namespace Content.Shared.Thanatophobia.ShipGuns;

[RegisterComponent]
public sealed partial class ShipGunComponent : Component
{
    public Angle PointTo;
    public Angle MaxRotSpeed = 2.5f;
    public bool IsShooting;
    public bool IsRotating;
}
