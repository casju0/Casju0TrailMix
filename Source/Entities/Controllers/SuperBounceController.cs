namespace Celeste.Mod.Casju0TrailMix.Entities;

[CustomEntity("Casju0TrailMix/SuperBounceController")]
public class SuperBounceController : Entity
{
    public const string flag = "Casju0TrailMix/superBounceEnabled";
    static SuperBounceSubMenu Settings { get => Casju0TrailMixModule.Settings.SuperBounceSettings; }

    bool enabled;

    public SuperBounceController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        enabled = data.Bool("enabled", true);
        Settings.DontRecoverDash = data.Bool("dontRecoverDash", true);
        Settings.SuperBounceXBoost = data.Float("superBounceXBoost", 0f);
        Settings.SuperBounceMultiplier = data.Float("superBounceMultiplier", 1.5f);
        Settings.LowBounceXBoost = data.Float("lowBounceXBoost", 40f);
        Settings.LowBounceMultiplier = data.Float("lowBounceMultiplier", 0.6f);
    }

    public override void Added(Scene scene)
    {
        (scene as Level).Session.SetFlag(flag, enabled);
    }

    public static void Load()
    {
        On.Celeste.Player.Bounce += HandleBounce;
    }

    public static void Unload()
    {
        On.Celeste.Player.Bounce -= HandleBounce;
    }

    private static void HandleBounce(
        On.Celeste.Player.orig_Bounce orig,
        Player self,
        float fromY
    )
    {
        if (!self.SceneAs<Level>().Session.GetFlag(flag))
        {
            orig(self, fromY);
            return;
        }

        var prevDashes = self.Dashes;
        orig(self, fromY);
        if (Settings.DontRecoverDash)
        {
            self.Dashes = prevDashes;
        }
        if (Input.Jump.Check)
        {
            self.Speed.X += Settings.SuperBounceXBoost * (float)self.moveX;
            self.varJumpSpeed = self.Speed.Y * Settings.SuperBounceMultiplier;
        }
        else
        {
            self.Speed.X += Settings.LowBounceXBoost * (float)self.moveX;
            self.varJumpSpeed = self.Speed.Y * Settings.LowBounceMultiplier;
        }
    }
}
