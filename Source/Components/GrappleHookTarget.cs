using Celeste.Mod.Casju0TrailMix.Components;

namespace Celeste.Mod.Casju0TrailMix.Entities;

[Tracked]
[CustomEntity("Casju0TrailMix/GrappleHookTarget")]
public class GrappleHookTarget : Entity
{
    public static readonly MTexture mTexture = GFX.Game["objects/Casju0TrailMix/grappleHookTarget"];
    public Hitbox Hitbox { get; private set; }

    public GrappleHookTarget(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        var image = new Image(mTexture);
        image.SetOrigin(8, 8);
        Add(image);

        Collidable = true;
        Visible = true;
        Hitbox = new Hitbox(8, 8, -4, -4);
        Collider = Hitbox;

        Add(new GrappleSolid());
    }
}
