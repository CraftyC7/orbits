﻿using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Reflection;

namespace orbits
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.fiufki.orbits";
        public const string PluginName = "Orbits";
        public const string PluginVersion = "1.0.6";

        internal static ManualLogSource Log;
        internal static AssetBundle OrbitsBundle;

        private void Awake()
        {
            Log = base.Logger;
            Log.LogInfo($"{PluginName} is loaded.");

            try
            {
                var harmony = new Harmony(PluginGUID);
                harmony.PatchAll();
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to initialize Harmony patches: {ex}");
            }
            
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string bundlePath = Path.Combine(folderPath, "orbits");

            OrbitsBundle = AssetBundle.LoadFromFile(bundlePath);
            if (OrbitsBundle == null)
            {
                Log.LogError("Failed to load assets from " + bundlePath + ".");
            }
        }
        
        internal static Dictionary<int, LevelPatchData> LevelPatches = new Dictionary<int, LevelPatchData>
        {
            {
                0, new LevelPatchData
                {
                    Description = "POPULATION: Abandoned.\nCONDITIONS: Arid. Thick haze, worsened by industrial artifacts.\nFAUNA: Dominated by a few species.",
                    ClipName    = null
                }
            },
            {
                1, new LevelPatchData
                {
                    Description = "POPULATION: Abandoned.\nCONDITIONS: Jagged and weathered terrain with great foundation.\nFAUNA: Ecosystem supports territorial behaviour.",
                    ClipName    = null
                }
            },
            {
                2, new LevelPatchData
                {
                    Description = "POPULATION: Abandoned.\nCONDITIONS: Humid. Rough terrain. Teeming with plant-life.\nFAUNA: A competitive ecosystem supports aggressive lifeforms.",
                    ClipName    = "MapView56Vow"
                }
            },
            {
                3, new LevelPatchData
                {
                    Description = "POPULATION: Unknown.\nCONDITIONS: Continual storms. A complete water mass without any land masses. This is where The Company resides.\nFAUNA: Unknown.",
                    ClipName    = "MapView71Gor"
                }
            },
            {
                4, new LevelPatchData
                {
                    Description = "POPULATION: Abandoned.\nCONDITIONS: Dense holt, expansive terrain. Persistent rain.\nFAUNA: Manifold of danger and valid ecosystem.",
                    ClipName    = "MapView61Mar"
                }
            },
            {
                5, new LevelPatchData
                {
                    Description = "POPULATION: Abandoned.\nCONDITIONS: A landscape of deep valleys and mountains.\nFAUNA: Home to a lively, diverse ecosystem of smaller-sized omnivores.",
                    ClipName    = "MapView20Ada"
                }
            },
            {
                6, new LevelPatchData
                {
                    Description = "POPULATION: None.\nCONDITIONS: Frozen, rocky. Deep mist causes adrift, hard to navigate.\nFAUNA: Orbiting a forlorn ecosystem.",
                    ClipName    = "MapView85Ren"
                }
            },
            {
                7, new LevelPatchData
                {
                    Description = "POPULATION: None.\nCONDITIONS: Orbits a white dwarf star. Prior monitoring, for security measures.\nFAUNA: Unlikely for complex life to exist.",
                    ClipName    = "MapView7Din"
                }
            },
            {
                8, new LevelPatchData
                {
                    Description = "POPULATION: Abandoned.\nCONDITIONS: Rocky, bare foundry. With a ragged countryside.\nFAUNA: A ecosystem, gazes for survival.",
                    ClipName    = "MapView21Off"
                }
            },
            {
                9, new LevelPatchData
                {
                    Description = "POPULATION: None.\nCONDITIONS: Recent constant snow and hot temperatures underground. Thick smog.\nFAUNA: Danger upon every corner.",
                    ClipName    = "MapView8Tit"
                }
            },
            {
                10, new LevelPatchData
                {
                    Description = "POPULATION: None.\nCONDITIONS: Waning forests. Abandoned facilities littered across the landscape.\nFAUNA: Rumored active machinery left behind.",
                    ClipName    = "MapView68Art"
                }
            },
            {
                12, new LevelPatchData
                {
                    Description = "POPULATION: Abandoned.\nCONDITIONS: Desolate, mostly made out of amethyst and similar crystals.\nFAUNA: Devoided of biological life.",
                    ClipName    = "MapView5Emb"
                }
            }
        };
    }

    public class LevelPatchData
    {
        public string Description;
        public string ClipName;
    }

    [HarmonyPatch(typeof(RoundManager), "Start")]
    public class RoundManager_Start_Patch
    {
        private static Dictionary<string, VideoClip> _cachedVideoClips = new Dictionary<string, VideoClip>();

        static void Postfix(RoundManager __instance)
        {
            try
            {
                if (plugin.OrbitsBundle == null)
                {
                    plugin.Log.LogWarning("AssetBundle not loaded; skipping level updates.");
                    return;
                }

                var levels = StartOfRound.Instance?.levels;
                if (levels == null)
                {
                    plugin.Log.LogWarning("No levels found; skipping updates.");
                    return;
                }

                foreach (var level in levels)
                {
                    if (plugin.LevelPatches.TryGetValue(level.levelID, out var patchData))
                    {
                        level.LevelDescription = patchData.Description;
                        plugin.Log.LogInfo($"Updated description of level ID {level.levelID}.");
                        
                        if (!string.IsNullOrEmpty(patchData.ClipName))
                        {
                            if (!_cachedVideoClips.TryGetValue(patchData.ClipName, out var clip))
                            {
                                clip = plugin.OrbitsBundle.LoadAsset<VideoClip>(patchData.ClipName);
                                if (clip != null)
                                {
                                    _cachedVideoClips[patchData.ClipName] = clip;
                                }
                            }

                            if (clip != null)
                            {
                                level.videoReel = clip;
                                plugin.Log.LogInfo($"Updated the video reel for level ID {level.levelID}.");
                            }
                            else
                            {
                                plugin.Log.LogWarning(
                                    $"Could not find '{patchData.ClipName}' in the 'orbits' asset bundle."
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                plugin.Log.LogError($"Error in RoundManager_Start_Patch Postfix: {ex}");
            }
        }
    }
}