using System;

namespace Celeste.Mod.Casju0TrailMix.Entities;

[Tracked(false)]
[CustomEntity("Casju0TrailMix/EnderBooster")]
public class EnderBooster : Actor
{
    static readonly MTexture s_outlineTexture = GFX.Game["objects/Casju0TrailMix/enderBooster/outline"];
    private const string spritePathPrefix = "objects/Casju0TrailMix/enderBooster/";
    private const string throwSoundPath = "event:/casju0_TrailMix/bow_shoot";
    private const string teleportSoundPathPrefix = "event:/casju0_TrailMix/teleport";

    enum States
    {
        Idle,
        Thrown,
        Respawning,
    }

    private bool shouldConsumeDash;
    private readonly Holdable hold;
    private readonly Sprite sprite;
    private readonly Wiggler wiggler;
    private readonly ParticleType particles;
    private float respawnTimer;
    private float noGravityTimer;
    private States state;
    private Vector2 respawnPosition;
    private Vector2 speed;
    private Vector2 prevLiftSpeed;

    public EnderBooster(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        shouldConsumeDash = data.Bool("shouldConsumeDash", false);
        Collider = new Hitbox(16, 16, -8, -16);

        Add(
            hold = new Holdable
            {
                OnPickup = HandlePickup,
                OnRelease = HandleRelease,
                OnHitSpring = HandleHitSpring,
                SlowRun = data.Bool("slowRun", true),
                SpeedGetter = () => speed,
                SpeedSetter = (v) => speed = v
            }
        );

        #region sprite setup
        Add(sprite = new Sprite(GFX.Game, spritePathPrefix));
        sprite.SetOrigin(16, 24);
        sprite.AddLoop(
            "idle",
            0f * 4,
            [.. GFX.Game.GetAtlasSubtextures($"{spritePathPrefix}idle")]
        );
        sprite.Add(
            "break",
            0.017f * 4,
            "end",
            [.. GFX.Game.GetAtlasSubtextures($"{spritePathPrefix}break")]
        );
        sprite.AddLoop("end", 0f, [.. GFX.Game.GetAtlasSubtextures($"{spritePathPrefix}end")]);
        Add(
            wiggler = Wiggler.Create(
                0.5f,
                4f,
                f =>
                {
                    sprite.Scale = Vector2.One * (1f + f * 0.25f);
                }
            )
        );
        sprite.Play("idle");
        #endregion

        respawnPosition = Position;

        particles = new ParticleType
        {
            LifeMin = 0.8f,
            LifeMax = 1f,
            Size = 1f,
            SizeRange = 0f,
            DirectionRange = MathF.PI * 2f,
            SpeedMin = 4f,
            SpeedMax = 8f,
            FadeMode = ParticleType.FadeModes.Late,
            Color = Calc.HexToColor("ee40ee"),
            Color2 = Calc.HexToColor("6c2378"),
            ColorMode = ParticleType.ColorModes.Fade,
        };
    }


    public override void Update()
    {
        base.Update();

        if (state != States.Respawning && Scene.OnInterval(0.085f))
        {
            SceneAs<Level>().ParticlesFG.Emit(particles, 1, Center, Vector2.One * 9f);
        }

        switch (state)
        {
            case States.Idle:
                break;
            case States.Thrown:
                bool collided = false;

                #region physics update
                if (hold.IsHeld)
                {
                    prevLiftSpeed = Vector2.Zero;
                }
                else
                {
                    if (hold.ShouldHaveGravity)
                    {
                        float num = 800f;
                        if (Math.Abs(speed.Y) <= 30f)
                        {
                            num *= 0.5f;
                        }
                        float num2 = 350f;
                        if (speed.Y < 0f)
                        {
                            num2 *= 0.5f;
                        }
                        speed.X = Calc.Approach(speed.X, 0f, num2 * Engine.DeltaTime);
                        if (noGravityTimer > 0f)
                        {
                            noGravityTimer -= Engine.DeltaTime;
                        }
                        else
                        {
                            speed.Y = Calc.Approach(speed.Y, 200f, num * Engine.DeltaTime);
                        }
                    }

                    MoveH(speed.X * Engine.DeltaTime, (coll) => { collided = true; });
                    MoveV(speed.Y * Engine.DeltaTime, (coll) =>
                    {
                        if (!(coll.Hit is Smw1f0))
                        {
                            collided = true;
                        }
                    });
                }
                hold.CheckAgainstColliders();
                #endregion

                #region teleport checks
                if (Input.DashPressed || collided)
                {
                    var player = Scene.Tracker.GetEntity<Player>();
                    if (player != null)
                    {
                        player.Center = Center;
                        if (Input.DashPressed)
                        {
                            if (!shouldConsumeDash)
                            {
                                player.Dashes += 1;
                            }
                            player.StateMachine.State = 2;
                        }
                        else if (collided)
                        {
                            player.Speed = Vector2.Zero;
                        }
                    }
                    Audio.Play(teleportSoundPathPrefix + Calc.Random.Range(1, 3));
                    sprite.Play("break");
                    Collidable = false;
                    respawnTimer = 1f;
                    state = States.Respawning;
                }
                #endregion
                break;
            case States.Respawning:
                respawnTimer -= Engine.DeltaTime;
                if (respawnTimer <= 0f)
                {
                    Position = respawnPosition;
                    Collidable = true;
                    wiggler.Start();
                    sprite.Play("idle");
                    state = States.Idle;
                }
                break;
        }
    }

    public override void Render()
    {
        s_outlineTexture.Draw(respawnPosition - sprite.Origin);
        if (state == States.Idle || state == States.Thrown)
        {
            sprite.DrawOutline();
        }
        base.Render();
    }

    private void HandlePickup()
    {
        AddTag(Tags.Persistent);
    }

    private void HandleRelease(Vector2 force)
    {
        state = States.Thrown;
        RemoveTag(Tags.Persistent);
        if (force.X != 0f && force.Y == 0f)
        {
            Audio.Play(throwSoundPath);
            force.Y = -0.4f;
        }
        speed = force * 200f;
        if (speed != Vector2.Zero)
        {
            noGravityTimer = 0.1f;
        }
    }

    private bool HandleHitSpring(Spring spring)
    {
        if (!hold.IsHeld)
        {
            if (spring.Orientation == Spring.Orientations.Floor && speed.Y >= 0f)
            {
                speed.X *= 0.5f;
                speed.Y = -160f;
                noGravityTimer = 0.15f;
                return true;
            }
            if (spring.Orientation == Spring.Orientations.WallLeft && speed.X <= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                speed.X = 220f;
                speed.Y = -80f;
                noGravityTimer = 0.1f;
                return true;
            }
            if (spring.Orientation == Spring.Orientations.WallRight && speed.X >= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f);
                speed.X = -220f;
                speed.Y = -80f;
                noGravityTimer = 0.1f;
                return true;
            }
        }
        return false;
    }
}
