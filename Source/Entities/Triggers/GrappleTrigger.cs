namespace Celeste.Mod.Casju0TrailMix.Entities;

[CustomEntity("Casju0TrailMix/GrappleTrigger")]
public class GrappleTrigger : Trigger
{
    static GrappleSubMenu Settings { get => Casju0TrailMixModule.Settings.GrappleSettings; }

    bool enabled;

    private GrappleSubMenu.InventoryTypes inventoryType;
    private GrappleSubMenu.ControlTypes controlType;
    private bool harpoonMode;
    private bool canShootWhenTired;
    private bool addMomentumToShot;
    private GrappleSubMenu.PlayerReleaseMomentums playerReleaseMomentum;
    private float playerReleaseMomentumSpeedCap;
    private float playerReleaseSpeedBoost;
    private GrappleSubMenu.FreezeFrameLengths freezeFrameLength;
    private GrappleSubMenu.FreezeFrameLengths contactPauseLength;
    private GrappleSubMenu.Shockwaves shockwave;
    private Color stringColor;
    private Color contactColor;
    private Color reelColor;
    private bool canWallCancel;
    private bool canItemCancel;
    private bool canWallJumpCancel;
    private bool canItemJumpCancel;
    private float jumpCancelStaminaCost;
    private bool canWallDashCancel;
    private bool canItemDashCancel;
    private GrappleSubMenu.PufferBehaviors pufferBehavior;
    private int yOffset;
    private float shootSpeed;
    private float retractSpeed;
    private float wallPullSpeed;
    private float itemPullSpeed;
    private float minShootDuration;
    private float maxShootDuration;
    private float cooldown;

    public GrappleTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        enabled = data.Bool("enabled", true);
        inventoryType = data.Enum<GrappleSubMenu.InventoryTypes>("inventoryType", GrappleSubMenu.InventoryTypes.AlwaysEquipped);
        controlType = data.Enum<GrappleSubMenu.ControlTypes>("controlType", GrappleSubMenu.ControlTypes.ReplaceGrab);
        harpoonMode = data.Bool("harpoonMode", false);
        canShootWhenTired = data.Bool("canShootWhenTired", false);
        addMomentumToShot = data.Bool("addMomentumToShot", true);
        playerReleaseMomentum = data.Enum<GrappleSubMenu.PlayerReleaseMomentums>("playerReleaseMomentum", GrappleSubMenu.PlayerReleaseMomentums.RetainGrappleSpeed);
        playerReleaseMomentumSpeedCap = data.Float("playerReleaseMomentumSpeedCap", -1f);
        playerReleaseSpeedBoost = data.Float("playerReleaseSpeedBoost", 40f);
        freezeFrameLength = data.Enum<GrappleSubMenu.FreezeFrameLengths>("freezeFrameLength", GrappleSubMenu.FreezeFrameLengths.Long);
        contactPauseLength = data.Enum<GrappleSubMenu.FreezeFrameLengths>("contactPauseLength", GrappleSubMenu.FreezeFrameLengths.None);
        shockwave = data.Enum<GrappleSubMenu.Shockwaves>("shockwave", GrappleSubMenu.Shockwaves.Small);
        stringColor = data.HexColor("stringColor", Calc.HexToColor("FFFFFF"));
        contactColor = data.HexColor("contactColor", Calc.HexToColor("FFB6C1"));
        reelColor = data.HexColor("reelColor", Calc.HexToColor("ADD8E6"));
        canWallCancel = data.Bool("canWallCancel", true);
        canItemCancel = data.Bool("canItemCancel", true);
        canWallJumpCancel = data.Bool("canWallJumpCancel", true);
        canItemJumpCancel = data.Bool("canItemJumpCancel", true);
        jumpCancelStaminaCost = data.Float("jumpCancelStaminaCost", 27.5f);
        canWallDashCancel = data.Bool("canWallDashCancel", true);
        canItemDashCancel = data.Bool("canItemDashCancel", true);
        pufferBehavior = data.Enum<GrappleSubMenu.PufferBehaviors>("pufferBehavior", GrappleSubMenu.PufferBehaviors.PullPuffer);
        yOffset = data.Int("yOffset", -2);
        shootSpeed = data.Float("shootSpeed", 360f);
        retractSpeed = data.Float("retractSpeed", 480f);
        wallPullSpeed = data.Float("wallPullSpeed", 240f);
        itemPullSpeed = data.Float("itemPullSpeed", 240f);
        minShootDuration = data.Float("minShootDuration", 0.1f);
        maxShootDuration = data.Float("maxShootDuration", 0.25f);
        cooldown = data.Float("cooldown", 0f);
        if (data.Bool("coverRoom", false))
        {
            ApplyChanges();
        }
    }

    public override void OnEnter(Player player)
    {
        ApplyChanges();
    }

    private void ApplyChanges()
    {
        SceneAs<Level>().Session.SetFlag(GrappleController.flag, enabled);
        Settings.InventoryType = inventoryType;
        Settings.ControlType = controlType;
        Settings.HarpoonMode = harpoonMode;
        Settings.CanShootWhenTired = canShootWhenTired;
        Settings.AddMomentumToShot = addMomentumToShot;
        Settings.PlayerReleaseMomentum = playerReleaseMomentum;
        Settings.PlayerReleaseMomentumSpeedCap = playerReleaseMomentumSpeedCap;
        Settings.PlayerReleaseSpeedBoost = playerReleaseSpeedBoost;
        Settings.FreezeFrameLength = freezeFrameLength;
        Settings.ContactPauseLength = contactPauseLength;
        Settings.Shockwave = shockwave;
        Settings.StringColor = stringColor;
        Settings.ContactColor = contactColor;
        Settings.ReelColor = reelColor;
        Settings.CanWallCancel = canWallCancel;
        Settings.CanItemCancel = canItemCancel;
        Settings.CanWallJumpCancel = canWallJumpCancel;
        Settings.CanItemJumpCancel = canItemJumpCancel;
        Settings.JumpCancelStaminaCost = jumpCancelStaminaCost;
        Settings.CanWallDashCancel = canWallDashCancel;
        Settings.CanItemDashCancel = canItemDashCancel;
        Settings.PufferBehavior = pufferBehavior;
        Settings.YOffset = yOffset;
        Settings.ShootSpeed = shootSpeed;
        Settings.RetractSpeed = retractSpeed;
        Settings.WallPullSpeed = wallPullSpeed;
        Settings.ItemPullSpeed = itemPullSpeed;
        Settings.MinShootDuration = minShootDuration;
        Settings.MaxShootDuration = maxShootDuration;
        Settings.Cooldown = cooldown;
    }
}
