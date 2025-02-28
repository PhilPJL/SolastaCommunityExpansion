﻿using System.Collections.Generic;
using SolastaCommunityExpansion.Level20.Features;
using SolastaModApi.Extensions;
using SolastaModApi.Infrastructure;
using static SolastaModApi.DatabaseHelper.CharacterClassDefinitions;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionAutoPreparedSpellss;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionCastSpells;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionFeatureSets;
using static SolastaModApi.DatabaseHelper.SpellDefinitions;

namespace SolastaCommunityExpansion.Level20.Classes
{
    internal static class PaladinBuilder
    {
        internal static void Load()
        {
            Paladin.FeatureUnlocks.AddRange(
                new FeatureUnlockByLevel(PowerPaladinCleansingTouchBuilder.PowerPaladinCleansingTouch, 14),
                new FeatureUnlockByLevel(FeatureSetAbilityScoreChoice, 16),
                new FeatureUnlockByLevel(PowerPaladinAuraOfCourage18Builder.Instance, 18),
                new FeatureUnlockByLevel(PowerPaladinAuraOfProtection18Builder.Instance, 18),
                new FeatureUnlockByLevel(FeatureSetAbilityScoreChoice, 19)
            );

            //// Oath of Devotion
            AutoPreparedSpellsOathOfDevotion.AutoPreparedSpellsGroups.Add(
                new FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup
                {
                    ClassLevel = 13,
                    SpellsList = new List<SpellDefinition>
                    {
                        FreedomOfMovement,
                        GuardianOfFaith
                    }
                });

            AutoPreparedSpellsOathOfDevotion.AutoPreparedSpellsGroups.Add(
                new FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup
                {
                    ClassLevel = 17,
                    SpellsList = new List<SpellDefinition>
                    {
                        // Commune,
                        FlameStrike
                    }
                });

            //// Oath of Motherlands
            AutoPreparedSpellsOathOfMotherland.AutoPreparedSpellsGroups.Add(
                new FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup
                {
                    ClassLevel = 13,
                    SpellsList = new List<SpellDefinition>
                    {
                        WallOfFire,
                        Stoneskin
                    }
                });

            AutoPreparedSpellsOathOfMotherland.AutoPreparedSpellsGroups.Add(
                new FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup
                {
                    ClassLevel = 17,
                    SpellsList = new List<SpellDefinition>
                    {
                        FlameStrike
                    }
                });

            //// Oath of Tirmar
            AutoPreparedSpellsOathOfTirmar.AutoPreparedSpellsGroups.Add(
                new FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup
                {
                    ClassLevel = 13,
                    SpellsList = new List<SpellDefinition>
                    {
                        Banishment,
                        // Compulsion
                    }
                });

            AutoPreparedSpellsOathOfTirmar.AutoPreparedSpellsGroups.Add(
                new FeatureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroup
                {
                    ClassLevel = 17,
                    SpellsList = new List<SpellDefinition>
                    {
                        WallOfForce,
                        HoldMonster
                    }
                });

            CastSpellPaladin.SetSpellCastingLevel(5);

            CastSpellPaladin.SlotsPerLevels.SetRange(SpellsHelper.HalfCastingSlots);
            CastSpellPaladin.ReplacedSpells.SetRange(SpellsHelper.EmptyReplacedSpells);
        }
    }
}
