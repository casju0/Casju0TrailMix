using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework.Graphics;

class SierpinskiFiveGon : Backdrop
{
    struct CustomVertex(float id, float index) : IVertexType
    {
        // each pentagon in the fractal has it's own ID.
        // using the id, we can determine where the position of each pentagon will be.
        // in the vertex buffer, there will be 5 vertices per ID:
        // each of those vertices will describe which point of the pentagon they are through the Index.
        public float Id = id;
        // for each ID, there are 5 indices ranging from 0 to 4.
        // the index describes which point of the pentagon that vertex should represent.
        public float Index = index;

        // the vertex buffer for the first 10 vertices will look like this:
        // Id, Index
        // [
        // 0, 0, <- start of first pentagon (ID 0)
        // 0, 1,
        // 0, 2,
        // 0, 3,
        // 0, 4,
        // 1, 0, <- start of second pentagon (ID 1)
        // 1, 1,
        // 1, 2,
        // 1, 3,
        // 1, 4
        // ... (and so on)
        // ]
        // 

        readonly VertexDeclaration IVertexType.VertexDeclaration => new(
            new VertexElement(0, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(4, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1)
        );

        public override readonly string ToString()
        {
            return $"{Id}:{Index}";
        }
    }

    private static CustomVertex[] vertices;
    private static int[] indices;
    private readonly Effect effect;
    const int TARGET_ITER = 3;

    public static void Load()
    {
        List<CustomVertex> vertices = [];
        List<int> indices = [];
        int j = 0;
        for (int i = 0; i < Math.Pow(6, TARGET_ITER); i += 1)
        {
            if (i % 6 == 0) { continue; } // coring the apple
            vertices.AddRange([
                new CustomVertex(i, 0),
                new CustomVertex(i, 1),
                new CustomVertex(i, 2),
                new CustomVertex(i, 3),
                new CustomVertex(i, 4),
            ]);

            // the indices makes a triangle fan out of the 5 vertices above to construct a pentagon.
            indices.AddRange([
                j, j + 1, j + 2, // first triangle
                j, j + 2, j + 3, // second triangle
                j, j + 3, j + 4, // third triangle
            ]);
            j += 5;
        }
        SierpinskiFiveGon.vertices = [.. vertices];
        SierpinskiFiveGon.indices = [.. indices];
    }

    public static void Unload()
    {
        vertices = [];
        indices = [];
    }

    public SierpinskiFiveGon()
    {
        if (Everest.Content.TryGet($"Effects/Casju0TrailMix/sierpinskiFiveGon.cso", out var effectAsset, true))
        {
            effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset.Data);
            effect.Parameters["zoom"].SetValue(1f);
            effect.Parameters["skid"].SetValue(0f);
            effect.Parameters["theta"].SetValue((float)(Math.Tau * 0.1));
        }
        else
        {
            throw new Exception("Failed to create the Sierpinski 5-gon effect!");
        }
    }

    public override void Render(Scene scene)
    {
        base.Render(scene);
        Renderer.EndSpritebatch();

        float zoom = 1f;
        float skid = 0f;
        float theta = 0;

        Level level = (Level)scene;

        if (level.Session.GetFlag("casju0TrailMix__5gonA"))
        {
            // slow zoom
            float t = (scene.TimeActive / 6) % 1;
            zoom = (float)Math.Pow(2.618, WrapToRange(0, 2, -t * 2) + 1);
        }
        else if (level.Session.GetFlag("casju0TrailMix__5gonB"))
        {
            // slow rotating
            float t = (scene.TimeActive / 6) % 1;
            zoom = (float)Math.Pow(2.618, WrapToRange(0, 2, -t * 2) + 1);
            theta = WrapToRange(0, (float)(Math.Tau * 0.4), t * (float)(Math.Tau * 0.4));
        }
        else if (level.Session.GetFlag("casju0TrailMix__5gonC"))
        {
            // small skid slow
            float t = (scene.TimeActive / 6) % 1;
            zoom = (float)Math.Pow(2.618, WrapToRange(0, 2, -t * 2) + 1);
            skid = t;
        }
        else if (level.Session.GetFlag("casju0TrailMix__5gonD"))
        {
            // large skid slow
            float t = (scene.TimeActive / 6) % 1;
            zoom = (float)Math.Pow(2.618, WrapToRange(0, 2, -t * 2) + 1);
            skid = WrapToRange(-1.618f, 2.618f, (float)(t * (1.618 + 2.618)));
        }
        else if (level.Session.GetFlag("casju0TrailMix__5gonE"))
        {
            // small skid fast
            float t = (scene.TimeActive / 2) % 1;
            zoom = (float)Math.Pow(2.618, WrapToRange(0, 2, -t * 2) + 1);
            skid = t;
        }
        else if (level.Session.GetFlag("casju0TrailMix__5gonF"))
        {
            // large skid fast
            float t = (scene.TimeActive / 2) % 1;
            zoom = (float)Math.Pow(2.618, WrapToRange(0, 2, -t * 2) + 1);
            skid = WrapToRange(-1.618f, 2.618f, (float)(t * (1.618 + 2.618)));
        }

        for (int i = 0; i < 6; i += 1)
        {
            effect.Parameters["zoom"].SetValue(zoom / (float)Math.Pow(2.618, i));
            effect.Parameters["skid"].SetValue(skid);
            effect.Parameters["theta"].SetValue((float)(theta + Math.Tau * 0.05f + i * Math.Tau * 0.1f));
            GFX.DrawIndexedVertices(Renderer.Matrix, vertices, vertices.Length, indices, indices.Length / 3, effect, BlendState.AlphaBlend);
        }
        Audio.CurrentMusicEventInstance?.getTimelinePosition(out int pos);
        Renderer.StartSpritebatch(BlendState.AlphaBlend);
    }

    private static float easeInSine(float x)
    {
        return (float)(1 - Math.Cos(x * Math.PI / 2));
    }

    private static float easeInBack(float x)
    {
        var c1 = 1.70158;
        var c3 = c1 + 1;

        return (float)(c3 * x * x * x - c1 * x * x);

    }

    private static float easeInOutBack(float x)
    {
        var c1 = 1.70158;
        var c2 = c1 * 1.525;

        return (float)(x < 0.5
          ? (Math.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
          : (Math.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2);

    }

    private static float easeOutCirc(float x)
    {
        return (float)Math.Sqrt(1 - Math.Pow(x - 1, 2));

    }

    private static float WrapToRange(float a, float b, float v)
    {
        var range = b - a;
        var tmp = (v - a) % range;
        if (tmp < 0)
        {
            tmp += range;
        }
        return tmp + a;
    }
}
