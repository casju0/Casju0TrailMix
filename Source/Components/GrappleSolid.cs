using System;

namespace Celeste.Mod.Casju0TrailMix.Components;

[Tracked]
public class GrappleSolid : Component
{
    public Hitbox Collider;
    public Action<int> OnGrappleAttach;
    public Action<int> OnGrappleRelease;

    public GrappleSolid()
        : base(true, false) { }
}
