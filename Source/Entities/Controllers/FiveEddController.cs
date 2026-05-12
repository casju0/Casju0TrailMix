using System;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.Casju0TrailMix.Entities;

[CustomEntity("Casju0TrailMix/FiveEddController")]
public class FiveEddController(EntityData data, Vector2 offset) : Entity(data.Position + offset)
{
    public const string flag = "Casju0TrailMix/fiveEddEnabled";
    private static Hook superWallJumpAngleCheckHook;

    private readonly bool enabled = data.Bool("enabled", true);
    private readonly bool persistent = data.Bool("persistent", true);

    public override void Added(Scene scene)
    {
        (scene as Level).Session.SetFlag(flag, enabled);
    }

    public override void Removed(Scene scene)
    {
        if (!persistent)
        {
            (scene as Level).Session.SetFlag(flag, false);
        }
    }

    public static void Load()
    {
        On.Celeste.Input.GetAimVector += HandleGetAimVector;
        On.Celeste.Player.DashBegin += HandleDashBegin;
        superWallJumpAngleCheckHook = new Hook(
            typeof(Player).GetProperty("SuperWallJumpAngleCheck", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).GetGetMethod(true),
            typeof(FiveEddController).GetMethod("modSuperWallJumpAngleCheck", BindingFlags.NonPublic | BindingFlags.Static)
        );
        On.Celeste.Player.SuperWallJump += HandleSuperWallJump;
        On.Celeste.DeathEffect.Draw += HandleDeathEffect5edd;
    }

    public static void Unload()
    {
        On.Celeste.Input.GetAimVector -= HandleGetAimVector;
        On.Celeste.Player.DashBegin -= HandleDashBegin;
        superWallJumpAngleCheckHook?.Dispose();
        On.Celeste.Player.SuperWallJump -= HandleSuperWallJump;
        On.Celeste.DeathEffect.Draw -= HandleDeathEffect5edd;
    }

    public static Vector2 HandleGetAimVector(On.Celeste.Input.orig_GetAimVector orig, Facings defaultFacing)
    {
        if ((Engine.Scene as Level).Session.GetFlag(flag))
        {
            var result = orig(defaultFacing);
            var downAngle = (float)Math.Tau * 0.25f;
            if (result.X == 0 && result.Y == 1)
            {
                // dash angle for down remains the same
                return result;
            }
            else if (result.X != 0 && result.Y >= 0)
            {
                // down-horizontal and horizontal inputs share the same dash angle
                var angle = downAngle + (float)Math.Tau * -0.2 * Math.Sign(result.X);
                return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            }
            else if (result.X != 0 && result.Y < 0)
            {
                // dash angle for up-horizontal input
                var angle = downAngle + (float)Math.Tau * -0.4 * Math.Sign(result.X);
                return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            }
            else
            {
                // dash angle for up input depends on which way the player is facing
                var angle = downAngle + (float)Math.Tau * -0.4 * (int)defaultFacing;
                return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            }
        }
        else
        {
            return orig(defaultFacing);
        }
    }

    public static void HandleDashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);
        if (self.SceneAs<Level>().Session.GetFlag(flag) && !self.Ducking && Input.MoveY.Value >= 0)
        {
            self.Ducking = true;
        }
    }

    private static bool modSuperWallJumpAngleCheck(Func<Player, bool> orig, Player self)
    {
        return self.SceneAs<Level>().Session.GetFlag(flag) || orig(self);
    }

    public static void HandleSuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir)
    {
        if (self.SceneAs<Level>().Session.GetFlag(flag) && self.Speed.Y >= 0)
        {
            self.Ducking = true;
            self.Facing = (Facings)dir;
            self.SuperJump();
        }
        else
        {
            orig(self, dir);
        }
    }

    private static void HandleDeathEffect5edd(On.Celeste.DeathEffect.orig_Draw orig, Vector2 position, Color color, float ease)
    {
        if ((Engine.Scene as Level).Session.GetFlag(flag))
        {
            Color color2 = Math.Floor(ease * 10f) % 2.0 == 0.0 ? color : Color.White;
            MTexture mTexture = GFX.Game["characters/player/hair00"];
            float num = ease < 0.5f ? 0.5f + ease : Ease.CubeOut(1f - (ease - 0.5f) * 2f);
            for (int i = 0; i < 5; i++)
            {
                Vector2 vector = Calc.AngleToVector((i / 5f + ease * 0.25f) * (MathF.PI * 2f), Ease.CubeOut(ease) * 24f);
                mTexture.DrawCentered(position + vector + new Vector2(-1f, 0f), Color.Black, new Vector2(num, num));
                mTexture.DrawCentered(position + vector + new Vector2(1f, 0f), Color.Black, new Vector2(num, num));
                mTexture.DrawCentered(position + vector + new Vector2(0f, -1f), Color.Black, new Vector2(num, num));
                mTexture.DrawCentered(position + vector + new Vector2(0f, 1f), Color.Black, new Vector2(num, num));
            }
            for (int j = 0; j < 5; j++)
            {
                Vector2 vector2 = Calc.AngleToVector((j / 5f + ease * 0.25f) * (MathF.PI * 2f), Ease.CubeOut(ease) * 24f);
                mTexture.DrawCentered(position + vector2, color2, new Vector2(num, num));
            }
        }
        else
        {
            orig(position, color, ease);
        }
    }
}