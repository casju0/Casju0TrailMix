using System;
using System.Collections.Generic;

namespace Celeste.Mod.Casju0TrailMix.Entities;

public class ThrowBlockDebris : Entity
{
    private float gravity;
    private float maxFallSpeed;

    private Vector2 speed;
    private float age;
    private List<MTexture> debrisSubTextures = new();
    private int startOffset;

    public ThrowBlockDebris(Vector2 position, Vector2 speed, float gravity = 800f, float maxFallSpeed = 240f)
    {
        Position = position;
        this.speed = speed;
        this.gravity = gravity;
        this.maxFallSpeed = maxFallSpeed;

        var debrisTexture = GFX.Game["objects/Casju0Trailmix/throwBlock/debris"];
        for (int i = 0; i < 6 * 8; i++)
        {
            var r = i / 6;
            var c = i % 6;
            debrisSubTextures.Add(debrisTexture.GetSubtexture(c * 8, r * 8, 8, 8));
        }

        startOffset = Calc.Random.Range(0, 8 * 6);

        Depth = Depths.FGParticles;
    }

    public override void Update()
    {
        base.Update();
        age += Engine.DeltaTime;
        if (age > 3000)
        {
            RemoveSelf();
        }

        speed.Y = Calc.Approach(speed.Y, maxFallSpeed, gravity * Engine.DeltaTime);

        Position += speed * Engine.DeltaTime;
    }

    public override void Render()
    {
        base.Render();
        int a = (int)Math.Floor(age / (Engine.DeltaTime * 2)) + startOffset;
        int r = a % 8;
        int c = a % 6;
        int i = r * 6 + c;
        debrisSubTextures[i].DrawCentered(Position);
    }
}

[Tracked]
[CustomEntity("Casju0TrailMix/ThrowBlock")]
public class ThrowBlock : Actor
{
    static ThrowBlockSubMenu Settings { get => Casju0TrailMixModule.Settings.ThrowBlockSettings; }

    private static bool playerJustWallGrabbedThrowBlock = false;

    private float wallSpringVelocityX;
    private float wallSpringVelocityY;
    private float floorSpringVelocityX;
    private float floorSpringVelocityY;

    private float groundFriction;
    private float gravity;
    private float maxFallSpeed;

    private float lifeTime;
    private float slowFlickerTime;

    private bool hasLight;

    private enum States
    {
        Untouched = 0,
        Grabbed = 1,
        Thrown = 2,
        Dropped = 3,
        Poofed = 4
    }

    private States state;
    private Vector2 speed;
    private Holdable hold;
    private Sprite sprite;
    private VertexLight light;
    private Solid solid;

    private Collision onCollideH;
    private Collision onCollideV;

    public ThrowBlock(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        Collider = new Hitbox(16f, 16f, -8f, -16f);
        Add(hold = Settings.UseSmwHoldables ? FemtoHelperImports.CreateSmwHoldable?.Invoke(0, 0, HandleClipDeath, null) ?? new Holdable() : new Holdable());
        hold.SlowFall = false;
        hold.SlowRun = false;
        hold.OnRelease = HandleRelease;
        hold.OnHitSpring = HandleHitSpring;
        hold.SpeedGetter = () => speed;
        hold.SpeedSetter = (v) => speed = v;

        #region sprite setup
        Add(sprite = new Sprite(GFX.Game, "objects/Casju0TrailMix/throwBlock/"));
        sprite.SetOrigin(8f, 16f);
        sprite.AddLoop(
            "idle",
            1f,
            GFX.Game.GetAtlasSubtextures("objects/Casju0TrailMix/throwBlock/idle").ToArray()
        );
        sprite.AddLoop(
            "active",
            3f / 60,
            GFX.Game.GetAtlasSubtextures("objects/Casju0TrailMix/throwBlock/active").ToArray()
        );
        sprite.AddLoop(
            "activeSlow",
            9f / 60,
            GFX.Game.GetAtlasSubtextures("objects/Casju0TrailMix/throwBlock/active").ToArray()
        );
        sprite.Add(
            "poof",
            0.017f * 4,
            "end",
            [.. GFX.Game.GetAtlasSubtextures("objects/Casju0TrailMix/enderBooster/break")]
        );
        sprite.AddLoop("end", 0f, [.. GFX.Game.GetAtlasSubtextures("objects/Casju0TrailMix/enderBooster/end")]);
        sprite.Play("idle");
        #endregion

        #region attribute setup
        wallSpringVelocityX = data.Float("wallSpringVelocityX", 220f);
        wallSpringVelocityY = data.Float("wallSpringVelocityY", -80);
        floorSpringVelocityX = data.Float("floorSpringVelocityX", 0.5f);
        floorSpringVelocityY = data.Float("floorSpringVelocityY", -330f);

        groundFriction = data.Float("groundFriction", 800f);
        gravity = data.Float("gravity", 800f);
        maxFallSpeed = data.Float("maxFallSpeed", 160f);

        lifeTime = data.Float("lifeTime", 10f);
        slowFlickerTime = data.Float("slowFlickerTime", 3f);

        hasLight = data.Bool("hasLight", true);
        #endregion

        onCollideH = OnCollideH;
        onCollideV = OnCollideV;

        Depth = Depths.TheoCrystal;

        Add(light = new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 48, 64));
        light.Visible = false;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Scene.Add(solid = new Solid(Position - new Vector2(8, 16), 16, 16, true));
        solid.EnableStaticMovers();
    }

    public static void Load()
    {
        On.Celeste.Player.Update += HandleUpdate;
        On.Celeste.Player.NormalUpdate += HandlePlayerNormalUpdate;
        On.Celeste.Player.DashUpdate += HandlePlayerDashUpdate;
        On.Celeste.Player.ClimbUpdate += HandlePlayerClimbUpdate;
        On.Celeste.Player.ClimbJump += HandlePlayerClimbJump;
        On.Celeste.Player.WallJump += HandlePlayerWallJump;
        On.Celeste.Player.SuperWallJump += HandlePlayerSuperWallJump;
        On.Celeste.Player.Jump += HandleJump;
    }

    public static void Unload()
    {
        On.Celeste.Player.Update -= HandleUpdate;
        On.Celeste.Player.NormalUpdate -= HandlePlayerNormalUpdate;
        On.Celeste.Player.DashUpdate -= HandlePlayerDashUpdate;
        On.Celeste.Player.ClimbUpdate -= HandlePlayerClimbUpdate;
        On.Celeste.Player.ClimbJump -= HandlePlayerClimbJump;
        On.Celeste.Player.WallJump -= HandlePlayerWallJump;
        On.Celeste.Player.SuperWallJump -= HandlePlayerSuperWallJump;
        On.Celeste.Player.Jump -= HandleJump;
    }

    public override void Update()
    {
        base.Update();

        #region lifetime
        if (state != States.Untouched && state != States.Poofed)
        {
            lifeTime -= Engine.DeltaTime;
            if (lifeTime <= 0)
            {
                state = States.Poofed;
                sprite.SetOrigin(16, 24);
                sprite.Play("poof");
                Collidable = false;
                light.Visible = false;
                solid.DestroyStaticMovers();
                if (hold.Holder is Player)
                {
                    hold.Holder.Drop();
                }
            }
            else if (lifeTime <= slowFlickerTime)
            {
                sprite.PlayOffset("activeSlow", sprite.CurrentAnimationFrame);
            }
        }
        #endregion
        var p = Scene.Tracker.GetEntity<Player>();

        #region physics update
        if (state == States.Poofed)
        {
            speed.X = Calc.Approach(speed.X, 0, groundFriction * Engine.DeltaTime);
            speed.Y = Calc.Approach(speed.Y, 0, groundFriction * Engine.DeltaTime);
        }
        else
        {
            if (OnGround())
            {
                float target =
                    (!OnGround(Position + Vector2.UnitX * 3f))
                        ? 20f
                        : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f));
                if (state != States.Thrown)
                {
                    speed.X = Calc.Approach(speed.X, target, groundFriction * Engine.DeltaTime);
                }
                Vector2 liftSpeed = base.LiftSpeed;
                if (liftSpeed.Y < 0f && speed.Y < 0f)
                {
                    speed.Y = 0f;
                }
            }
            else if (hold.ShouldHaveGravity)
            {
                float num = gravity;
                if (Math.Abs(speed.Y) <= 30f)
                {
                    num *= 0.5f;
                }
                speed.Y = Calc.Approach(speed.Y, maxFallSpeed, num * Engine.DeltaTime);
            }
        }

        MoveH(speed.X * Engine.DeltaTime, onCollideH);
        MoveV(speed.Y * Engine.DeltaTime, onCollideV);

        if (state == States.Dropped || state == States.Thrown)
        {
            foreach (TouchSwitch entity2 in base.Scene.Tracker.GetEntities<TouchSwitch>())
            {
                if (CollideCheck(entity2))
                {
                    entity2.TurnOn();
                    break;
                }
            }

            foreach (Seeker seeker in base.Scene.Tracker.GetEntities<Seeker>())
            {
                if (CollideCheck(seeker))
                {
                    seeker.GotBouncedOn(this);
                    Break();
                    break; // break
                }
            }
        }
        hold.CheckAgainstColliders();
        #endregion

        var prevPosition = solid.Position;
        solid.Position = Position + new Vector2(-8, -16);
        var delta = solid.Position - prevPosition;
        solid.MoveStaticMovers(delta);
    }

    private void Break()
    {
        Audio.Play("event:/casju0_TrailMix/smw_shatter", Center);
        solid.DestroyStaticMovers();
        RemoveSelf();
        var dy = Vector2.UnitY * 4;
        Scene.Add(new ThrowBlockDebris(TopLeft - dy, new Vector2(-20f, -360f), gravity, maxFallSpeed));
        Scene.Add(new ThrowBlockDebris(TopRight - dy, new Vector2(20f, -360f), gravity, maxFallSpeed));
        Scene.Add(new ThrowBlockDebris(BottomLeft, new Vector2(-20f, -300f), gravity, maxFallSpeed));
        Scene.Add(new ThrowBlockDebris(BottomRight, new Vector2(20f, -300f), gravity, maxFallSpeed));
    }

    private void OnCollideH(CollisionData data)
    {
        if (state == States.Dropped || state == States.Thrown)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(speed.X));
            }
            else if (data.Hit is DashBlock)
            {
                (data.Hit as DashBlock).Break(Center, speed.SafeNormalize(), true);
            }
            Break();
        }
    }

    private void OnCollideV(CollisionData data)
    {
        if (state == States.Dropped)
        {
            speed.X *= 0.5f;
        }
        if (state == States.Dropped || state == States.Thrown)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(speed.Y));
            }

            if (Math.Abs(speed.Y) > 40 && state != States.Thrown)
            {
                speed.Y *= Math.Sign(speed.Y) == 1 ? -0.3f : -0.1f;
            }
            else
            {
                speed.Y = 0f;
            }
        }
    }

    private void HandleRelease(Vector2 force)
    {
        RemoveTag(Tags.Persistent);
        Audio.Play("event:/casju0_TrailMix/smw_kick");
        Player player = Scene.Tracker.GetNearestEntity<Player>(Position);
        if (force.X != 0f && force.Y == 0f)
        {
            state = States.Thrown;
            if (Input.Aim.Value.Y < 0f)
            {
                state = States.Dropped;
                force.Y = -4f;
                force.X = player.Speed.X / 300f;
            }
        }
        else
        {
            state = States.Dropped;
            force.X = player.Speed.X / 300f;
        }
        speed = force * new Vector2(240f, 100f);
    }

    public bool HandleHitSpring(Spring spring)
    {
        if (!hold.IsHeld)
        {
            if (spring.Orientation == Spring.Orientations.Floor && speed.Y >= 0f)
            {
                speed.X *= floorSpringVelocityX;
                speed.Y = floorSpringVelocityY;
                return true;
            }
            if (spring.Orientation == Spring.Orientations.WallLeft && speed.X <= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                speed.X = wallSpringVelocityX;
                speed.Y = wallSpringVelocityY;
                return true;
            }
            if (spring.Orientation == Spring.Orientations.WallRight && speed.X >= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                speed.X = -wallSpringVelocityX;
                speed.Y = wallSpringVelocityY;
                return true;
            }
        }
        return false;
    }

    private void HandleClipDeath(Vector2 vector)
    {
        Break();
    }

    #region pickup handlers
    private static Func<Player, bool> MakePickupHandler(Func<Player, ThrowBlock, bool> checkFn, bool giveGrace)
    {
        return (Player player) =>
        {
            if (HoldCheck(player))
            {
                var throwBlocks = player.Scene.Tracker.GetEntities<ThrowBlock>();

                foreach (ThrowBlock throwBlock in throwBlocks)
                {
                    if (checkFn(player, throwBlock))
                    {
                        player.Pickup(throwBlock.hold);
                        player.jumpGraceTimer = giveGrace ? Settings.GraceJumpDuration : 0.0f;

                        throwBlock.state = States.Grabbed;
                        throwBlock.sprite.Play("active");
                        throwBlock.light.Visible = throwBlock.hasLight;
                        throwBlock.AddTag(Tags.Persistent);
                        throwBlock.solid.Collidable = false;
                        return true;
                    }
                }
            }
            return false;
        };
    }

    private static Func<Player, bool> TryPickupNormalUpdate = MakePickupHandler((p, tb) => RideCheck(p, tb), true);
    private static Func<Player, bool> TryPickupDashUpdate = MakePickupHandler(
        (p, tb) => RideCheck(p, tb) || WallCheck(p, tb, false, -1) || WallCheck(p, tb, false, 1),
        true
    );
    private static Func<Player, bool> TryPickupClimbUpdate = MakePickupHandler((p, tb) => WallCheck(p, tb, false, (int)p.Facing), true);
    private static Func<Player, bool> TryPickupClimbJump = MakePickupHandler((p, tb) => WallCheck(p, tb, false, (int)p.Facing), false);
    private static Func<Player, bool> TryPickupWallJump = MakePickupHandler(
        (p, tb) => WallCheck(p, tb, false, -1) || WallCheck(p, tb, false, 1),
        false
    );
    private static Func<Player, bool> TryPickupSuperWallJump = MakePickupHandler(
        (p, tb) => WallCheck(p, tb, true, -1) || WallCheck(p, tb, true, 1),
        false
    );
    #endregion

    #region player hooks
    private static void HandleUpdate(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);
        if (self.jumpGraceTimer <= 0f)
        {
            playerJustWallGrabbedThrowBlock = false;
        }
    }

    private static void HandleJump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx)
    {
        // convert a coyote jump to a climb jump if the player just grabbed a throwblock on its side
        if (playerJustWallGrabbedThrowBlock)
        {
            self.Stamina -= 27.5f - Settings.ClimbJumpRefundAmount;
            self.sweatSprite.Play("jump", restart: true);
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            if (self.Facing == Facings.Right)
            {
                self.Play("event:/char/madeline/jump_climb_right");
                Dust.Burst(self.Center + Vector2.UnitX * 2f, MathF.PI * -3f / 4f, 4, self.DustParticleFromSurfaceIndex(-1));
            }
            else
            {
                self.Play("event:/char/madeline/jump_climb_left");
                Dust.Burst(self.Center + Vector2.UnitX * -2f, -MathF.PI / 4f, 4, self.DustParticleFromSurfaceIndex(-1));
            }
            orig(self, particles: false, playSfx: false);
        }
        else
        {
            orig(self, particles, playSfx);
        }
    }

    private static int HandlePlayerNormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self)
    {
        return TryPickupNormalUpdate(self) ? Player.StPickup : orig(self);
    }

    private static int HandlePlayerDashUpdate(On.Celeste.Player.orig_DashUpdate orig, Player self)
    {
        var result = orig(self);
        if (Settings.AllowDashPickups)
        {
            TryPickupDashUpdate(self);
        }
        return result;
    }

    private static int HandlePlayerClimbUpdate(On.Celeste.Player.orig_ClimbUpdate orig, Player self)
    {
        if (Settings.AllowClimbPickups && TryPickupClimbUpdate(self))
        {
            playerJustWallGrabbedThrowBlock = true;
            return Player.StPickup;
        }
        else { return orig(self); }
    }

    private static void HandlePlayerClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
    {
        if (Settings.AllowClimbPickups && TryPickupClimbJump(self))
        {
            self.StateMachine.State = Player.StPickup;
            self.Stamina += Settings.ClimbJumpRefundAmount;
        }
        orig(self);
    }

    private static void HandlePlayerWallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir)
    {
        if (Settings.AllowWallJumpPickups && TryPickupWallJump(self))
        {
            self.StateMachine.State = Player.StPickup;
        }
        orig(self, dir);
    }

    private static void HandlePlayerSuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir)
    {
        if (Settings.AllowWallJumpPickups && TryPickupSuperWallJump(self))
        {
            self.StateMachine.State = Player.StPickup;
        }
        orig(self, dir);
    }
    #endregion

    private static bool HoldCheck(Player player)
    {
        return player.Holding == null && Input.Grab.Check && !player.IsTired;
    }

    private static bool RideCheck(Player player, ThrowBlock throwBlock)
    {
        var origPosition = player.Position;
        player.Position.Y += 1;
        var result = player.CollideCheck(throwBlock);
        player.Position = origPosition;
        return result;
    }

    private static bool WallCheck(Player player, ThrowBlock block, bool super = false, int? dir = null)
    {
        var origPosition = player.Position;
        player.Position.X += (super ? 5 : 3) * dir ?? (int)player.Facing;
        var result = player.CollideCheck(block);
        player.Position = origPosition;
        return result;
    }
}
