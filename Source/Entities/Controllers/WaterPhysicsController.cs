using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.Casju0TrailMix.Entities;

[CustomEntity("Casju0TrailMix/WaterPhysicsController")]
public class WaterPhysicsController : Entity
{
    public const string flag = "Casju0TrailMix/waterPhysicsEnabled";
    static WaterPhysicsSubMenu Settings { get => Casju0TrailMixModule.Settings.WaterPhysicsSettings; }

    bool enabled;
    private static ILHook playerUpdateHook;

    public WaterPhysicsController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        enabled = data.Bool("enabled", true);
        Settings.SwimMode = data.Enum<WaterPhysicsSubMenu.SwimModes>("swimMode", WaterPhysicsSubMenu.SwimModes.MegaMan);
        Settings.MMRiseSpeed = data.Float("mmRiseSpeed", -105f);
        Settings.MMRiseAcceleration = data.Float("mmRiseAcceleration", 105f);
        Settings.SmwHorizontalSpeed = data.Float("smwHorizontalSpeed", 90f);
        Settings.SmwHorizontalAccel = data.Float("smwHorizontalAccel", 400f);
        Settings.SmwFallMaxSpeed = data.Float("smwFallMaxSpeed", 120f);
        Settings.SmwFallAccel = data.Float("smwFallAccel", 120f);
        Settings.SmwRiseCancelDecel = data.Float("smwRiseCancelDecel", 240f);
        Settings.SmwPaddleSpeed = data.Float("smwPaddleSpeed", 120f);
        Settings.SmwPaddleMaxSpeed = data.Float("smwPaddleMaxSpeed", -120f);
        Settings.SmwSuperPaddleSpeed = data.Float("smwSuperPaddleSpeed", 240f);
        Settings.SmwSuperPaddleMaxSpeed = data.Float("smwSuperPaddleMaxSpeed", -160f);
    }

    public override void Added(Scene scene)
    {
        (scene as Level).Session.SetFlag(flag, enabled);
    }

    public static void Load()
    {
        playerUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update"), HandleUpdate);
        IL.Celeste.Player.NormalUpdate += HandleNormalUpdate;
        On.Celeste.Player.DashUpdate += HandleDashUpdate;
        On.Celeste.Player.SwimUpdate += CustomSwimming;
    }

    public static void Unload()
    {
        if (playerUpdateHook != null) { playerUpdateHook.Dispose(); }
        IL.Celeste.Player.NormalUpdate -= HandleNormalUpdate;
        On.Celeste.Player.DashUpdate -= HandleDashUpdate;
        On.Celeste.Player.SwimUpdate -= CustomSwimming;
    }

    private static void HandleUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        int state = -1;
        ILLabel label = null;
        // before: if (self.StateMachine.State == 3 && ...)
        // after:  if (self.StateMachine.State == 3 && !ShouldDisableVanillaWaterJump() && ...)
        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(out state), instr => instr.MatchBneUn(out label)))
        {
            if (state == Player.StSwim)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(ShouldDisableVanillaWaterJump);
                cursor.EmitBrtrue(label);
            }
        }
    }

    private static void HandleNormalUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        ILLabel ilLabel = null;
        while (cursor.TryGotoNext(MoveType.After, instr =>
        {
            int x;
            instr.MatchStloc(out x);
            return x == 13;
        }, instr => instr.MatchBrfalse(out ilLabel)))
        {
            // before: if ((water = CollideFirst<Water>(Position + Vector2.UnitY * 2f)) != null)
            // after:  if ((water = CollideFirst<Water>(Position + Vector2.UnitY * 2f)) != null &&
            //             !ShouldDisableVanillaWaterJump())
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(ShouldDisableVanillaWaterJump);
            cursor.EmitBrtrue(ilLabel);
        }
    }

    private static int HandleDashUpdate(On.Celeste.Player.orig_DashUpdate orig, Player self)
    {
        if (!self.SceneAs<Level>().Session.GetFlag(flag))
        {
            return orig(self);
        }
        else
        {
            var origJumps = SaveData.Instance.TotalJumps;
            var origWallJumps = SaveData.Instance.TotalWallJumps;
            var result = orig(self);
            if (origJumps == SaveData.Instance.TotalJumps && origWallJumps == SaveData.Instance.TotalWallJumps)
            {
                self.varJumpTimer = 0f;
            }
            return result;
        }
    }

    private static int CustomSwimming(On.Celeste.Player.orig_SwimUpdate orig, Player self)
    {
        if (!self.SceneAs<Level>().Session.GetFlag(flag))
        {
            return orig(self);
        }
        var insideWater = self.CollideCheck<Water>();
        switch (Settings.SwimMode)
        {
            case WaterPhysicsSubMenu.SwimModes.MegaMan:
                if (!insideWater)
                {
                    self.varJumpTimer = 0f;
                    return Player.StNormal;
                }

                if (insideWater && self.OnGround() && Input.Jump.Pressed)
                {
                    self.Jump();
                }
                else if (insideWater && Input.Jump.Released)
                {
                    self.varJumpTimer = 0f;
                }

                if (self.varJumpTimer > 0 && insideWater)
                {
                    self.varJumpTimer = 69420f;
                    self.Speed.Y = Calc.Approach(self.Speed.Y, Settings.MMRiseSpeed, Settings.MMRiseAcceleration * Engine.DeltaTime);
                    self.varJumpSpeed = self.Speed.Y;
                }

                var normalUpdate = self.NormalUpdate();

                return normalUpdate == Player.StNormal ? Player.StSwim : normalUpdate;
            case WaterPhysicsSubMenu.SwimModes.SMW:
                if (!insideWater)
                {
                    return Player.StNormal;
                }
                if (self.CanDash)
                {
                    return self.StartDash();
                }
                Vector2 aimVector = Input.GetAimVector();
                self.Speed.X = Calc.Approach(self.Speed.X, Settings.SmwHorizontalSpeed * (float)self.moveX, (float)(Settings.SmwHorizontalAccel * (double)Engine.DeltaTime));
                self.Speed.Y = Calc.Approach(self.Speed.Y, Settings.SmwFallMaxSpeed, Settings.SmwFallAccel * Engine.DeltaTime);
                if (aimVector.Y > 0 && self.Speed.Y < 0)
                {
                    self.Speed.Y = Calc.Approach(self.Speed.Y, Settings.SmwFallMaxSpeed, Settings.SmwRiseCancelDecel * Engine.DeltaTime);
                }
                if (Input.Jump.Pressed)
                {
                    Input.Jump.ConsumePress();
                    float num = aimVector.Y < 0 ? Settings.SmwSuperPaddleSpeed : Settings.SmwPaddleSpeed;
                    float num2 = aimVector.Y < 0 ? Settings.SmwSuperPaddleMaxSpeed : Settings.SmwPaddleMaxSpeed;
                    self.Speed.Y = Math.Max(self.Speed.Y - num, num2);
                    Audio.Play("event:/casju0_TrailMix/smw_swim");
                }
                return Player.StSwim;
            default:
                return orig(self);
        }
    }

    private static bool ShouldDisableVanillaWaterJump(Player self)
    {
        return self.SceneAs<Level>().Session.GetFlag(flag);
    }
}
