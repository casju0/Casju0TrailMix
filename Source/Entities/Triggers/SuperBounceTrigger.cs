namespace Celeste.Mod.Casju0TrailMix.Entities;

[CustomEntity("Casju0TrailMix/SuperBounceTrigger")]
class SuperBounceTrigger : Trigger
{
    static SuperBounceSubMenu Settings { get => Casju0TrailMixModule.Settings.SuperBounceSettings; }

    bool enabled;

    private bool dontRecoverDash = true;
    private float superBounceXBoost = 0f;
    private float superBounceMultiplier = 1.5f;
    private float lowBounceXBoost = 40f;
    private float lowBounceMultiplier = 0.6f;

    public SuperBounceTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        enabled = data.Bool("enabled", true);
        dontRecoverDash = data.Bool("dontRecoverDash", true);
        superBounceXBoost = data.Float("superBounceXBoost", 0f);
        superBounceMultiplier = data.Float("superBounceMultiplier", 1.5f);
        lowBounceXBoost = data.Float("lowBounceXBoost", 40f);
        lowBounceMultiplier = data.Float("lowBounceMultiplier", 0.6f);
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
        SceneAs<Level>().Session.SetFlag(SuperBounceController.flag, enabled);
        Settings.DontRecoverDash = dontRecoverDash;
        Settings.SuperBounceXBoost = superBounceXBoost;
        Settings.SuperBounceMultiplier = superBounceMultiplier;
        Settings.LowBounceXBoost = lowBounceXBoost;
        Settings.LowBounceMultiplier = lowBounceMultiplier;
    }
}
