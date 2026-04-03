namespace Celeste.Mod.Casju0TrailMix.Entities;

[Tracked(false)]
[CustomEntity("Casju0TrailMix/Coin")]
public class Coin : Entity
{
    private readonly Hitbox hitbox;
    private readonly Sprite sprite;

    public Coin(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        var variant = data.Attr("variant", "yellow");
        Add(sprite = new Sprite(GFX.Game, "objects/Casju0TrailMix/coin/"));
        sprite.AddLoop(
            "idle",
            0.136f,
            GFX.Game.GetAtlasSubtextures($"objects/Casju0TrailMix/coin/{variant}").ToArray()
        );
        sprite.Add(
            "collect",
            0.017f,
            "done",
            GFX.Game.GetAtlasSubtextures($"objects/Casju0TrailMix/coin/sparkle").ToArray()
        );
        sprite.AddLoop(
            "done",
            0f,
            GFX.Game.GetAtlasSubtextures($"objects/Casju0TrailMix/coin/empty").ToArray()
        );
        sprite.SetOrigin(8f, 11f);
        sprite.Play("idle");

        Depth = Depths.Below;
        Collidable = true;
        Visible = true;
        Collider = hitbox = new Hitbox(12f, 16f, -6f, -8f);

        Add(new PlayerCollider(OnCollide, hitbox));
    }

    private void OnCollide(Player player)
    {
        Audio.Play("event:/casju0_TrailMix/smw_coin", Position);
        sprite.Play("collect");
        Collidable = false;
    }
}
