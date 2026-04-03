namespace Celeste.Mod.Casju0TrailMix.Entities;

[Tracked]
[CustomEntity("Casju0TrailMix/Smw1f0Settings")]
public class Smw1f0Settings : Entity
{
    static Smw1f0SubMenu Settings { get => Casju0TrailMixModule.Settings.Smw1f0Settings; }

    public Smw1f0Settings(EntityData data, Vector2 position)
    {
        Settings.FallSpeed = data.Float("fallSpeed", 250f);
    }
}
