﻿using System.Collections.Generic;
using static SolastaCommunityExpansion.Level20.Features.FeatureDefinitionIndomitableMightBuilder;
using static SolastaCommunityExpansion.Level20.Features.FeatureDefinitionPrimalChampionBuilder;
using static SolastaModApi.DatabaseHelper.CharacterClassDefinitions;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionAttributeModifiers;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionFeatureSets;

namespace SolastaCommunityExpansion.Level20.Classes
{
    internal static class BarbarianBuilder
    {
        internal static void Load()
        {
            Barbarian.FeatureUnlocks.AddRange(new List<FeatureUnlockByLevel> {
                // TODO 15: Persistent Rage
                new FeatureUnlockByLevel(FeatureSetAbilityScoreChoice, 16),
                new FeatureUnlockByLevel(AttributeModifierBarbarianBrutalCriticalAdd, 17),
                new FeatureUnlockByLevel(FeatureDefinitionIndomitableMight, 18),
                new FeatureUnlockByLevel(FeatureSetAbilityScoreChoice, 19),
                new FeatureUnlockByLevel(FeatureDefinitionPrimalChampion, 20),
            });
        }
    }
}
