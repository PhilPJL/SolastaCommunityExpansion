﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static SolastaModApi.DatabaseHelper.CharacterClassDefinitions;

namespace SolastaCommunityExpansion.Patches.Multiclass.Wildshape
{
    // uses class level when offering wildshape
    [HarmonyPatch(typeof(ShapeOptionItem), "Bind")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class ShapeOptionItem_Bind
    {
        internal static void Postfix(
            GuiLabel ___levelLabel,
            Color ___validLevelColor,
            Color ___invalidLevelColor,
            Toggle ___toggle,
            CanvasGroup ___canvasGroup,
            RulesetCharacter shifter,
            int requiredLevel)
        {
            if (shifter is RulesetCharacterHero rulesetCharacterHero
                && rulesetCharacterHero.ClassesAndLevels.TryGetValue(Druid, out var levels))
            {
                var isShapeOptionAvailable = requiredLevel <= levels;

                ___levelLabel.TMP_Text.color = isShapeOptionAvailable ? ___validLevelColor : ___invalidLevelColor;
                ___toggle.interactable = isShapeOptionAvailable;
                ___canvasGroup.alpha = isShapeOptionAvailable ? 1f : 0.3f;
            }
        }
    }
}
