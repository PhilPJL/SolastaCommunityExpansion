﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using HarmonyLib;
using SolastaCommunityExpansion.Models;
using SolastaModApi.Infrastructure;

namespace SolastaCommunityExpansion.Patches.CustomFeatures.PactMagic
{
    // Guarantee Warlock Spell Level will be used whenever possible on SC Warlocks
    [HarmonyPatch(typeof(SpellsByLevelBox), "OnActivateStandardBox")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class SpellsByLevelBox_OnActivateStandardBox
    {
        internal static void Prefix(
            int index,
            SpellsByLevelBox.SpellCastEngagedHandler ___spellCastEngaged,
            Dictionary<int, SpellDefinition> ___spellsByIndex,
            RulesetSpellRepertoire ___spellRepertoire)
        {
            if (___spellCastEngaged != null)
            {
                Global.CastedSpell = ___spellsByIndex[index];
                Global.CastedSpellRepertoire = ___spellRepertoire;
            }
        }

        public static int MySpellLevel(SpellDefinition spellDefinition, SpellsByLevelBox spellsByLevelBox)
        {
            var rulesetSpellRepertoire = spellsByLevelBox.GetField<SpellsByLevelBox, RulesetSpellRepertoire>("spellRepertoire");
            var isWarlockSpell = SharedSpellsContext.IsWarlock(rulesetSpellRepertoire.SpellCastingClass);

            if (isWarlockSpell)
            {
                var hero = SharedSpellsContext.GetHero(rulesetSpellRepertoire.CharacterName);
                var warlockSpellLevel = SharedSpellsContext.GetWarlockSpellLevel(hero);

                return warlockSpellLevel;
            }

            return spellDefinition.SpellLevel;
        }

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var spellLevelMethod = typeof(SpellDefinition).GetMethod("get_SpellLevel");
            var mySpellLevelMethod = typeof(SpellsByLevelBox_OnActivateStandardBox).GetMethod("MySpellLevel");

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(spellLevelMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, mySpellLevelMethod);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }

    // Guarantee Warlock Spell Level will be used whenever possible on SC Warlocks
    [HarmonyPatch(typeof(SpellsByLevelBox), "OnActivateAdvancedBox")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class SpellsByLevelBox_OnActivateAdvancedBox
    {
        public static void Prefix(
            int index,
            SpellsByLevelBox.SpellCastEngagedHandler ___spellCastEngaged,
            Dictionary<int, SpellDefinition> ___spellsByIndex,
            RulesetSpellRepertoire ___spellRepertoire)
        {
            if (___spellCastEngaged != null)
            {
                Global.CastedSpell = ___spellsByIndex[index];
                Global.CastedSpellRepertoire = ___spellRepertoire;
            }
        }
    }
}
