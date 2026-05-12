namespace Celeste.Mod.Casju0TrailMix.Entities;

[CustomEntity("Casju0TrailMix/FiveEddTrigger")]
public class FiveEddTrigger : Trigger
{
    readonly bool enabled;
    readonly bool revertOnLeave;

    public FiveEddTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        enabled = data.Bool("enabled", true);
        revertOnLeave = data.Bool("revertOnLeave", false);
        if (data.Bool("coverRoom", false))
        {
            ApplyChanges();
        }
    }

    private void ApplyChanges()
    {
        SceneAs<Level>().Session.SetFlag(FiveEddController.flag, enabled);
    }

    public override void OnEnter(Player player)
    {
        ApplyChanges();
    }

    public override void OnLeave(Player player)
    {
        if (revertOnLeave)
        {
            SceneAs<Level>().Session.SetFlag(FiveEddController.flag, false);
        }
    }
}
