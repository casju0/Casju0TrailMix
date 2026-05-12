
namespace Celeste.Mod.Casju0TrailMix.Entities;

[Tracked]
[CustomEntity("Casju0TrailMix/SpaceWrappedFallingBlock")]
public class SpaceWrappedFallingBlock(EntityData data, Vector2 offset) : FallingBlock(data, offset)
{
    public static void Load()
    {
        On.Celeste.SpaceController.Update += HandleSpaceUpdate;
    }

    public static void Unload()
    {
        On.Celeste.SpaceController.Update -= HandleSpaceUpdate;
    }

    private static void HandleSpaceUpdate(On.Celeste.SpaceController.orig_Update orig, SpaceController self)
    {
        orig(self);
        foreach (SpaceWrappedFallingBlock entity in self.Scene.Tracker.GetEntities<SpaceWrappedFallingBlock>())
        {
            if (entity != null)
            {
                if (entity.Top > self.level.Camera.Bottom + 12f)
                {
                    var prevPosition = entity.Position;
                    entity.Bottom = self.level.Camera.Top - 4f;
                    var delta = entity.Position - prevPosition;
                    foreach (var child in entity.staticMovers)
                    {
                        child.Entity.Position += delta;
                    }
                }
                else if (entity.Bottom < self.level.Camera.Top - 4f)
                {
                    var prevPosition = entity.Position;
                    entity.Top = self.level.Camera.Bottom + 12f;
                    var delta = entity.Position - prevPosition;
                    foreach (var child in entity.staticMovers)
                    {
                        child.Entity.Position += delta;
                    }
                }
            }
        }
    }
}