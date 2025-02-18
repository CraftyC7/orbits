using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Reflection;

namespace orbits;
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class plugin : BaseUnityPlugin {
    public const string PluginGUID = "com.fiufki.orbits";
    public const string PluginName = "Orbits";
    public const string PluginVersion = "1.0.6";
    
    internal static ManualLogSource Logger;
    internal static AssetBundle orbitsAssets;
    
    private void Awake() {
        Logger = base.Logger;
        
        new Harmony(PluginGUID).PatchAll();
        
        var dirAssets = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "orbits");
        
        orbitsAssets = AssetBundle.LoadFromFile(dirAssets);
        if (orbitsAssets == null) {
            Logger.LogError("No assets for Orbits found. Not loading Orbits.");
            Destroy(this);
            return;
        }
        Logger.LogInfo($"{PluginName} is loaded.");
    }
    
    internal static Dictionary<int, (string ClipName, string Description)> LevelPatches = new() { 
    [0]  = (null, "POPULATION: Abandoned.\nCONDITIONS: Arid. Thick haze, worsened by industrial artifacts.\nFAUNA: Dominated by a few species."),
    [1]  = (null, "POPULATION: Abandoned.\nCONDITIONS: Jagged and weathered terrain with great foundation.\nFAUNA: Ecosystem supports territorial behaviour."),
    [2]  = ("MapView56Vow", "POPULATION: Abandoned.\nCONDITIONS: Humid. Rough terrain. Teeming with plant-life.\nFAUNA: A competitive ecosystem supports aggressive lifeforms."),
    [3]  = ("MapView71Gor", "POPULATION: Unknown.\nCONDITIONS: Continual storms. A complete water mass without any land masses. This is where The Company resides.\nFAUNA: Unknown."),
    [4]  = ("MapView61Mar", "POPULATION: Abandoned.\nCONDITIONS: Dense holt, expansive terrain. Persistent rain.\nFAUNA: Manifold of danger and valid ecosystem."),
    [5]  = ("MapView20Ada", "POPULATION: Abandoned.\nCONDITIONS: A landscape of deep valleys and mountains.\nFAUNA: Home to a lively, diverse ecosystem of smaller-sized omnivores."),
    [6]  = ("MapView85Ren", "POPULATION: None.\nCONDITIONS: Frozen, rocky. Deep mist causes adrift, hard to navigate.\nFAUNA: Orbiting a forlorn ecosystem."),
    [7]  = ("MapView7Din", "POPULATION: None.\nCONDITIONS: Orbits a white dwarf star. Prior monitoring, for security measures.\nFAUNA: Unlikely for complex life to exist."),
    [8]  = ("MapView21Off", "POPULATION: Abandoned.\nCONDITIONS: Rocky, bare foundry. With a ragged countryside.\nFAUNA: A ecosystem, gazes for survival."),
    [9]  = ("MapView8Tit", "POPULATION: None.\nCONDITIONS: Recent constant snow and hot temperatures underground. Thick smog.\nFAUNA: Danger upon every corner."),
    [10] = ("MapView68Art", "POPULATION: None.\nCONDITIONS: Waning forests. Abandoned facilities littered across the landscape.\nFAUNA: Rumored active machinery left behind."),
    [12] = ("MapView5Emb", "POPULATION: Abandoned.\nCONDITIONS: Desolate, mostly made out of amethyst and similar crystals.\nFAUNA: Devoided of biological life.")
    };
}

public class LevelPatchData {
    public string Description;
    public string ClipName;
}

[HarmonyPatch(typeof(RoundManager), "Start")]
public class RoundManager_Start_Patch {
    private static Dictionary<string, VideoClip> _cachedVideoClips = new Dictionary<string, VideoClip>();
    
    static void Postfix(RoundManager __instance) {
        var levels = StartOfRound.Instance?.levels;
        
        foreach (var level in levels)
        {
            if (plugin.LevelPatches.TryGetValue(level.levelID, out var patchData)) {
                level.LevelDescription = patchData.Description;
                
                if (!string.IsNullOrEmpty(patchData.ClipName)) {
                    if (!_cachedVideoClips.TryGetValue(patchData.ClipName, out var clip)) {
                        clip = plugin.orbitsAssets.LoadAsset<VideoClip>(patchData.ClipName);
                        if (clip != null) {
                            _cachedVideoClips[patchData.ClipName] = clip;
                        }
                    }
                    
                    level.videoReel = clip;
                } 
            } 
        }
        
        plugin.Logger.LogInfo($"Finished Orbits operations.");
    }
}