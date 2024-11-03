namespace Content.Server.Thanatophobia.CargoDefense;

/// <summary>
/// The overall system in charge of the "Cargo Defense" gamemode.
/// Cargo defense is a gamemode where members on a space station will fight off waves of incoming attackers.
/// Attacks may come from space or from the station itself.
/// At the end of each wave, there will be a chance to purchase important materials, support items and ship guns.
/// If the cargo is destroyed, the players lose.
/// At most, we probably want waves to go only up to 50.
/// </summary>
public sealed partial class CargoDefenseSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
    }
}
