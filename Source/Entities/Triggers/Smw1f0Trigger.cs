namespace Celeste.Mod.Casju0TrailMix.Entities;

[CustomEntity("Casju0TrailMix/Smw1f0Trigger")]
class Smw1f0Trigger : Trigger
{
    static Smw1f0SubMenu Settings { get => Casju0TrailMixModule.Settings.Smw1f0Settings; }

    private float fallSpeed;

    public Smw1f0Trigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        fallSpeed = data.Float("fallSpeed", 250f);
        if (data.Bool("coverRoom", false))
        {
            ApplyChanges();
        }
    }

    public override void OnEnter(Player player)
    {
        ApplyChanges();
    }

    public void ApplyChanges()
    {
        Settings.FallSpeed = fallSpeed;
    }
}
