namespace Celeste.Mod.Casju0TrailMix.Entities;

[CustomEntity("Casju0TrailMix/WaterPhysicsTrigger")]
class WaterPhysicsTrigger : Trigger
{
    static WaterPhysicsSubMenu Settings { get => Casju0TrailMixModule.Settings.WaterPhysicsSettings; }

    bool enabled;

    WaterPhysicsSubMenu.SwimModes swimMode;
    private float mmRiseSpeed;
    private float mmRiseAcceleration;
    private float smwHorizontalSpeed;
    private float smwHorizontalAccel;
    private float smwFallMaxSpeed;
    private float smwFallAccel;
    private float smwRiseCancelDecel;
    private float smwPaddleSpeed;
    private float smwPaddleMaxSpeed;
    private float smwSuperPaddleSpeed;
    private float smwSuperPaddleMaxSpeed;

    public WaterPhysicsTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        enabled = data.Bool("enabled", true);
        swimMode = data.Enum<WaterPhysicsSubMenu.SwimModes>("swimMode", WaterPhysicsSubMenu.SwimModes.MegaMan);
        mmRiseSpeed = data.Float("mmRiseSpeed", -105f);
        mmRiseAcceleration = data.Float("mmRiseAcceleration", 105f);
        smwHorizontalSpeed = data.Float("smwHorizontalSpeed", 90f);
        smwHorizontalAccel = data.Float("smwHorizontalAccel", 400f);
        smwFallMaxSpeed = data.Float("smwFallMaxSpeed", 120f);
        smwFallAccel = data.Float("smwFallAccel", 120f);
        smwRiseCancelDecel = data.Float("smwRiseCancelDecel", 160f);
        smwPaddleSpeed = data.Float("smwPaddleSpeed", 30f);
        smwPaddleMaxSpeed = data.Float("smwPaddleMaxSpeed", -120f);
        smwSuperPaddleSpeed = data.Float("smwSuperPaddleSpeed", 60f);
        smwSuperPaddleMaxSpeed = data.Float("smwSuperPaddleMaxSpeed", -160f);
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
        SceneAs<Level>().Session.SetFlag(WaterPhysicsController.flag, enabled);
        Settings.SwimMode = swimMode;
        Settings.MMRiseSpeed = mmRiseSpeed;
        Settings.MMRiseAcceleration = mmRiseAcceleration;
        Settings.SmwHorizontalSpeed = smwHorizontalSpeed;
        Settings.SmwHorizontalAccel = smwHorizontalAccel;
        Settings.SmwFallMaxSpeed = smwFallMaxSpeed;
        Settings.SmwFallAccel = smwFallAccel;
        Settings.SmwRiseCancelDecel = smwRiseCancelDecel;
        Settings.SmwPaddleSpeed = smwPaddleSpeed;
        Settings.SmwPaddleMaxSpeed = smwPaddleMaxSpeed;
        Settings.SmwSuperPaddleSpeed = smwSuperPaddleSpeed;
        Settings.SmwSuperPaddleMaxSpeed = smwSuperPaddleMaxSpeed;
    }
}
