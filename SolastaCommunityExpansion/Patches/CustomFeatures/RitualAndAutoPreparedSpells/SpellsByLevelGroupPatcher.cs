﻿//
// This is now handled on MC Level Up patches
//
#if false
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;

namespace SolastaCommunityExpansion.Patches.CustomFeatures.RitualAndAutoPreparedSpells
{
    [HarmonyPatch(typeof(SpellsByLevelGroup), "CommonBind")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class SpellsByLevelGroup_CommonBind
    {
        /*
         * So this patch seems like it should be unnecessary, however when collecting the autoprepared spells TA only looks at the FIRST
         * feature returned from enumerate features rather than iterate over all of them. This means only 1 feature worth of auto prepared
         * spells are shown to the user during level up. For the inspection/spell preparation binding a different method to collect the auto
         * prepared spells is used which works properly.
         */
        internal static void Prefix(
            SpellsByLevelGroup __instance,
            SpellBox.BindMode bindMode,
            RulesetCharacter caster,
            ref List<SpellDefinition> allSpells, 
            ref List<SpellDefinition> auToPreparedSpells)
        {
            if (!Main.Settings.ShowAllAutoPreparedSpells || bindMode == SpellBox.BindMode.Inspection || bindMode == SpellBox.BindMode.Preparation)
            {
                return;
            }

            // Wait what? Yes, during level up no caster is bound. This is techncially fine, but we need one to collect the spells.
            if (caster == null)
            {
                caster = Global.ActiveLevelUpHero;
            }

            // Collect all the auto prepared spells.
            // Also filter the prepped spells by level this group is displaying.
            caster.EnumerateFeaturesToBrowse<FeatureDefinitionAutoPreparedSpells>(caster.FeaturesToBrowse);

            foreach (var autoPreparedSpells in caster.FeaturesToBrowse.OfType<FeatureDefinitionAutoPreparedSpells>())
            {
                foreach (FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup preparedSpellsGroup in autoPreparedSpells.AutoPreparedSpellsGroups)
                {
                    foreach (SpellDefinition spell in preparedSpellsGroup.SpellsList)
                    {
                        var flag = !auToPreparedSpells.Contains(spell) && __instance.SpellLevel == spell.SpellLevel;

                        if (flag)
                        {
                            auToPreparedSpells.Add(spell);

                            // If a spell is not in all spells it won't be shown in the UI.
                            // Add the auto prepared spells here to make sure the user sees them.
                            allSpells.TryAdd(spell);
                        }
                    }
                }
            }
        }
    }
}
#endif
