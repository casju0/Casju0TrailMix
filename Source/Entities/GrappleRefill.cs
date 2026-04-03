using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.Casju0TrailMix.Entities;

[CustomEntity("Casju0TrailMix/GrappleRefill")]
public class GrappleRefill : Entity
{
    static GrappleSubMenu Settings { get => Casju0TrailMixModule.Settings.GrappleSettings; }

    private static readonly ParticleType P_Shatter = new(Refill.P_Shatter)
    {
        Color = Calc.HexToColor("d3e8ff"),
        Color2 = Calc.HexToColor("85b0fc"),
    };

    private static readonly ParticleType P_Regen = new(Refill.P_Regen)
    {
        Color = Calc.HexToColor("a5d1ff"),
        Color2 = Calc.HexToColor("6da0e0"),
    };

    private static readonly ParticleType P_Glow = new(Refill.P_Glow)
    {
        Color = Calc.HexToColor("a5d1ff"),
        Color2 = Calc.HexToColor("6da0e0"),
    };

    private readonly Sprite sprite;
    private readonly Sprite flash;
    private readonly Image outline;
    private readonly Wiggler wiggler;
    private readonly BloomPoint bloom;
    private readonly VertexLight light;
    private Level level;
    private readonly SineWave sine;
    private readonly bool oneUse;
    private readonly ParticleType p_shatter;
    private readonly ParticleType p_regen;
    private readonly ParticleType p_glow;
    private float respawnTimer;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public GrappleRefill(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        base.Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));
        oneUse = data.Bool("oneUse", false);
        string text;
        text = "objects/Casju0TrailMix/grappleRefill/";
        p_shatter = P_Shatter;
        p_regen = P_Regen;
        p_glow = P_Glow;
        Add(outline = new Image(GFX.Game["objects/refill/" + "outline"]));
        outline.CenterOrigin();
        outline.Visible = false;
        Add(sprite = new Sprite(GFX.Game, text + "idle"));
        sprite.AddLoop("idle", "", 0.1f);
        sprite.Play("idle");
        sprite.CenterOrigin();
        Add(flash = new Sprite(GFX.Game, "objects/refill/" + "flash"));
        flash.Add("flash", "", 0.05f);
        flash.OnFinish = delegate
        {
            flash.Visible = false;
        };
        flash.CenterOrigin();
        Add(
            wiggler = Wiggler.Create(
                1f,
                4f,
                [MethodImpl(MethodImplOptions.NoInlining)]
        (float v) =>
                {
                    sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
                }
            )
        );
        Add(new MirrorReflection());
        Add(bloom = new BloomPoint(0.8f, 16f));
        Add(light = new VertexLight(Color.White, 1f, 16, 48));
        Add(sine = new SineWave(0.6f, 0f));
        sine.Randomize();
        UpdateY();
        base.Depth = -100;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
        level = SceneAs<Level>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                Respawn();
            }
        }
        else if (base.Scene.OnInterval(0.1f))
        {
            level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
        }
        UpdateY();
        light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
        bloom.Alpha = light.Alpha * 0.8f;
        if (base.Scene.OnInterval(2f) && sprite.Visible)
        {
            flash.Play("flash", restart: true);
            flash.Visible = true;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Respawn()
    {
        if (!Collidable)
        {
            Collidable = true;
            sprite.Visible = true;
            outline.Visible = false;
            base.Depth = -100;
            wiggler.Start();
            Audio.Play("event:/game/general/diamond_return", Position);
            level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void UpdateY()
    {
        Sprite obj = flash;
        Sprite obj2 = sprite;
        float num = (bloom.Y = sine.Value * 2f);
        float y = (obj2.Y = num);
        obj.Y = y;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        if (sprite.Visible)
        {
            sprite.DrawOutline();
        }
        base.Render();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void OnPlayer(Player player)
    {
        if (GrappleHook.HasAmmo)
        {
            if (Settings.InventoryType == GrappleSubMenu.InventoryTypes.RequiresRefill)
            {
                GrappleHook.Grapple.Refilled = true;
            }
            Audio.Play("event:/game/general/diamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            Add(new Coroutine(RefillRoutine(player)));
            respawnTimer = 2.5f;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator RefillRoutine(Player player)
    {
        Celeste.Freeze(0.05f);
        yield return null;
        level.Shake();
        Sprite obj = sprite;
        Sprite obj2 = flash;
        bool visible = false;
        obj2.Visible = false;
        obj.Visible = visible;
        if (!oneUse)
        {
            outline.Visible = true;
        }
        Depth = 8999;
        yield return 0.05f;
        float num = player.Speed.Angle();
        level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
        level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
        SlashFx.Burst(Position, num);
        if (oneUse)
        {
            RemoveSelf();
        }
    }
}
