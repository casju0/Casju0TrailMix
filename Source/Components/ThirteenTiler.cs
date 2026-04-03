using System;
using System.Collections.Generic;

namespace Celeste.Mod.Casju0TrailMix.Components;

[Tracked]
public class ThirteenTiler : Component
{
    public Type Kind;
    public int Index;
    public List<(float, float, int, int)> ImageCoords = new();

    public ThirteenTiler(Type kind, int index) : base(false, false)
    {
        Kind = kind;
        Index = index;
    }

    public override void EntityAwake()
    {
        for (float x = base.Entity.Left; x < base.Entity.Right; x += 8f)
        {
            for (float y = base.Entity.Top; y < base.Entity.Bottom; y += 8f)
            {
                bool l = CheckForSame(x - 8f, y);
                bool r = CheckForSame(x + 8f, y);
                bool u = CheckForSame(x, y - 8f);
                bool d = CheckForSame(x, y + 8f);
                if (l && r && u && d)
                {
                    if (!CheckForSame(x + 8f, y - 8f))
                    {
                        ImageCoords.Add((x, y, 3, 0));
                    }
                    else if (!CheckForSame(x - 8f, y - 8f))
                    {
                        ImageCoords.Add((x, y, 3, 1));
                    }
                    else if (!CheckForSame(x + 8f, y + 8f))
                    {
                        ImageCoords.Add((x, y, 3, 2));
                    }
                    else if (!CheckForSame(x - 8f, y + 8f))
                    {
                        ImageCoords.Add((x, y, 3, 3));
                    }
                    else
                    {
                        ImageCoords.Add((x, y, 1, 1));
                    }
                }
                else if (l && r && !u && d)
                {
                    ImageCoords.Add((x, y, 1, 0));
                }
                else if (l && r && u && !d)
                {
                    ImageCoords.Add((x, y, 1, 2));
                }
                else if (l && !r && u && d)
                {
                    ImageCoords.Add((x, y, 2, 1));
                }
                else if (!l && r && u && d)
                {
                    ImageCoords.Add((x, y, 0, 1));
                }
                else if (l && !r && !u && d)
                {
                    ImageCoords.Add((x, y, 2, 0));
                }
                else if (!l && r && !u && d)
                {
                    ImageCoords.Add((x, y, 0, 0));
                }
                else if (l && !r && u && !d)
                {
                    ImageCoords.Add((x, y, 2, 2));
                }
                else if (!l && r && u && !d)
                {
                    ImageCoords.Add((x, y, 0, 2));
                }
            }
        }
    }

    private bool CheckForSame(float x, float y)
    {
        foreach (ThirteenTiler component in base.Scene.Tracker.GetComponents<ThirteenTiler>())
        {
            if (Kind == component.Entity.GetType() && component.Index == Index && component.Entity.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8)))
            {
                return true;
            }
        }
        return false;
    }
}
