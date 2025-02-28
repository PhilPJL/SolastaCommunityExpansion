﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SolastaCommunityExpansion.CustomDefinitions;

namespace SolastaCommunityExpansion.Patches.CustomFeatures.NotifyConditionRemoval
{
    //
    // INotifyConditionRemoval
    //
    [HarmonyPatch(typeof(RulesetCharacter), "Kill")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class RulesetCharacter_Kill
    {
        internal static void Prefix(RulesetCharacter __instance)
        {
            foreach (var keyValuePair in __instance.ConditionsByCategory)
            {
                foreach (RulesetCondition rulesetCondition in keyValuePair.Value)
                {
                    if (rulesetCondition?.ConditionDefinition is INotifyConditionRemoval notifiedDefinition)
                    {
                        notifiedDefinition.BeforeDyingWithCondition(__instance, rulesetCondition);
                    }
                }
            }
        }
    }
}
