using System;

namespace Celeste.Mod.Casju0TrailMix.Components;

[Tracked]
public class GrappleHoldable : Component
{
    public Hitbox Collider;
    public Action<int> OnGrapplePickup;
    public Action<int> OnGrappleRelease;

    public GrappleHoldable()
        : base(true, false) { }
}
