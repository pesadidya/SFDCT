using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using SFD;
using SFD.Code;
using SFD.CollisionGroups;
using SFD.Colors;
using SFD.Effects;
using SFD.GameKeyboard;
using SFD.GUI.Text;
using SFD.GUI;
using SFD.ManageLists;
using SFD.Materials;
using SFD.Parser;
using SFD.PingData;
using SFD.Projectiles;
using SFD.Sounds;
using SFD.States;
using SFD.SteamIntegration;
using SFD.Tiles;
using SFD.UserProgression;
using SFD.Weapons;
using SFDCT.Fighter;
using SFDCT.Helper;
using static System.Net.Mime.MediaTypeNames;
using static SFD.Sounds.SoundHandler;
using SFRSettings = SFDCT.Settings.Values;

namespace SFDCT.Bootstrap;

/// <summary>
///     This is where SFR starts.
///     This class handles and loads all the new textures, music, sounds, tiles, colors etc...
///     This class is also used to tweak some game code on startup, such as window title.
/// </summary>
[HarmonyPatch]
internal static class Assets
{
    /// <summary>
    ///     This method is executed whenever we close the game or it crash.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.OnExiting))]
    private static void Dispose()
    {
        Logger.LogError("Disposing");
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.Load))]
    private static void LoadConfigIni()
    {
        Misc.ConfigIni.Initialize();
    }

    /// <summary>
    ///     Fix for loading textures from both paths.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TitleContainer), nameof(TitleContainer.OpenStream))]
    private static bool StreamPatch(string name, ref Stream __result)
    {
        if (name.Contains(@"Content\Data"))
        {
            if (name.EndsWith(".xnb.xnb"))
            {
                name = name.Substring(0, name.Length - 4);
            }

            __result = File.OpenRead(name);
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Fix for loading textures from both paths.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants.Paths), nameof(Constants.Paths.GetContentAssetPathFromFullPath))]
    private static bool GetContentAssetPathFromFullPath(string path, ref string __result)
    {
        __result = path;
        return false;
    }

    // Keep for future use
    /*
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Items), nameof(Items.Load))]
    private static bool LoadItems(GameSFD game)
    {
        Logger.LogInfo("LOADING: Items");

        var content = game.Content;
        Items.m_allItems = new List<Item>();
        Items.m_allFemaleItems = new List<Item>();
        Items.m_allMaleItems = new List<Item>();
        Items.m_slotAllItems = new List<Item>[10];
        Items.m_slotFemaleItems = new List<Item>[10];
        Items.m_slotMaleItems = new List<Item>[10];

        for (int i = 0; i < Items.m_slotAllItems.Length; i++)
        {
            Items.m_slotAllItems[i] = new List<Item>();
            Items.m_slotFemaleItems[i] = new List<Item>();
            Items.m_slotMaleItems[i] = new List<Item>();
        }

        var files = Directory.GetFiles(Path.Combine(ContentPath, @"Data\Items"), "*.xnb", SearchOption.AllDirectories).ToList();
        var originalItems = Directory.GetFiles(Constants.Paths.GetContentFullPath(@"Data\Items"), "*.xnb", SearchOption.AllDirectories).ToList();
        foreach (string item in originalItems)
        {
            if (files.TrueForAll(f => Path.GetFileNameWithoutExtension(f) != Path.GetFileNameWithoutExtension(item)))
            {
                files.Add(item);
            }
        }

        foreach (string file in files)
        {
            if (GameSFD.Closing)
            {
                return false;
            }

            var item = content.Load<Item>(file);
            if (Items.m_allItems.Any(item2 => item2.ID == item.ID))
            {
                throw new Exception("Can't load items");
            }

            item.PostProcess();
            Items.m_allItems.Add(item);
            Items.m_slotAllItems[item.EquipmentLayer].Add(item);
        }

        Items.PostProcessGenders();
        Player.HurtLevel1 = Items.GetItem("HurtLevel1");
        Player.HurtLevel2 = Items.GetItem("HurtLevel2") ?? Player.HurtLevel1;

        return false;
    }
    */

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), MethodType.Constructor)]
    private static void Init(GameSFD __instance)
    {
        string SplashText = "";
        if (SFD.Constants.RANDOM.Next(99) == 0)
        {
            // If you decode it you shall burn in
            // +300ms latency hell.
            SplashText = SFD.Converter.Base64ToString(SFD.Converter.Base64ToString("TFNCTWFXOXJhVzVrZVNCM1lYTWdhR1Z5WlNFPQ=="));
        }
        
        // "Superfighters Deluxe v1.3.7d - Custom v.1.0.0 "
        __instance.Window.Title = __instance.Window.Title.Replace("v.1.3.7x", Misc.Constants.Version.SFD) + " - Custom " + Misc.Constants.Version.SFDCT + SplashText;
    }

    // Keep for future use
    /*
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Animations), nameof(Animations.Load))]
    private static void LoadAnimations(Microsoft.Xna.Framework.Game game)
    {
        Logger.LogInfo("LOADING: Animations");
        var data = Animations.Data;
        var anims = data.Animations;

        var customData = AnimHandler.GetAnimations(data);
        Array.Resize(ref anims, data.Animations.Length + customData.Count);
        for (int i = 0; i < customData.Count; i++)
        {
            anims[anims.Length - 1 - i] = customData[i];
            // Logger.LogDebug("Adding animation: " + customData[i].Name);
        }

        data.Animations = anims;
        AnimationsData animData = new(data.Animations);
        Animations.Data = animData;
    }
    */
}