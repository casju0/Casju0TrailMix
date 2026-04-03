namespace Celeste.Mod.Casju0TrailMix.Entities;

[Tracked]
[CustomEntity("Casju0TrailMix/ThrowBlockSettings")]
public class ThrowBlockSettings : Entity
{
    static ThrowBlockSubMenu Settings { get => Casju0TrailMixModule.Settings.ThrowBlockSettings; }

    public ThrowBlockSettings(EntityData data, Vector2 position)
    {
        Settings.AllowDashPickups = data.Bool("allowDashPickups", true);
        Settings.AllowClimbPickups = data.Bool("allowClimbPickups", true);
        Settings.AllowWallJumpPickups = data.Bool("allowWallJumpPickups", true);
        Settings.ClimbJumpRefundAmount = data.Float("climbJumpRefundAmount", 0.0f);
        Settings.GraceJumpDuration = data.Float("graceJumpDuration", 0.1f);
    }
}
