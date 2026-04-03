using System;
using Celeste.Mod.Casju0TrailMix.Components;
using Celeste.Mod.Casju0TrailMix.Entities;
using MonoMod.ModInterop;

namespace Celeste.Mod.Casju0TrailMix;

[ModExportName("Casju0TrailMix")]
public static class Casju0TrailMixExports
{
    public static bool IsSmw1f0(Entity entity)
    {
        return entity is Smw1f0;
    }

    public static bool IsRidingSmw1f0(Actor actor)
    {
        var t = false;
        foreach (Smw1f0 jumpthru in actor.Scene.Tracker.GetEntities<Smw1f0>())
        {
            jumpthru.Collidable = true;
            if (actor.IsRiding(jumpthru))
            {
                t = true;
            }
            jumpthru.Collidable = false;
            if (t) { break; }
        }
        return t;
    }

    public static Component AddGrappleSolidComponent(Entity entity)
    {
        var component = new GrappleSolid();
        entity.Add(component);
        return component;
    }

    public static Component AddGrappleHoldableComponent(
        Entity entity,
        Hitbox hitbox,
        Action<int> onGrapplePickup,
        Action<int> onGrappleRelease
    )
    {
        var component = new GrappleHoldable
        {
            Collider = hitbox,
            OnGrapplePickup = onGrapplePickup,
            OnGrappleRelease = onGrappleRelease,
        };
        entity.Add(component);
        return component;
    }

    public static Component AddGrouperComponent(Entity entity, int index)
    {
        var component = new Grouper(index);
        entity.Add(component);
        return component;
    }

    public static Component AddThirteenTilerComponent(Entity entity, Type kind, int index)
    {
        var component = new ThirteenTiler(kind, index);
        entity.Add(component);
        return component;
    }
}
