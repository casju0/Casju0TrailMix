namespace Celeste.Mod.Casju0TrailMix.Entities;

[CustomEntity("Casju0TrailMix/OffscreenPlayerIndicator")]
public class OffscreenPlayerIndicator : Entity
{
    public static readonly MTexture mTexture = GFX.Game["objects/Casju0TrailMix/offscreenPlayerIndicator"];

    private string visibleFlag;
    private bool inverted;
    private float padding;
    private bool spaceMode;

    public OffscreenPlayerIndicator(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Depth = Depths.Top;
        visibleFlag = data.String("visibleFlag", "");
        inverted = data.Bool("inverted", false);
        padding = data.Float("padding", -1f);
        spaceMode = data.Bool("spaceMode", true);
    }

    public override void Render()
    {
        base.Render();
        if (SceneAs<Level>().Session.GetFlag(visibleFlag) == !inverted)
        {
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                var camera = player.SceneAs<Level>().Camera;
                var dashColor = player.Dashes >= 2 ? Color.Magenta : player.Dashes == 1 ? Color.Red : Color.CornflowerBlue;

                if (padding < 0 || camera.Top >= player.Bottom || (spaceMode && player.Bottom >= camera.Bottom - padding))
                {
                    mTexture.DrawCentered(new Vector2(player.Center.X, camera.Top + 7.5f), dashColor);
                }
                if (padding < 0 || camera.Bottom <= player.Top || (spaceMode && player.Top <= camera.Top + padding))
                {
                    mTexture.DrawCentered(new Vector2(player.Center.X, camera.Bottom - 7.5f), dashColor);
                }
            }
        }
    }
}