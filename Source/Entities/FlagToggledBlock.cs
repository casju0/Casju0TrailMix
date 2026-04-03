using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Celeste.Mod.Casju0TrailMix.Components;

namespace Celeste.Mod.Casju0TrailMix.Entities;

[Tracked]
[CustomEntity("Casju0TrailMix/FlagToggledBlock")]
public class FlagToggledBlock : Solid
{
    public int Index;
    public bool Activated;

    private string flag;
    private bool inverted;

    private Color activeColor;
    private Color inactiveColor;
    private MTexture activeTexture;
    private MTexture inactiveTexture;
    private List<Image> pressed = new List<Image>();
    private List<Image> solid = new List<Image>();
    private List<Image> all = new List<Image>();

    private LightOcclude occluder;
    private Wiggler wiggler;
    private Vector2 wigglerScaler;
    private Grouper grouper;
    private ThirteenTiler thirteenTiler;

    public FlagToggledBlock(Vector2 position, float width, float height, int index)
        : base(position, width, height, safe: false)
    {
        SurfaceSoundIndex = 35;
        Index = index;
        Collidable = false;
        Add(occluder = new LightOcclude());
        Add(grouper = new Grouper(index));
        Add(thirteenTiler = new ThirteenTiler(typeof(FlagToggledBlock), index));
    }

    public FlagToggledBlock(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Int("index"))
    {
        flag = data.String("flag", "");
        inverted = data.Bool("inverted", false);
        activeColor = data.HexColor("activeColor", Color.Magenta);
        inactiveColor = data.HexColor("inactiveColor", Color.DarkMagenta);
        activeTexture = GFX.Game[data.String("activeTexture", "objects/cassetteblock/solid")];
        inactiveTexture = GFX.Game[data.String("inactiveTexture", "objects/cassetteblock/pressed00")];
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach (StaticMover staticMover in staticMovers)
        {
            if (staticMover.Entity is Spikes spikes)
            {
                spikes.EnabledColor = activeColor;
                spikes.DisabledColor = inactiveColor;
                spikes.VisibleWhenDisabled = true;
                spikes.SetSpikeColor(this.activeColor);
            }
            if (staticMover.Entity is Spring spring)
            {
                spring.DisabledColor = inactiveColor;
                spring.VisibleWhenDisabled = true;
            }
        }

        if (grouper.GroupLeader)
        {
            wigglerScaler = new Vector2(Calc.ClampedMap(grouper.XMax - grouper.XMin, 32f, 96f, 1f, 0.2f), Calc.ClampedMap(grouper.YMax - grouper.YMin, 32f, 96f, 1f, 0.2f));
            Add(wiggler = Wiggler.Create(0.3f, 3f));
            foreach (Grouper g in grouper.Group)
            {
                (g.Entity as FlagToggledBlock).wiggler = wiggler;
                (g.Entity as FlagToggledBlock).wigglerScaler = wigglerScaler;
                g.GroupOrigin = grouper.GroupOrigin;
            }
        }

        foreach (StaticMover staticMover2 in staticMovers)
        {
            if (staticMover2.Entity is Spikes spikes2)
            {
                spikes2.SetOrigins(grouper.GroupOrigin);
            }
        }

        foreach (var (x, y, tx, ty) in thirteenTiler.ImageCoords)
        {
            pressed.Add(CreateImage(x, y, tx, ty, inactiveTexture));
            solid.Add(CreateImage(x, y, tx, ty, activeTexture));
        }

        if (!Collidable)
        {
            DisableStaticMovers();
        }
        UpdateVisualState();
    }

    private Image CreateImage(float x, float y, int tx, int ty, MTexture tex)
    {
        Vector2 vector = new Vector2(x - X, y - Y);
        Image image = new Image(tex.GetSubtexture(tx * 8, ty * 8, 8, 8));
        Vector2 vector2 = grouper.GroupOrigin - Position;
        image.Origin = vector2 - vector;
        image.Position = vector2;
        image.Color = activeColor;
        Add(image);
        all.Add(image);
        return image;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        Activated = SceneAs<Level>().Session.GetFlag(flag) == inverted;

        if (grouper.GroupLeader && Activated && !Collidable)
        {
            bool blocked = false;
            foreach (Grouper g in grouper.Group)
            {
                if ((g.Entity as FlagToggledBlock).BlockedCheck())
                {
                    blocked = true;
                    break;
                }
            }
            if (!blocked)
            {
                foreach (Grouper g in grouper.Group)
                {
                    (g.Entity as FlagToggledBlock).Collidable = true;
                    (g.Entity as FlagToggledBlock).EnableStaticMovers();
                }
                wiggler.Start();
            }
        }
        else if (!Activated && Collidable)
        {
            Collidable = false;
            DisableStaticMovers();
        }
        UpdateVisualState();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool BlockedCheck()
    {
        TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
        if (theoCrystal != null && !TryActorWiggleUp(theoCrystal))
        {
            return true;
        }
        Player player = CollideFirst<Player>();
        if (player != null && !TryActorWiggleUp(player))
        {
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void UpdateVisualState()
    {
        if (!Collidable)
        {
            Depth = 8990;
        }
        else
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null && entity.Top >= Bottom - 1f)
            {
                Depth = 10;
            }
            else
            {
                Depth = -10;
            }
        }
        foreach (StaticMover staticMover in staticMovers)
        {
            staticMover.Entity.Depth = Depth + 1;
        }
        occluder.Visible = Collidable;
        foreach (Image image in solid)
        {
            image.Visible = Collidable;
        }
        foreach (Image image in pressed)
        {
            image.Visible = !Collidable;
        }
        if (!grouper.GroupLeader)
        {
            return;
        }
        Vector2 scale = new Vector2(1f + wiggler.Value * 0.05f * wigglerScaler.X, 1f + wiggler.Value * 0.15f * wigglerScaler.Y);
        foreach (Grouper g in grouper.Group)
        {
            foreach (Image image in (g.Entity as FlagToggledBlock).all)
            {
                image.Scale = scale;
            }
            foreach (StaticMover staticMover2 in (g.Entity as FlagToggledBlock).staticMovers)
            {
                if (!(staticMover2.Entity is Spikes spikes))
                {
                    continue;
                }
                foreach (Component component in spikes.Components)
                {
                    if (component is Image image)
                    {
                        image.Scale = scale;
                    }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryActorWiggleUp(Entity actor)
    {
        foreach (Grouper g in grouper.Group)
        {
            if (g.Entity != this && g.Entity.CollideCheck(actor, g.Entity.Position + Vector2.UnitY * 4f))
            {
                return false;
            }
        }
        bool collidable = Collidable;
        Collidable = true;
        for (int i = 1; i <= 4; i++)
        {
            if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * i))
            {
                actor.Position -= Vector2.UnitY * i;
                Collidable = collidable;
                return true;
            }
        }
        Collidable = collidable;
        return false;
    }
}
