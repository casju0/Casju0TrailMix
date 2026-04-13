using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.Casju0TrailMix;

public class Casju0TrailMixModuleSettings : EverestModuleSettings
{
    [SettingNumberInput]
    public GrappleSubMenu GrappleSettings { get; set; } = new();
    public SuperBounceSubMenu SuperBounceSettings { get; set; } = new();
    public TechlessSubMenu TechlessSettings { get; set; } = new();
    public Smw1f0SubMenu Smw1f0Settings { get; set; } = new();
    public ThrowBlockSubMenu ThrowBlockSettings { get; set; } = new();
    public WaterPhysicsSubMenu WaterPhysicsSettings { get; set; } = new();
    [DefaultButtonBinding(button: Buttons.LeftTrigger, key: Keys.LeftShift)]
    public ButtonBinding ShootGrapple { get; set; }
}

[SettingSubMenu]
public class GrappleSubMenu
{
    public enum InventoryTypes
    {
        RequiresRefill,
        AlwaysEquipped
    }
    public InventoryTypes InventoryType { get; set; } = InventoryTypes.RequiresRefill;
    public enum ControlTypes
    {
        ReplaceGrab,
        ReplaceDash,
        UserBinding
    }
    public ControlTypes ControlType { get; set; } = ControlTypes.ReplaceGrab;
    public bool HarpoonMode { get; set; } = false;
    public bool CanShootWhenTired { get; set; } = false;
    public bool AddMomentumToShot { get; set; } = true;
    public enum PlayerReleaseMomentums
    {
        RetainGrappleSpeed,
        RevertSpeed
    }
    public PlayerReleaseMomentums PlayerReleaseMomentum = PlayerReleaseMomentums.RetainGrappleSpeed;
    public float PlayerReleaseMomentumSpeedCap = -1f;
    public float PlayerReleaseSpeedBoost = 40f;
    public enum FreezeFrameLengths
    {
        None,
        Short,
        Long,
        VeryLong
    }
    public FreezeFrameLengths FreezeFrameLength { get; set; } = FreezeFrameLengths.Long;
    public FreezeFrameLengths ContactPauseLength { get; set; } = FreezeFrameLengths.None;
    public enum Shockwaves
    {
        None,
        Small,
        Large
    }
    public Shockwaves Shockwave { get; set; } = Shockwaves.Small;
    public Color StringColor { get; set; } = Calc.HexToColor("FFFFFF");
    public Color ContactColor { get; set; } = Calc.HexToColor("FFB6C1");
    public Color ReelColor { get; set; } = Calc.HexToColor("ADD8E6");
    public bool CanWallCancel { get; set; } = true;
    public bool CanItemCancel { get; set; } = true;
    public bool CanWallJumpCancel { get; set; } = true;
    public bool CanItemJumpCancel { get; set; } = true;
    [SettingNumberInput]
    public float JumpCancelStaminaCost { get; set; } = 27.5f;
    public bool CanWallDashCancel { get; set; } = true;
    public bool CanItemDashCancel { get; set; } = true;
    public enum PufferBehaviors
    {
        Ignore,
        PullPuffer,
        PullPlayer
    }
    public PufferBehaviors PufferBehavior { get; set; } = PufferBehaviors.PullPuffer;
    [SettingRange(-5, 5)]
    public int YOffset { get; set; } = -2;
    [SettingNumberInput]
    public float ShootSpeed { get; set; } = 360f;
    [SettingNumberInput]
    public float RetractSpeed { get; set; } = 480f;
    [SettingNumberInput]
    public float WallPullSpeed { get; set; } = 240f;
    [SettingNumberInput]
    public float ItemPullSpeed { get; set; } = 240f;
    [SettingNumberInput]
    public float MinShootDuration { get; set; } = 0.1f;
    [SettingNumberInput]
    public float MaxShootDuration { get; set; } = 0.25f;
    [SettingNumberInput]
    public float Cooldown { get; set; } = 0f;
}

[SettingSubMenu]
public class SuperBounceSubMenu
{
    public bool DontRecoverDash { get; set; } = true;
    [SettingNumberInput]
    public float SuperBounceXBoost { get; set; } = 0f;
    [SettingNumberInput]
    public float SuperBounceMultiplier { get; set; } = 1.5f;
    [SettingNumberInput]
    public float LowBounceXBoost { get; set; } = 40f;
    [SettingNumberInput]
    public float LowBounceMultiplier { get; set; } = 0.6f;
}

[SettingSubMenu]
public class TechlessSubMenu
{
    public bool Hypers { get; set; } = true;
    public bool Supers { get; set; } = true;
    [SettingNumberInput]
    public float DashJumpSpeedLimit { get; set; } = -1f;
    public bool WallBounces { get; set; } = true;
    public bool NerfedWallBoosts { get; set; } = false;
    [SettingNumberInput]
    public float NerfedWallBoostMoveDuration { get; set; } = 0.16f;
    [SettingNumberInput]
    public int StaminaLimit { get; set; } = -1;
    public enum PufferModes
    {
        Vanilla,
        NoBoost,
        AlwaysBoost
    }
    public PufferModes PufferBoostMode { get; set; } = PufferModes.Vanilla;
}

[SettingSubMenu]
public class Smw1f0SubMenu
{
    [SettingNumberInput]
    public float FallSpeed { get; set; } = 250f;
}

[SettingSubMenu]
public class ThrowBlockSubMenu
{
    public bool UseSmwHoldables { get; set; } = true;
    public bool AllowDashPickups { get; set; } = true;
    public bool AllowClimbPickups { get; set; } = true;
    public bool AllowWallJumpPickups { get; set; } = true;
    [SettingNumberInput]
    public float ClimbJumpRefundAmount { get; set; } = 0f;
    [SettingNumberInput]
    public float GraceJumpDuration { get; set; } = 0.1f;
}

[SettingSubMenu]
public class WaterPhysicsSubMenu
{
    public enum SwimModes
    {
        MegaMan,
        SMW
    }
    public SwimModes SwimMode { get; set; } = SwimModes.MegaMan;
    [SettingNumberInput]
    public float MMRiseSpeed { get; set; } = -105f;
    [SettingNumberInput]
    public float MMRiseAcceleration { get; set; } = 105f;
    [SettingNumberInput]
    public float SmwHorizontalSpeed { get; set; } = 90f;
    [SettingNumberInput]
    public float SmwHorizontalAccel { get; set; } = 400f;
    [SettingNumberInput]
    public float SmwFallMaxSpeed { get; set; } = 120f;
    [SettingNumberInput]
    public float SmwFallAccel { get; set; } = 120f;
    [SettingNumberInput]
    public float SmwRiseCancelDecel { get; set; } = 240f;
    [SettingNumberInput]
    public float SmwPaddleSpeed { get; set; } = 120f;
    [SettingNumberInput]
    public float SmwPaddleMaxSpeed { get; set; } = -120f;
    [SettingNumberInput]
    public float SmwSuperPaddleSpeed { get; set; } = 240f;
    [SettingNumberInput]
    public float SmwSuperPaddleMaxSpeed { get; set; } = -160f;
}
