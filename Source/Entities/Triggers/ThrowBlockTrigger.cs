namespace Celeste.Mod.Casju0TrailMix.Entities;

[CustomEntity("Casju0TrailMix/ThrowBlockTrigger")]
class ThrowBlockTrigger : Trigger
{
    static ThrowBlockSubMenu Settings { get => Casju0TrailMixModule.Settings.ThrowBlockSettings; }

    private bool allowDashPickups;
    private bool allowClimbPickups;
    private bool allowWallJumpPickups;
    private float climbJumpRefundAmount;
    private float graceJumpDuration;

    public ThrowBlockTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        allowDashPickups = data.Bool("allowDashPickups", true);
        allowClimbPickups = data.Bool("allowClimbPickups", true);
        allowWallJumpPickups = data.Bool("allowWallJumpPickups", true);
        climbJumpRefundAmount = data.Float("climbJumpRefundAmount", 0.0f);
        graceJumpDuration = data.Float("graceJumpDuration", 0.1f);
        if (data.Bool("coverRoom", false))
        {
            ApplyChanges();
        }
    }

    public override void OnEnter(Player player)
    {
        ApplyChanges();
    }

    public void ApplyChanges()
    {
        Settings.AllowDashPickups = allowDashPickups;
        Settings.AllowClimbPickups = allowClimbPickups;
        Settings.AllowWallJumpPickups = allowWallJumpPickups;
        Settings.ClimbJumpRefundAmount = climbJumpRefundAmount;
        Settings.GraceJumpDuration = graceJumpDuration;
    }
}
