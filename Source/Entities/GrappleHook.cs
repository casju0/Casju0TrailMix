using System;
using System.Reflection;
using Celeste.Mod.Casju0TrailMix.Components;
using FMOD.Studio;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.Casju0TrailMix.Entities;

[Tracked]
[CustomEntity("Casju0TrailMix/GrappleHook")]
public class GrappleHook : Actor
{
    public static GrappleSubMenu Settings { get => Casju0TrailMixModule.Settings.GrappleSettings; }

    public static int StGrappleShoot { get; private set; }
    public static int StGrappleToWall { get; private set; }
    public static int StGrappleItem { get; private set; }

    public static GrappleHook Grapple { get; private set; }

    private const string shootSoundPath = "event:/casju0_TrailMix/grapple_shoot";
    private const string harpoonSoundPath = "event:/casju0_TrailMix/grapple_harpoon";
    private const string retractSoundPath = "event:/casju0_TrailMix/grapple_retract";
    private const string grappleHitSoundPath = "event:/casju0_TrailMix/grapple_hit";
    private const string manaSoundPath = "event:/casju0_TrailMix/max_mana";
    public EventInstance shootSound { get; private set; }
    public EventInstance retractSound { get; private set; }

    public const float hitboxWidth = 8;
    public const float hitboxHeight = 4;
    public const float climbJumpForceXMoveDuration = 0.17f;

    public float ShootTimer { get; private set; }

    public bool CanAdvance { get; private set; }
    public bool CanInterrupt { get; private set; }

    public float WavyInverseStrength { get; private set; } = 30;
    public float WavyFrequency { get; private set; } = 0.5f;

    public Hitbox Hitbox { get; private set; }
    public bool AlreadyShotOnceWithCurrentHeldInput { get; private set; }
    public bool FreezeFramesActive { get; private set; }
    public Holdable GrappledHoldable { get; private set; }
    public Facings Facing { get; private set; }
    public Vector2 PreGrappleSpeed { get; private set; }
    public float PreJumpTimer { get; private set; }
    public bool IsGrappleHookOutsideGun { get; private set; }
    public bool Refilled { get; set; }
    public bool ShouldPlayRetractSound { get; private set; }
    public float ContactPauseTimer { get; private set; }
    public float CooldownTimer { get; private set; }

    public static Hook GrappleAsGrabInputHook { get; private set; }

    public static bool HasAmmo
    {
        get
        {
            return (Settings.InventoryType == GrappleSubMenu.InventoryTypes.AlwaysEquipped) ||
                   (Settings.InventoryType == GrappleSubMenu.InventoryTypes.RequiresRefill && Grapple.Refilled);
        }
    }

    public static bool ShootCheck
    {
        get
        {
            switch (Settings.ControlType)
            {
                case GrappleSubMenu.ControlTypes.ReplaceGrab:
                    return Input.Grab.Check;
                case GrappleSubMenu.ControlTypes.ReplaceDash:
                    return Input.Dash.Check;
                case GrappleSubMenu.ControlTypes.UserBinding:
                    return Casju0TrailMixModule.Settings.ShootGrapple.Check;
                default:
                    return false;
            }
        }
    }

    public static bool LevelBoundsCheck(Player player, Facings dir)
    {
        Level level = player.SceneAs<Level>();
        var stuckAtLeft = (int)dir == -1 && player.Left - 1 <= (float)level.Bounds.Left;
        var stuckAtRight = (int)dir == 1 && player.Right + 1 >= (float)level.Bounds.Right;
        return stuckAtLeft || stuckAtRight;
    }

    public GrappleHook()
        : base(Vector2.Zero)
    {
        Visible = true;
        base.Collider = new Hitbox(
            hitboxWidth,
            hitboxHeight,
            -hitboxWidth / 2,
            -hitboxHeight
        );
    }

    public static void Load()
    {
        On.Celeste.Player.ctor += AddGrappleStateMachines;
        On.Celeste.Player.NormalUpdate += HandlePlayerUpdate;
        On.Celeste.Level.LoadLevel += AddGrappleOnLevelLoad;
        On.Celeste.Player.Die += StopGrappleSoundsOnDeath;
        On.Celeste.Puffer.ctor_EntityData_Vector2 += AddPufferGrappleInteraction;
        On.Celeste.Player.WindMove += DisableWindMoveForGrapplingPlayers;
        GrappleAsGrabInputHook = new Hook(
            typeof(Input).GetProperty("GrabCheck", BindingFlags.Public | BindingFlags.Static).GetGetMethod(true),
            typeof(GrappleHook).GetMethod("modGrabCheck", BindingFlags.NonPublic | BindingFlags.Static)
        );
    }

    public static void Unload()
    {
        On.Celeste.Player.ctor -= AddGrappleStateMachines;
        On.Celeste.Player.NormalUpdate -= HandlePlayerUpdate;
        On.Celeste.Level.LoadLevel -= AddGrappleOnLevelLoad;
        On.Celeste.Player.Die -= StopGrappleSoundsOnDeath;
        On.Celeste.Puffer.ctor_EntityData_Vector2 -= AddPufferGrappleInteraction;
        On.Celeste.Player.WindMove -= DisableWindMoveForGrapplingPlayers;
        GrappleAsGrabInputHook?.Dispose();
    }

    static bool modGrabCheck(Func<bool> orig)
    {
        return ShootCheck || orig();
    }

    public override void Update()
    {
        base.Update();
        if (CooldownTimer > 0)
        {
            CooldownTimer -= Engine.DeltaTime;
            if (CooldownTimer <= 0)
            {
                Audio.Play(manaSoundPath);
            }
        }
        if (ShouldPlayRetractSound && ContactPauseTimer <= 0)
        {
            retractSound = Audio.Play(retractSoundPath);
            ShouldPlayRetractSound = false;
        }
        var self = Scene.Tracker.GetEntity<Player>();
        if (self != null &&
            Grapple.IsGrappleHookOutsideGun &&
            self.StateMachine.State != StGrappleItem &&
            self.StateMachine.State != StGrappleShoot &&
            self.StateMachine.State != StGrappleToWall)
        {
            var difference = (self.Center + Vector2.UnitY * Settings.YOffset) - Grapple.Center;
            var distance = difference.LengthSquared();
            var movement = difference.SafeNormalize() * Settings.RetractSpeed * Engine.DeltaTime;
            var movementDistance = movement.LengthSquared();
            Grapple.NaiveMove(movement);
            if (distance <= movementDistance)
            {
                Grapple.IsGrappleHookOutsideGun = false;
                Audio.Stop(retractSound);
            }
        }
    }

    public override void Render()
    {
        base.Render();
        Player player = Scene.Tracker.GetEntity<Player>();
        if (player == null) { return; }
        // cooldown circle
        if (CooldownTimer > 0f)
        {
            float x = (Settings.Cooldown - CooldownTimer) / Settings.Cooldown;
            float c1 = 1.70158f;
            float c3 = c1 + 1;
            float t = c3 * x * x * x - c1 * x * x;
            float color = Math.Min(t + 0.25f, 1f);
            Draw.Circle(player.Center, (1 - t) * 20f, new Color(color, color, color, color), 4);
        }
        if (IsGrappleHookOutsideGun)
        {
            if (IsPlayerGrapplingSomething(player))
            {
                WavyInverseStrength = Math.Max(15, WavyInverseStrength - 1);
                WavyFrequency = Math.Max(WavyFrequency - 0.025f, 0.2f);
            }
            else
            {
                WavyInverseStrength = 30;
                WavyFrequency = 0.5f;
            }

            // use a berenstein line algorithm to get the location of each pixel for a straight line
            // then modulate the y values of those pixels with a sine wave
            var p1 = player.Center + Vector2.UnitY * Settings.YOffset;
            var p2 =
                Center
                + Vector2.UnitX * (hitboxWidth / 2) * (float)Facing;
            var delta = p2 - p1;
            var step = Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y));
            if (step != 0)
            {
                var stepX = delta.X / step;
                var stepY = delta.Y / step;
                for (var i = 0; i < step; i++)
                {
                    var baseY = p1.Y + i * stepY;
                    var oy1 =
                        (float)Math.Sin(-Scene.TimeActive * 50 + i * WavyFrequency)
                        * (step / WavyInverseStrength);
                    var oy2 = 1 - (Math.Abs(step / 2 - i) / (step / 2));
                    Draw.Point(
                        new Vector2(p1.X + i * stepX, baseY + oy1 * oy2),
                        (Celeste.FreezeTimer + ContactPauseTimer) > 0 ? Settings.ContactColor
                            : IsPlayerGrapplingSomething(player) ? Settings.ReelColor
                            : Settings.StringColor
                        );
                }
            }
        }
    }

    private static void AddGrappleStateMachines(
        On.Celeste.Player.orig_ctor orig,
        Player self,
        Vector2 position,
        PlayerSpriteMode spriteMode
    )
    {
        orig(self, position, spriteMode);
        StGrappleShoot = self.StateMachine.AddState(
            "grappleShoot",
            () => GrappleShootUpdate(self),
            null,
            () => GrappleShootBegin(self),
            () => GrappleShootEnd(self)
        );
        StGrappleToWall = self.StateMachine.AddState(
            "grappleToWall",
            () => GrappleToWallUpdate(self),
            null,
            () => GrappleToWallBegin(self),
            () => GrappleToWallEnd(self)
        );
        StGrappleItem = self.StateMachine.AddState(
            "grappleItem",
            () => GrappleItemUpdate(self),
            null,
            () => GrappleItemBegin(self),
            () => GrappleItemEnd(self)
        );
    }

    public static int HandlePlayerUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self)
    {
        if (!self.SceneAs<Level>().Session.GetFlag(GrappleController.flag))
        {
            return orig(self);
        }

        if (HasAmmo)
        {
            if (Grapple.AlreadyShotOnceWithCurrentHeldInput && !ShootCheck)
            {
                Grapple.AlreadyShotOnceWithCurrentHeldInput = false;
            }

            if (self.LiftBoost.Y < 0f && self.wasOnGround && !self.onGround && self.Speed.Y >= 0f)
            {
                self.Speed.Y = self.LiftBoost.Y;
            }

            if (self.Holding == null)
            {
                if (ShootCheck)
                {
                    // if you can grab an item, prefer that over shooting the grapple
                    if (!self.IsTired && !self.Ducking)
                    {
                        foreach (Holdable component in self.Scene.Tracker.GetComponents<Holdable>())
                        {
                            if (component.Check(self) && self.Pickup(component))
                            {
                                return 8;
                            }
                        }
                    }
                    // otherwise, try shoot out the grapple
                    if (
                        Grapple.CooldownTimer <= 0 &&
                        (Settings.CanShootWhenTired || !self.IsTired) &&
                        !Grapple.AlreadyShotOnceWithCurrentHeldInput &&
                        !Grapple.IsGrappleHookOutsideGun
                    )
                    {
                        Grapple.AlreadyShotOnceWithCurrentHeldInput = true;
                        if (Settings.InventoryType == GrappleSubMenu.InventoryTypes.RequiresRefill)
                        {
                            Grapple.Refilled = false;
                        }
                        return StGrappleShoot;
                    }
                }
            }
        }
        return orig(self);
    }

    private static void AddGrappleOnLevelLoad(
        On.Celeste.Level.orig_LoadLevel orig,
        Level self,
        Player.IntroTypes playerIntro,
        bool isFromLoader
    )
    {
        Grapple = [];
        self.Add(Grapple);
        orig(self, playerIntro, isFromLoader);
    }

    private static PlayerDeadBody StopGrappleSoundsOnDeath(
        On.Celeste.Player.orig_Die orig,
        Player self,
        Vector2 direction,
        bool evenIfInvincible,
        bool registerDeathInStats
    )
    {
        Audio.Stop(Grapple.retractSound);
        Audio.Stop(Grapple.shootSound);
        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }

    private static void AddPufferGrappleInteraction(
        On.Celeste.Puffer.orig_ctor_EntityData_Vector2 orig,
        Puffer self,
        EntityData data,
        Vector2 offset
    )
    {
        orig(self, data, offset);
        if (Settings.PufferBehavior == GrappleSubMenu.PufferBehaviors.PullPuffer)
        {
            self.Add(
                new GrappleHoldable
                {
                    OnGrappleRelease = (dir) =>
                    {
                        self.GotoHitSpeed(Vector2.UnitX * 120f * -dir);
                    },
                }
            );
            self.Add(new Holdable { cannotHoldTimer = float.PositiveInfinity });
        }
        else if (Settings.PufferBehavior == GrappleSubMenu.PufferBehaviors.PullPlayer)
        {
            self.Add(new GrappleSolid());
        }
    }

    public static void DisableWindMoveForGrapplingPlayers(On.Celeste.Player.orig_WindMove orig, Player self, Vector2 move)
    {
        if (self.StateMachine.State != StGrappleItem &&
            self.StateMachine.State != StGrappleToWall &&
            self.StateMachine.State != StGrappleShoot)
        {
            orig(self, move);
        }
    }

    public static bool IsPlayerGrapplingSomething(Player p)
    {
        return p.StateMachine.State == StGrappleToWall
            || p.StateMachine.State == StGrappleItem;
    }

    private static void OnContact(bool hitWall)
    {
        if (hitWall)
        {
            Audio.Play(grappleHitSoundPath);
            if (Settings.Shockwave != GrappleSubMenu.Shockwaves.None)
            {
                float shockwaveMultiplier = Settings.Shockwave == GrappleSubMenu.Shockwaves.Large ? 2 : 1;
                Grapple.SceneAs<Level>()
                    .Displacement.AddBurst(
                        Grapple.Center,
                        0.4f,
                        8f * shockwaveMultiplier,
                        64f * shockwaveMultiplier,
                        0.5f,
                        Ease.QuadOut,
                        Ease.QuadOut
                    );
            }
        }

        switch (Settings.FreezeFrameLength)
        {
            case GrappleSubMenu.FreezeFrameLengths.Short:
                Celeste.Freeze(0.05f);
                break;
            case GrappleSubMenu.FreezeFrameLengths.Long:
                Celeste.Freeze(0.1f);
                break;
            case GrappleSubMenu.FreezeFrameLengths.VeryLong:
                Celeste.Freeze(0.2f);
                break;
            default:
                break;
        }
        Grapple.ContactPauseTimer = 0f;
        switch (Settings.ContactPauseLength)
        {
            case GrappleSubMenu.FreezeFrameLengths.Short:
                Grapple.ContactPauseTimer = 0.05f;
                break;
            case GrappleSubMenu.FreezeFrameLengths.Long:
                Grapple.ContactPauseTimer = 0.1f;
                break;
            case GrappleSubMenu.FreezeFrameLengths.VeryLong:
                Grapple.ContactPauseTimer = 0.2f;
                break;
            default:
                break;
        }
    }

    private static void GrappleCancel(Player p)
    {
        switch (Settings.PlayerReleaseMomentum)
        {
            case GrappleSubMenu.PlayerReleaseMomentums.RetainGrappleSpeed:
                p.Speed.X += Settings.PlayerReleaseSpeedBoost * (int)Grapple.Facing;
                break;
            case GrappleSubMenu.PlayerReleaseMomentums.RevertSpeed:
                p.Speed = Grapple.PreGrappleSpeed;
                break;
        }
        if (Settings.PlayerReleaseMomentumSpeedCap >= 0)
        {
            p.Speed.X = Math.Clamp(p.Speed.X, -Settings.PlayerReleaseMomentumSpeedCap, Settings.PlayerReleaseMomentumSpeedCap);
        }
    }

    static void ReleaseItem(Player p)
    {
        var direction = (
            p.Center + p.carryOffset - Grapple.GrappledHoldable.Entity.Center
        ).SafeNormalize();
        Grapple.GrappledHoldable.SetSpeed(Settings.ItemPullSpeed * direction);
        var grappleComponent =
            Grapple.GrappledHoldable.Entity.Components.Get<GrappleHoldable>();
        grappleComponent?.OnGrappleRelease?.Invoke((int)Grapple.Facing);
        Grapple.GrappledHoldable = null;
    }

    #region grapple state machine functions
    #region grapple shoot state
    static int GrappleShootUpdate(Player self)
    {
        if (Grapple.ShootTimer > Settings.MinShootDuration)
        {
            Grapple.CanInterrupt = true;
        }
        if (Grapple.ShootTimer > Settings.MaxShootDuration)
        {
            Grapple.CanAdvance = false;
        }
        Grapple.ShootTimer += Engine.DeltaTime;
        var shouldRetract = !Grapple.CanAdvance || (!ShootCheck && Grapple.CanInterrupt);
        if (shouldRetract)
        {
            if (Settings.HarpoonMode)
            {
                OnContact(false);
                return StGrappleToWall;
            }
            else
            {
                Grapple.ShouldPlayRetractSound = true;
                self.Speed = Grapple.PreGrappleSpeed;
                self.varJumpTimer = Grapple.PreJumpTimer;
                return Player.StNormal;
            }
        }
        var grappleHookTarget = Grapple.CollideFirstByComponent<GrappleSolid>();
        if (grappleHookTarget != null)
        {
            OnContact(true);
            return StGrappleToWall;
        }
        if (ClimbBlocker.Check(self.Scene, Grapple))
        {
            return Player.StNormal;
        }
        var shootSpeed = Settings.ShootSpeed;
        var preXSpeed = Grapple.PreGrappleSpeed.X * (int)Grapple.Facing;
        if (Settings.AddMomentumToShot && preXSpeed > Player.DashSpeed)
        {
            shootSpeed += preXSpeed - Player.DashSpeed;
        }
        var collided = false;
        Grapple.MoveH(((int)Grapple.Facing) * shootSpeed * Engine.DeltaTime, (_coll) => collided = true);
        if (collided)
        {
            OnContact(true);
            return StGrappleToWall;
        }
        foreach (Holdable component in self.Scene.Tracker.GetComponents<Holdable>())
        {
            var grappleComponent = component.Entity.Components.Get<GrappleHoldable>();
            Collider origCollider = component.Entity.Collider;
            if (component.PickupCollider != null)
            {
                component.Entity.Collider = component.PickupCollider;
            }
            else if (grappleComponent?.Collider != null)
            {
                component.Entity.Collider = grappleComponent.Collider;
            }
            bool grappleCollided = Grapple.CollideCheck(component.Entity);
            component.Entity.Collider = origCollider;

            if (grappleCollided)
            {
                Grapple.GrappledHoldable = component;
                grappleComponent?.OnGrapplePickup?.Invoke((int)Grapple.Facing);
                return StGrappleItem;
            }
        }
        return StGrappleShoot;
    }

    static void GrappleShootBegin(Player self)
    {
        Grapple.ShootTimer = 0f;
        Grapple.IsGrappleHookOutsideGun = true;
        Grapple.PreGrappleSpeed = self.Speed;
        Grapple.PreJumpTimer = self.varJumpTimer;
        self.Speed = Vector2.Zero;
        Grapple.Center = self.Center + Vector2.UnitY * Settings.YOffset;
        Grapple.Facing = self.Facing;
        Grapple.CanAdvance = true;
        Grapple.CanInterrupt = false;
        Grapple.shootSound = Audio.Play(Settings.HarpoonMode ? harpoonSoundPath : shootSoundPath);
    }

    static void GrappleShootEnd(Player self)
    {
        if (!Settings.HarpoonMode)
        {
            Audio.Stop(Grapple.shootSound);
        }
    }
    #endregion
    #region grapple to wall state
    static int GrappleToWallUpdate(Player self)
    {
        if (Grapple.ContactPauseTimer > 0f)
        {
            Grapple.ContactPauseTimer -= Engine.DeltaTime;
            return StGrappleToWall;
        }
        var pullSpeed = Settings.WallPullSpeed;
        var preXSpeed = Grapple.PreGrappleSpeed.X * (int)Grapple.Facing;
        if (Settings.AddMomentumToShot && preXSpeed > Player.DashSpeed)
        {
            pullSpeed += preXSpeed - Player.DashSpeed;
        }
        self.Speed.X = pullSpeed * (int)Grapple.Facing;

        if (Settings.CanWallDashCancel && (Input.DashPressed || Input.CrouchDashPressed) && self.Dashes > 0)
        {
            return self.StartDash();
        }
        else if (Settings.CanWallJumpCancel && Input.Jump.Pressed)
        {
            if (self.Stamina > 0)
            {
                self.Stamina -= Settings.JumpCancelStaminaCost;
                self.Jump();
                self.sweatSprite.Play("jump", restart: true);
                self.forceMoveXTimer = climbJumpForceXMoveDuration;
                self.forceMoveX = (int)Grapple.Facing;
                if (self.Facing == Facings.Right)
                {
                    self.Play("event:/char/madeline/jump_climb_right");
                }
                else
                {
                    self.Play("event:/char/madeline/jump_climb_left");
                }
            }
            GrappleCancel(self);
            return Player.StNormal;
        }
        else if (Settings.CanWallCancel && !ShootCheck)
        {
            GrappleCancel(self);
            return Player.StNormal;
        }
        else if (self.ClimbCheck((int)Grapple.Facing))
        {
            return Player.StClimb;
        }
        else if (LevelBoundsCheck(self, Grapple.Facing))
        {
            GrappleCancel(self);
            return Player.StNormal;
        }
        else
        {
            var playerFront = self.CenterX + self.Width / 2f * (float)Grapple.Facing;
            var grappleFront = Grapple.CenterX + Grapple.Width / 2f * (float)Grapple.Facing;
            if (Math.Sign(grappleFront - playerFront) != (int)Grapple.Facing)
            {
                GrappleCancel(self);
                return Player.StNormal;
            }
            var collided = false;
            var origPosition = self.Position;
            self.Position.X += (int)Grapple.Facing;
            if (self.CollideCheck<Solid>())
            {
                collided = true;
            }
            self.Position = origPosition;
            return collided ? Player.StNormal : StGrappleToWall;
        }
    }

    static void GrappleToWallBegin(Player self)
    {
        if (!Settings.HarpoonMode)
        {
            Grapple.ShouldPlayRetractSound = true;
        }
    }

    private static void GrappleToWallEnd(Player self)
    {
        Grapple.CooldownTimer = Settings.Cooldown;
    }
    #endregion
    #region grapple item state
    static int GrappleItemUpdate(Player self)
    {
        Grapple.GrappledHoldable.gravityTimer = 0.1f;
        Grapple.GrappledHoldable.SetSpeed(Vector2.Zero);
        if (Grapple.ContactPauseTimer > 0f)
        {
            Grapple.ContactPauseTimer -= Engine.DeltaTime;
            return StGrappleItem;
        }
        else if (Settings.CanItemJumpCancel && (Input.DashPressed || Input.CrouchDashPressed) && self.Dashes > 0)
        {
            ReleaseItem(self);
            return self.StartDash();
        }
        else if (Settings.CanItemJumpCancel && Input.Jump.Pressed)
        {
            if (self.Stamina > 0)
            {
                self.Stamina -= Settings.JumpCancelStaminaCost;
                self.Jump();
                self.sweatSprite.Play("jump", restart: true);
                self.forceMoveXTimer = climbJumpForceXMoveDuration;
                self.forceMoveX = (int)Grapple.Facing;
                if (self.Facing == Facings.Right)
                {
                    self.Play("event:/char/madeline/jump_climb_right");
                }
                else
                {
                    self.Play("event:/char/madeline/jump_climb_left");
                }
            }
            GrappleCancel(self);
            return Player.StNormal;
        }
        else if (Settings.CanItemCancel && !ShootCheck)
        {
            self.Speed = Grapple.PreGrappleSpeed;
            self.varJumpTimer = Grapple.PreJumpTimer;
            ReleaseItem(self);
            GrappleCancel(self);
            return Player.StNormal;
        }
        else
        {
            var direction = (
                (self.Center + Vector2.UnitY * Settings.YOffset) - Grapple.GrappledHoldable.Entity.Center
            ).SafeNormalize();

            var a = Grapple.GrappledHoldable.EntityAs<Actor>();
            var pullSpeed = Settings.WallPullSpeed;
            var preXSpeed = Grapple.PreGrappleSpeed.X * (int)Grapple.Facing;
            if (Settings.AddMomentumToShot && preXSpeed > Player.DashSpeed)
            {
                pullSpeed += preXSpeed - Player.DashSpeed;
            }

            var shouldExit = false;
            var nextTarget = pullSpeed * direction * Engine.DeltaTime;
            var aStartPos = a.Position;
            a.MoveV(nextTarget.Y);
            a.MoveH(nextTarget.X, (_) => { ReleaseItem(self); shouldExit = true; });
            var aEndPos = a.Position;
            var delta = aEndPos - aStartPos;
            Grapple.NaiveMove(delta);
            if (shouldExit)
            {
                GrappleCancel(self);
                return Player.StNormal;
            }

            var grappledReachedPlayer =
                Grapple.Facing == Facings.Left
                    ? Grapple.Right >= self.Left
                    : Grapple.Left <= self.Right;
            if (grappledReachedPlayer)
            {
                self.Pickup(Grapple.GrappledHoldable);
                self.Speed = Grapple.PreGrappleSpeed;
                self.varJumpTimer = Grapple.PreJumpTimer;
                Grapple.IsGrappleHookOutsideGun = false;
                Grapple.GrappledHoldable = null;
                Audio.Stop(Grapple.retractSound);
                return 8;
            }
            return StGrappleItem;
        }
    }

    static void GrappleItemBegin(Player self)
    {
        var grappleComponent =
            Grapple.GrappledHoldable.Entity.Components.Get<GrappleHoldable>();
        grappleComponent?.OnGrapplePickup?.Invoke((int)Grapple.Facing);
        OnContact(true);
        if (!Settings.HarpoonMode)
        {
            Grapple.ShouldPlayRetractSound = true;
        }
    }

    private static void GrappleItemEnd(Player self)
    {
        Grapple.CooldownTimer = Settings.Cooldown;
    }
    #endregion
    #endregion
}
