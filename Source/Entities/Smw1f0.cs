namespace Celeste.Mod.Casju0TrailMix.Entities;

[Tracked]
[CustomEntity("Casju0TrailMix/Smw1f0")]
public class Smw1f0 : JumpthruPlatform
{
    private class Smw1f0Slide : Component
    {
        public Smw1f0Slide() : base(true, false) { }
        public float Speed { get; set; }
        public override void Update()
        {
            var hold = Entity.Components.Get<Holdable>();
            if (hold != null && hold.SpeedSetter != null)
            {
                hold.SpeedSetter(new Vector2(Speed, Casju0TrailMixModule.Settings.Smw1f0Settings.FallSpeed));
            }
        }
    }

    public Smw1f0(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, "smw1f0", data.Int("surfaceIndex", -1))
    {
        Collidable = false;
    }

    public static void Load()
    {
        On.Celeste.Player.Pickup += HandlePickup;
        On.Celeste.Actor.MoveVExact += HandleMoveV;
        On.Celeste.Actor.MoveHExact += HandleMoveH;
    }

    public static void Unload()
    {
        On.Celeste.Player.Pickup -= HandlePickup;
        On.Celeste.Actor.MoveVExact -= HandleMoveV;
        On.Celeste.Actor.MoveHExact -= HandleMoveH;
    }

    private static bool HandlePickup(On.Celeste.Player.orig_Pickup orig, Player self, Holdable pickup)
    {
        pickup.Entity.Components.RemoveAll<Smw1f0Slide>();
        return orig(self, pickup);
    }

    private static bool HandleMoveV(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV, Collision onCollide, Solid pusher
    )
    {
        var hold = self.Components.Get<Holdable>();
        Vector2 prevSpeed;

        if (hold == null ||
            hold.SpeedGetter == null ||
            (prevSpeed = hold.SpeedGetter()).Y < 0)
        {
            return orig(self, moveV, onCollide, pusher);
        }
        else
        {
            var prevIsRiding = false;
            var nextIsRiding = false;
            var jumpThrus = self.Scene.Tracker.GetEntities<Smw1f0>();
            jumpThrus.ForEach(smw1f0 =>
                {
                    smw1f0.Collidable = true;
                    if (!prevIsRiding && self.IsRiding(smw1f0 as Smw1f0))
                    {
                        prevIsRiding = true;
                    }
                }
            );
            var t = orig(self, moveV, (coll) =>
                {
                    if (!(coll.Hit is Smw1f0 jumpthru))
                    {
                        onCollide?.Invoke(coll);
                    }
                },
                pusher
            );
            jumpThrus.ForEach(smw1f0 =>
                {
                    if (!nextIsRiding && self.IsRiding(smw1f0 as Smw1f0))
                    {
                        nextIsRiding = true;
                    }
                    smw1f0.Collidable = false;
                }
            );
            if (!prevIsRiding && nextIsRiding)
            {
                self.Components.Add(new Smw1f0Slide { Speed = prevSpeed.X });
            }
            return t;
        }
    }

    private static bool HandleMoveH(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher)
    {
        var hold = self.Components.Get<Holdable>();
        var slide = self.Components.Get<Smw1f0Slide>();

        if (slide == null || hold == null ||
            hold.SpeedGetter == null) { return orig(self, moveH, onCollide, pusher); }
        else
        {
            var isRiding = false;
            var jumpThrus = self.Scene.Tracker.GetEntities<Smw1f0>();
            jumpThrus.ForEach(smw1f0 =>
                {
                    smw1f0.Collidable = true;
                }
            );
            var t = orig(self, moveH, (coll) =>
                {
                    onCollide?.Invoke(coll);
                    slide.Speed = hold.SpeedGetter().X;
                },
                pusher
            );
            jumpThrus.ForEach(smw1f0 =>
                {
                    if (!isRiding && self.IsRiding(smw1f0 as Smw1f0))
                    {
                        isRiding = true;
                    }
                    smw1f0.Collidable = false;
                }
            );
            if (!isRiding)
            {
                self.Components.RemoveAll<Smw1f0Slide>();
            }
            return t;
        }
    }
}
