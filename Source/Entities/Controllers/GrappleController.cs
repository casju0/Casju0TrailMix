namespace Celeste.Mod.Casju0TrailMix.Entities;

[Tracked]
[CustomEntity("Casju0TrailMix/GrappleController")]
public class GrappleController : Entity
{
    public const string flag = "Casju0TrailMix/grappleEnabled";
    static GrappleSubMenu Settings { get => Casju0TrailMixModule.Settings.GrappleSettings; }

    bool enabled;

    public GrappleController(EntityData data, Vector2 position)
    {
        enabled = data.Bool(flag, true);
        Settings.InventoryType = data.Enum<GrappleSubMenu.InventoryTypes>("inventoryType", GrappleSubMenu.InventoryTypes.AlwaysEquipped);
        Settings.ControlType = data.Enum<GrappleSubMenu.ControlTypes>("controlType", GrappleSubMenu.ControlTypes.ReplaceGrab);
        Settings.HarpoonMode = data.Bool("harpoonMode", false);
        Settings.CanShootWhenTired = data.Bool("canShootWhenTired", false);
        Settings.AddMomentumToShot = data.Bool("addMomentumToShot", true);
        Settings.PlayerReleaseMomentum = data.Enum<GrappleSubMenu.PlayerReleaseMomentums>("playerReleaseMomentum", GrappleSubMenu.PlayerReleaseMomentums.RetainGrappleSpeed);
        Settings.PlayerReleaseMomentumSpeedCap = data.Float("playerReleaseMomentumSpeedCap", -1f);
        Settings.PlayerReleaseSpeedBoost = data.Float("playerReleaseSpeedBoost", 40f);
        Settings.FreezeFrameLength = data.Enum<GrappleSubMenu.FreezeFrameLengths>("freezeFrameLength", GrappleSubMenu.FreezeFrameLengths.Long);
        Settings.ContactPauseLength = data.Enum<GrappleSubMenu.FreezeFrameLengths>("contactPauseLength", GrappleSubMenu.FreezeFrameLengths.None);
        Settings.Shockwave = data.Enum<GrappleSubMenu.Shockwaves>("shockwave", GrappleSubMenu.Shockwaves.Small);
        Settings.StringColor = data.HexColor("stringColor", Calc.HexToColor("FFFFFF"));
        Settings.ContactColor = data.HexColor("contactColor", Calc.HexToColor("FFB6C1"));
        Settings.ReelColor = data.HexColor("reelColor", Calc.HexToColor("ADD8E6"));
        Settings.CanWallCancel = data.Bool("canWallCancel", true);
        Settings.CanItemCancel = data.Bool("canItemCancel", true);
        Settings.CanWallJumpCancel = data.Bool("canWallJumpCancel", true);
        Settings.CanItemJumpCancel = data.Bool("canItemJumpCancel", true);
        Settings.JumpCancelStaminaCost = data.Float("jumpCancelStaminaCost", 27.5f);
        Settings.CanWallDashCancel = data.Bool("canWallDashCancel", true);
        Settings.CanItemDashCancel = data.Bool("canItemDashCancel", true);
        Settings.PufferBehavior = data.Enum<GrappleSubMenu.PufferBehaviors>("pufferBehavior", GrappleSubMenu.PufferBehaviors.PullPuffer);
        Settings.YOffset = data.Int("yOffset", -2);
        Settings.ShootSpeed = data.Float("shootSpeed", 360f);
        Settings.RetractSpeed = data.Float("retractSpeed", 480f);
        Settings.WallPullSpeed = data.Float("wallPullSpeed", 240f);
        Settings.ItemPullSpeed = data.Float("itemPullSpeed", 240f);
        Settings.MinShootDuration = data.Float("minShootDuration", 0.1f);
        Settings.MaxShootDuration = data.Float("maxShootDuration", 0.25f);
        Settings.Cooldown = data.Float("cooldown", 0f);
    }

    public override void Added(Scene scene)
    {
        (scene as Level).Session.SetFlag(flag, enabled);
    }
}
