using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
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
using CConst = SFDCT.Misc.Constants;
using CSettings = SFDCT.Settings.Values;
using CIni = SFDCT.Misc.ConfigIni;
using Box2D.XNA;
using static System.Windows.Forms.LinkLabel;

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

        // Settings
        CSettings.CheckOverrides();
        CIni.Save();

        Program.RevertVanillaSFDConfig();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Challenges), nameof(Challenges.Load))]
    private static IEnumerable<CodeInstruction> ItemsLock(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        // At these lines, some items used in the official campaigns and challenges
        // are locked with no real reason. (Except that clothing is weird on FrankenBear/Mech)
        // (Normal bear skin requires extra patching, and therefore is the only skin to not work 
        // on other servers)
        code.RemoveRange(859, 61);
        return code;
    }

    /// <summary>
    ///     Init the configuration ini file
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.Load))]
    private static void LoadConfigIni()
    {
        CIni.Initialize();
    }

    /// <summary>
    ///     Change window title to Superfighters Deluxe Custom
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), MethodType.Constructor)]
    private static void Init(GameSFD __instance)
    {
        __instance.Window.Title = $"Superfighters Deluxe Custom {CConst.Version.Label}";
    }
}