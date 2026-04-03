using System;

namespace Celeste.Mod.Casju0TrailMix.Entities;

[CustomEntity("Casju0TrailMix/TechlessController")]
public class TechlessController : Entity
{
    public const string flag = "Casju0TrailMix/techlessEnabled";
    static TechlessSubMenu Settings { get => Casju0TrailMixModule.Settings.TechlessSettings; }

    bool enabled;

    public enum PufferModes
    {
        Vanilla,
        NoBoosts,
        AlwaysBoost
    }
    public static PufferModes PufferMode = PufferModes.Vanilla;

    public TechlessController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        enabled = data.Bool("enabled", true);
        Settings.Hypers = data.Bool("enableHypers", true);
        Settings.Supers = data.Bool("enableSupers", true);
        Settings.DashJumpSpeedLimit = data.Float("dashJumpSpeedLimit", -1f);
        Settings.WallBounces = data.Bool("enableWallBounces", true);
        Settings.NerfedWallBoosts = data.Bool("nerfedWallBoosts", false);
        Settings.NerfedWallBoostMoveDuration = data.Float("nerfedWallBoostMoveDuration", 0.16f);
        Settings.StaminaLimit = data.Int("staminaLimit", -1);
    }

    public override void Added(Scene scene)
    {
        (scene as Level).Session.SetFlag(flag, enabled);
    }

    public static void Load()
    {
        On.Celeste.Player.SuperJump += HandleSuperJump;
        On.Celeste.Player.SuperWallJump += HandleSuperWallJump;
        On.Celeste.Player.NormalUpdate += HandleNormalUpdate;
        On.Celeste.Player.ExplodeLaunch_Vector2_bool_bool += HandleExplodeLaunch;
    }

    public static void Unload()
    {
        On.Celeste.Player.SuperJump -= HandleSuperJump;
        On.Celeste.Player.SuperWallJump -= HandleSuperWallJump;
        On.Celeste.Player.NormalUpdate -= HandleNormalUpdate;
        On.Celeste.Player.ExplodeLaunch_Vector2_bool_bool -= HandleExplodeLaunch;
    }

    private static void HandleSuperJump(On.Celeste.Player.orig_SuperJump orig, Player self)
    {
        if (!self.SceneAs<Level>().Session.GetFlag(flag))
        {
            orig(self);
            return;
        }

        if (self.Ducking && !Settings.Hypers)
        {
            if (Settings.DashJumpSpeedLimit > 0)
            {
                self.Speed.X = Calc.Clamp(self.Speed.X, -Settings.DashJumpSpeedLimit, Settings.DashJumpSpeedLimit);
            }
            self.Jump();
        }
        else if (!self.Ducking && !Settings.Supers)
        {
            if (Settings.DashJumpSpeedLimit > 0)
            {
                self.Speed.X = Calc.Clamp(self.Speed.X, -Settings.DashJumpSpeedLimit, Settings.DashJumpSpeedLimit);
            }
            self.Jump();
        }
        else
        {
            orig(self);
        }
    }

    private static void HandleSuperWallJump(
        On.Celeste.Player.orig_SuperWallJump orig,
        Player self,
        int dir
    )
    {
        if (!self.SceneAs<Level>().Session.GetFlag(flag))
        {
            orig(self, dir);
            return;
        }

        if (!Settings.WallBounces)
        {
            self.WallJump(dir);
        }
        else
        {
            orig(self, dir);
        }
    }

    private static int HandleNormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self)
    {
        if (!self.SceneAs<Level>().Session.GetFlag(flag))
        {
            return orig(self);
        }

        if (Settings.StaminaLimit > 0 && self.Stamina > Settings.StaminaLimit)
        {
            self.Stamina = Settings.StaminaLimit;
        }
        if (Settings.NerfedWallBoosts && self.wallBoostTimer > 0f && self.moveX == self.wallBoostDir)
        {
            self.forceMoveX = self.wallBoostDir;
            self.forceMoveXTimer = Settings.NerfedWallBoostMoveDuration;
        }
        return orig(self);
    }

    private static Vector2 HandleExplodeLaunch(
        On.Celeste.Player.orig_ExplodeLaunch_Vector2_bool_bool orig,
        Player self,
        Vector2 from,
        bool snapUp,
        bool sidesOnly
    )
    {
        if (!self.SceneAs<Level>().Session.GetFlag(flag))
        {
            return orig(self, from, snapUp, sidesOnly);
        }

        if (PufferMode == PufferModes.Vanilla)
        {
            return orig(self, from, snapUp, sidesOnly);
        }
        else
        {
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Celeste.Freeze(0.1f);
            self.launchApproachX = null;
            Vector2 vector = (self.Center - from).SafeNormalize(-Vector2.UnitY);
            float num = Vector2.Dot(vector, Vector2.UnitY);
            if (snapUp && num <= -0.7f)
            {
                vector.X = 0f;
                vector.Y = -1f;
            }
            else if (num <= 0.65f && num >= -0.55f)
            {
                vector.Y = 0f;
                vector.X = Math.Sign(vector.X);
            }
            if (sidesOnly && vector.X != 0f)
            {
                vector.Y = 0f;
                vector.X = Math.Sign(vector.X);
            }
            self.Speed = 280f * vector;
            if (self.Speed.Y <= 50f)
            {
                self.Speed.Y = Math.Min(-150f, self.Speed.Y);
                self.AutoJump = true;
            }
            if (self.Speed.X != 0f)
            {
                if (PufferMode == PufferModes.AlwaysBoost)
                {
                    self.explodeLaunchBoostTimer = 0f;
                    self.Speed.X *= 1.2f;
                }
                else
                {
                    self.explodeLaunchBoostTimer = 0.01f;
                    self.explodeLaunchBoostSpeed = self.Speed.X * 1.2f;
                }
            }
            SlashFx.Burst(self.Center, self.Speed.Angle());
            if (!self.Inventory.NoRefills)
            {
                self.RefillDash();
            }
            self.RefillStamina();
            self.dashCooldownTimer = 0.2f;
            self.StateMachine.State = 7;
            return vector;
        }
    }
}
