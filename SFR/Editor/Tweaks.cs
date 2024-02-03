﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SFD.MapEditor;
using SFD.States;
using CConst = SFDCT.Misc.Constants;

namespace SFDCT.Editor;

[HarmonyPatch]
internal static class Tweaks
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StateEditor), nameof(StateEditor.Load))]
    private static void PatchEditorWindowIcon(StateEditor __instance)
    {
        // Fix the editor window displaying SFD icon
        if (Program.GameIcon != null)
        {
            __instance.m_mapEditorForm.Icon = Program.GameIcon;
        }
    }

    /// <summary>
    ///     The game checks the version written into the ImagesList.sfdx file,
    ///     if it's different it rebuilds it again. This makes the game build it
    ///     with the target SFD version (v.1.3.7d) instead of v.1.3.7x. So if the
    ///     user uses vanilla-SFD, the imagelist won't be re-built.
    /// </summary>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(SFDMapEditor), nameof(SFDMapEditor.BuildTreeViewImageList), new Type[] {})]
    private static IEnumerable<CodeInstruction> SFDMapEditorBuildTreeViewImageList(IEnumerable<CodeInstruction> instructions)
    {
        foreach(CodeInstruction code in instructions.ToList())
        {
            if (code.operand == null)
            {
                continue;
            }
            if (code.operand.Equals("v.1.3.7x"))
            {
                code.operand = CConst.Version.SFD;
            }
        }
        return instructions;
    }
}