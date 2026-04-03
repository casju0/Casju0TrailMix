using System.Collections.Generic;

namespace Celeste.Mod.Casju0TrailMix.Components;

[Tracked]
public class Grouper : Component
{
    public List<Grouper> Group;
    public bool GroupLeader;
    public Vector2 GroupOrigin;
    public int Index;
    public float XMin = float.MaxValue;
    public float XMax = float.MinValue;
    public float YMin = float.MaxValue;
    public float YMax = float.MinValue;

    public Grouper(int index) : base(false, false)
    {
        Index = index;
    }

    public override void EntityAwake()
    {
        if (Group == null)
        {
            GroupLeader = true;
            Group = new List<Grouper>();
            Group.Add(this);
            FindInGroup(this);
            foreach (Grouper item in Group)
            {
                if (item.Entity.Left < XMin)
                {
                    XMin = item.Entity.Left;
                }
                if (item.Entity.Right > XMax)
                {
                    XMax = item.Entity.Right;
                }
                if (item.Entity.Bottom > YMax)
                {
                    YMax = item.Entity.Bottom;
                }
                if (item.Entity.Top < YMin)
                {
                    YMin = item.Entity.Top;
                }
            }
            GroupOrigin = new Vector2((int)(XMin + (XMax - XMin) / 2f), (int)YMax);
            foreach (Grouper item2 in Group)
            {
                item2.GroupOrigin = GroupOrigin;
                item2.XMin = XMin;
                item2.XMax = XMax;
                item2.YMax = YMax;
                item2.YMin = YMin;
            }
        }
    }

    private void FindInGroup(Grouper self)
    {
        foreach (Grouper other in base.Scene.Tracker.GetComponents<Grouper>())
        {
            var eOther = other.Entity;
            var eSelf = self.Entity;
            if (other != this && other != self && other.Index == Index && (eOther.CollideRect(new Rectangle((int)eSelf.X - 1, (int)eSelf.Y, (int)eSelf.Width + 2, (int)eSelf.Height)) || eOther.CollideRect(new Rectangle((int)eSelf.X, (int)eSelf.Y - 1, (int)eSelf.Width, (int)eSelf.Height + 2))) && !Group.Contains(other))
            {
                Group.Add(other);
                FindInGroup(other);
                other.Group = Group;
            }
        }
    }
}
