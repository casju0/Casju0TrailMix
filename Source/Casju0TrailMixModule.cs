global using Celeste.Mod.Entities;
global using Microsoft.Xna.Framework;
global using Monocle;
using System;
using Celeste.Mod.Casju0TrailMix.Entities;
using MonoMod.ModInterop;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.Casju0TrailMix;

[ModImportName("FemtoHelper")]
public static class FemtoHelperImports
{
    public static Func<int, int, Action<Vector2>, Action<Vector2>, Holdable> CreateSmwHoldable;
}

public class Casju0TrailMixModule : EverestModule
{
    public static Casju0TrailMixModule Instance { get; private set; }

    public override Type SettingsType => typeof(Casju0TrailMixModuleSettings);
    public static Casju0TrailMixModuleSettings Settings => (Casju0TrailMixModuleSettings)Instance._Settings;

    public override Type SessionType => typeof(Casju0TrailMixModuleSession);
    public static Casju0TrailMixModuleSession Session => (Casju0TrailMixModuleSession)Instance._Session;

    public override Type SaveDataType => typeof(Casju0TrailMixModuleSaveData);
    public static Casju0TrailMixModuleSaveData SaveData => (Casju0TrailMixModuleSaveData)Instance._SaveData;

    public static ILHook PlayerOrigUpdateHook { get; private set; }

    public static SpriteBank CTMSpriteBank;

    public Casju0TrailMixModule()
    {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(Casju0TrailMixModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(Casju0TrailMixModule), LogLevel.Info);
#endif
    }

    public override void Load()
    {
        typeof(Casju0TrailMixExports).ModInterop();
        typeof(FemtoHelperImports).ModInterop();
        ThrowBlock.Load();
        GrappleHook.Load();
        Smw1f0.Load();
        TechlessController.Load();
        SuperBounceController.Load();
        WaterPhysicsController.Load();
    }

    public override void Unload()
    {
        ThrowBlock.Unload();
        GrappleHook.Unload();
        Smw1f0.Unload();
        TechlessController.Unload();
        SuperBounceController.Unload();
        WaterPhysicsController.Unload();
    }

    // Optional, do anything requiring either the Celeste or mod content here. 
    // Usually involves Spritebanks or custom particle effects.
    public override void LoadContent(bool firstLoad)
    {
        base.LoadContent(firstLoad);
        // CTMSpriteBank = new SpriteBank(GFX.Game, "Graphics/Casju0TrailMix/Sprites.xml");
    }
}
