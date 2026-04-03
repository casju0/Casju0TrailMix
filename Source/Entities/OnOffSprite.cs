namespace Celeste.Mod.Casju0TrailMix.Entities;

[Tracked]
[CustomEntity("Casju0TrailMix/OnOffSprite")]
class OnOffSprite : Entity
{
    private static ulong lastFlipTime;

    private string flag;
    private bool onOffType;
    private Image activeImage;
    private Image inactiveImage;
    private string activationSound;

    private int prevColliderCount;
    private bool active;

    public OnOffSprite(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        flag = data.String("flag", "");
        onOffType = data.Bool("onOffType", true);

        Add(activeImage = new Image(GFX.Game[data.String("activeImage", "")]));
        Add(inactiveImage = new Image(GFX.Game[data.String("inactiveImage", "")]));
        activeImage.CenterOrigin();
        inactiveImage.CenterOrigin();
        activeImage.Visible = active;
        inactiveImage.Visible = !active;
        activationSound = data.String("activationAudio", "event:/casju0_TrailMix/smw_switch");

        Collider = new Hitbox(16, 16, -8, -8);

        Depth = Depths.Below;
    }

    public override void Awake(Scene scene)
    {
        active = SceneAs<Level>().Session.GetFlag(flag);
    }

    public override void Update()
    {
        if (Engine.FrameCounter == lastFlipTime)
        {
            return;
        }

        active = SceneAs<Level>().Session.GetFlag(flag) == onOffType;
        var colliderCount = (CollideCheck<Player>() ? 1 : 0) + CollideAllByComponent<Holdable>().Count;

        if (active || colliderCount == 0)
        {
            activeImage.Visible = active;
            inactiveImage.Visible = !active;
        }

        if (!active && prevColliderCount == 0 && colliderCount > 0)
        {
            SceneAs<Level>().Session.SetFlag(flag, onOffType);
            lastFlipTime = Engine.FrameCounter;
            Audio.Play(activationSound);

            foreach (OnOffSprite e in Scene.Tracker.GetEntities<OnOffSprite>())
            {
                e.active = e.onOffType == onOffType;
                if (e.active || e.prevColliderCount == 0)
                {
                    activeImage.Visible = e.active;
                    inactiveImage.Visible = !e.active;
                }
            }
        }

        prevColliderCount = colliderCount;
    }
}
