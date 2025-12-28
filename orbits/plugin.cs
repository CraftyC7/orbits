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
    public const string PluginGUID = "com.crafty.orbits";
    public const string PluginName = "Orbits Video Only";
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
    
    internal static Dictionary<int, string> LevelPatches = new() { 
    [0]  = (null),
    [1]  = (null),
    [2]  = ("MapView56Vow"),
    [3]  = ("MapView71Gor"),
    [4]  = ("MapView61Mar"),
    [5]  = ("MapView20Ada"),
    [6]  = ("MapView85Ren"),
    [7]  = ("MapView7Din"),
    [8]  = ("MapView21Off"),
    [9]  = ("MapView8Tit"),
    [10] = ("MapView68Art"),
    [12] = ("MapView5Emb")
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
                if (!string.IsNullOrEmpty(patchData)) {
                    if (!_cachedVideoClips.TryGetValue(patchData, out var clip)) {
                        clip = plugin.orbitsAssets.LoadAsset<VideoClip>(patchData);
                        if (clip != null) {
                            _cachedVideoClips[patchData] = clip;
                        }
                    }
                    
                    level.videoReel = clip;
                } 
            } 
        }
        
        plugin.Logger.LogInfo($"Finished Orbits operations.");
    }
}
