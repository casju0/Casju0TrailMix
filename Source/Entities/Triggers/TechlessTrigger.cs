namespace Celeste.Mod.Casju0TrailMix.Entities;

[CustomEntity("Casju0TrailMix/TechlessTrigger")]
class TechlessTrigger : Trigger
{
    static TechlessSubMenu Settings { get => Casju0TrailMixModule.Settings.TechlessSettings; }

    bool enabled;

    private bool enableHypers;
    private bool enableSupers;
    private float dashJumpSpeedLimit;
    private bool enableWallBounces;
    private bool nerfedWallBoosts;
    private float nerfedWallBoostMoveDuration;
    private int staminaLimit;

    public TechlessTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        enabled = data.Bool("enabled", true);
        enableHypers = data.Bool("enableHypers", true);
        enableSupers = data.Bool("enableSupers", true);
        dashJumpSpeedLimit = data.Float("dashJumpSpeedLimit", -1f);
        enableWallBounces = data.Bool("enableWallBounces", true);
        nerfedWallBoosts = data.Bool("nerfedWallBoosts", false);
        nerfedWallBoostMoveDuration = data.Float("nerfedWallBoostMoveDuration", 0.16f);
        staminaLimit = data.Int("staminaLimit", -1);
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
        SceneAs<Level>().Session.SetFlag(TechlessController.flag, enabled);
        Settings.Hypers = enableHypers;
        Settings.Supers = enableSupers;
        Settings.DashJumpSpeedLimit = dashJumpSpeedLimit;
        Settings.WallBounces = enableWallBounces;
        Settings.NerfedWallBoosts = nerfedWallBoosts;
        Settings.NerfedWallBoostMoveDuration = nerfedWallBoostMoveDuration;
        Settings.StaminaLimit = staminaLimit;
    }
}
