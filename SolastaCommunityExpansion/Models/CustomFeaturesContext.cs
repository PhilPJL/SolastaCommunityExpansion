﻿using System.Collections.Generic;
using System.Linq;
using SolastaCommunityExpansion.Api.AdditionalExtensions;
using SolastaCommunityExpansion.CustomDefinitions;
using SolastaModApi.Extensions;
using UnityEngine;

namespace SolastaCommunityExpansion.Models
{
    public static class CustomFeaturesContext
    {
        internal static void RecursiveGrantCustomFeatures(RulesetCharacterHero hero, string tag, List<FeatureDefinition> features, bool handleCustomCode = true)
        {
            foreach (var grantedFeature in features)
            {
                if (handleCustomCode && grantedFeature is IFeatureDefinitionCustomCode customFeature)
                {
                    customFeature.ApplyFeature(hero, tag);
                }

                if (grantedFeature is FeatureDefinitionFeatureSet set && set.Mode == FeatureDefinitionFeatureSet.FeatureSetMode.Union)
                {
                    RecursiveGrantCustomFeatures(hero, tag, set.FeatureSet, handleCustomCode);
                }

                if (grantedFeature is not FeatureDefinitionProficiency featureDefinitionProficiency)
                {
                    continue;
                }

                if (featureDefinitionProficiency.ProficiencyType != RuleDefinitions.ProficiencyType.FightingStyle)
                {
                    continue;
                }

                featureDefinitionProficiency.Proficiencies
                    .ForEach(prof => 
                    hero.TrainedFightingStyles
                        .Add(DatabaseRepository.GetDatabase<FightingStyleDefinition>()
                            .GetElement(prof, false)));
            }
        }

        internal static void RecursiveRemoveCustomFeatures(RulesetCharacterHero hero, string tag, List<FeatureDefinition> features, bool handleCustomCode = true)
        {
            var selectedClass = LevelUpContext.GetSelectedClass(hero);

            // this happens during character creation
            if (selectedClass == null)
            {
                return;
            }

            foreach (var grantedFeature in features)
            {
                if (handleCustomCode && grantedFeature is IFeatureDefinitionCustomCode customFeature)
                {
                    customFeature.RemoveFeature(hero, tag);
                }

                if (grantedFeature is FeatureDefinitionFeatureSet set && set.Mode == FeatureDefinitionFeatureSet.FeatureSetMode.Union)
                {
                    RecursiveRemoveCustomFeatures(hero, tag, set.FeatureSet, handleCustomCode);
                }

                if (grantedFeature is not FeatureDefinitionProficiency featureDefinitionProficiency)
                {
                    continue;
                }

                if (featureDefinitionProficiency.ProficiencyType != RuleDefinitions.ProficiencyType.FightingStyle)
                {
                    continue;
                }

                featureDefinitionProficiency.Proficiencies
                    .ForEach(prof =>
                        hero.TrainedFightingStyles
                            .Remove(DatabaseRepository.GetDatabase<FightingStyleDefinition>()
                                .GetElement(prof, false)));
            }

            hero.ClearFeatureModifiers(tag);
        }

        private static void RemoveFeatureDefinitionPointPool(RulesetCharacterHero hero, RulesetSpellRepertoire heroRepertoire, FeatureDefinitionPointPool featureDefinitionPointPool)
        {
            var poolAmount = featureDefinitionPointPool.PoolAmount;

            switch (featureDefinitionPointPool.PoolType)
            {
                case HeroDefinitions.PointsPoolType.AbilityScore:
                    // this is handled when attributes are refreshed
                    break;

                case HeroDefinitions.PointsPoolType.Cantrip:
                    heroRepertoire?.KnownCantrips.RemoveRange(heroRepertoire.KnownCantrips.Count - poolAmount, poolAmount);
                    break;

                case HeroDefinitions.PointsPoolType.Spell:
                    heroRepertoire?.KnownSpells.RemoveRange(heroRepertoire.KnownSpells.Count - poolAmount, poolAmount);
                    break;

                case HeroDefinitions.PointsPoolType.Expertise:
                    hero.TrainedExpertises.RemoveRange(hero.TrainedExpertises.Count - poolAmount, poolAmount);
                    break;

                case HeroDefinitions.PointsPoolType.Feat:
                    hero.TrainedFeats.RemoveRange(hero.TrainedFeats.Count - poolAmount, poolAmount);
                    break;

                case HeroDefinitions.PointsPoolType.Language:
                    hero.TrainedLanguages.RemoveRange(hero.TrainedLanguages.Count - poolAmount, poolAmount);
                    break;

                case HeroDefinitions.PointsPoolType.Metamagic:
                    hero.TrainedMetamagicOptions.RemoveRange(hero.TrainedMetamagicOptions.Count - poolAmount, poolAmount);
                    break;

                case HeroDefinitions.PointsPoolType.Skill:
                    hero.TrainedSkills.RemoveRange(hero.TrainedSkills.Count - poolAmount, poolAmount);
                    break;

                case HeroDefinitions.PointsPoolType.Tool:
                    hero.TrainedToolTypes.RemoveRange(hero.TrainedToolTypes.Count - poolAmount, poolAmount);
                    break;
            }
        }

        internal static void RemoveFeatures(RulesetCharacterHero hero, CharacterClassDefinition characterClassDefinition, string tag, List<FeatureDefinition> featuresToRemove)
        {
            var classLevel = hero.ClassesAndLevels[characterClassDefinition];
            var heroRepertoire = hero.SpellRepertoires.FirstOrDefault(x => LevelUpContext.IsRepertoireFromSelectedClassSubclass(hero, x));
            var buildinData = hero.GetHeroBuildingData();
            var spellTag = GetSpellLearningTag(hero, tag);

            foreach (var featureDefinition in featuresToRemove)
            {
                if (featureDefinition is FeatureDefinitionCastSpell && heroRepertoire != null)
                {
                    hero.SpellRepertoires.Remove(heroRepertoire);
                }
                if (featureDefinition is FeatureDefinitionAutoPreparedSpells featureDefinitionAutoPreparedSpells && heroRepertoire != null)
                {
                    var spellsToRemove = featureDefinitionAutoPreparedSpells.AutoPreparedSpellsGroups.FirstOrDefault(x => x.ClassLevel == classLevel)?.SpellsList.Count ?? 0;

                    while (spellsToRemove-- > 0)
                    {
                        heroRepertoire.AutoPreparedSpells.RemoveAt(heroRepertoire.AutoPreparedSpells.Count - 1);
                    }
                }
                else if (featureDefinition is FeatureDefinitionBonusCantrips featureDefinitionBonusCantrips && heroRepertoire != null)
                {
                    //TODO: fix potential problem if several features grant same cantrip, but we only remove one of them
                    heroRepertoire.KnownCantrips.RemoveAll(featureDefinitionBonusCantrips.BonusCantrips.Contains);
                    if (buildinData != null)
                    {
                        if (buildinData.BonusCantrips.ContainsKey(spellTag))
                        {
                            buildinData.BonusCantrips[spellTag]
                                .RemoveAll(featureDefinitionBonusCantrips.BonusCantrips.Contains);
                        }
                    }
                }
                else if (featureDefinition is FeatureDefinitionFightingStyleChoice)
                {
                    hero.TrainedFightingStyles.RemoveAt(hero.TrainedFightingStyles.Count - 1);
                }
                else if (featureDefinition is FeatureDefinitionSubclassChoice)
                {
                    hero.ClassesAndSubclasses.Remove(characterClassDefinition);
                }
                else if (featureDefinition is FeatureDefinitionPointPool featureDefinitionPointPool)
                {
                    RemoveFeatureDefinitionPointPool(hero, heroRepertoire, featureDefinitionPointPool);
                }
                else if (featureDefinition is FeatureDefinitionFeatureSet featureDefinitionFeatureSet && featureDefinitionFeatureSet.Mode == FeatureDefinitionFeatureSet.FeatureSetMode.Union)
                {
                    RemoveFeatures(hero, characterClassDefinition, tag, featureDefinitionFeatureSet.FeatureSet);
                }
            }
        }

        public static void ActuallyRemoveCharacterFeature(RulesetCharacterHero hero, FeatureDefinition feature)
        {
            if (feature is FeatureDefinitionFeatureSet set && set.Mode == FeatureDefinitionFeatureSet.FeatureSetMode.Union)
            {
                foreach (var f in set.FeatureSet)
                {
                    ActuallyRemoveCharacterFeature(hero, f);
                }
            }

            var selectedClass = LevelUpContext.GetSelectedClass(hero);
            foreach (var e in hero.ActiveFeatures)
            {
                var tag = e.Key;
                var features = e.Value;

                if (features.Contains(feature))
                {
                    var featuresToRemove = new List<FeatureDefinition> { feature };
                    RecursiveRemoveCustomFeatures(hero, tag, featuresToRemove, handleCustomCode: false);
                    if (selectedClass != null)
                    {
                        RemoveFeatures(hero, selectedClass, tag, featuresToRemove);
                    }
                    features.Remove(feature);
                    break;
                }
            }
        }

        internal static void RechargeLinkedPowers(RulesetCharacter character, RuleDefinitions.RestType restType)
        {
            var pointPoolPowerDefinitions = new List<FeatureDefinitionPower>();

            foreach (var usablePower in character.UsablePowers)
            {
                if (usablePower.PowerDefinition is IPowerSharedPool pool)
                {
                    var pointPoolPower = pool.GetUsagePoolPower();

                    // Only add to recharge here if it (recharges on a short rest and this is a short or long rest) or 
                    // it recharges on a long rest and this is a long rest.
                    if (!pointPoolPowerDefinitions.Contains(pointPoolPower)
                        && ((pointPoolPower.RechargeRate == RuleDefinitions.RechargeRate.ShortRest &&
                            (restType == RuleDefinitions.RestType.ShortRest || restType == RuleDefinitions.RestType.LongRest)) ||
                            (pointPoolPower.RechargeRate == RuleDefinitions.RechargeRate.LongRest && restType == RuleDefinitions.RestType.LongRest)))
                    {
                        pointPoolPowerDefinitions.Add(pointPoolPower);
                    }
                }
            }

            // Find the UsablePower of the point pool powers.
            foreach (var poolPower in character.UsablePowers)
            {
                if (pointPoolPowerDefinitions.Contains(poolPower.PowerDefinition))
                {
                    var poolSize = GetMaxUsesForPool(poolPower, character);

                    poolPower.SetRemainingUses(poolSize);

                    AssignUsesToSharedPowersForPool(character, poolPower, poolSize, poolSize);
                }
            }
        }

        internal static void AssignUsesToSharedPowersForPool(RulesetCharacter character, RulesetUsablePower poolPower, int remainingUses, int totalUses)
        {
            // Find powers that rely on this pool
            foreach (var usablePower in character.UsablePowers)
            {
                if (usablePower.PowerDefinition is IPowerSharedPool pool)
                {
                    var pointPoolPower = pool.GetUsagePoolPower();

                    if (pointPoolPower == poolPower.PowerDefinition)
                    {
                        usablePower.SetMaxUses(totalUses / usablePower.PowerDefinition.CostPerUse);
                        usablePower.SetRemainingUses(remainingUses / usablePower.PowerDefinition.CostPerUse);
                    }
                }
            }
        }

        internal static int GetMaxUsesForPool(RulesetUsablePower poolPower, RulesetCharacter character)
        {
            int totalPoolSize = poolPower.MaxUses;

            foreach (var modifierPower in character.UsablePowers)
            {
                if (modifierPower.PowerDefinition is IPowerPoolModifier modifier && modifier.GetUsagePoolPower() == poolPower.PowerDefinition)
                {
                    totalPoolSize += modifierPower.MaxUses;
                }
            }

            return totalPoolSize;
        }

        internal static void UpdateUsageForPowerPool(this RulesetCharacter character, RulesetUsablePower modifiedPower, int poolUsage)
        {
            if (modifiedPower.PowerDefinition is not IPowerSharedPool sharedPoolPower)
            {
                return;
            }

            var pointPoolPower = sharedPoolPower.GetUsagePoolPower();

            foreach (var poolPower in character.UsablePowers)
            {
                if (poolPower.PowerDefinition == pointPoolPower)
                {
                    var maxUses = GetMaxUsesForPool(poolPower, character);
                    var remainingUses = Mathf.Clamp(poolPower.RemainingUses - poolUsage, 0, maxUses);

                    poolPower.SetRemainingUses(remainingUses);
                    AssignUsesToSharedPowersForPool(character, poolPower, remainingUses, maxUses);

                    return;
                }
            }
        }

        public static EffectDescription ModifySpellEffect(EffectDescription original, RulesetEffectSpell spell)
        {
            //TODO: find a way to cache result, so it works faster - this method is called sveral times per spell cast
            var result = original;
            var caster = spell.Caster;

            var baseDefinition = spell.SpellDefinition.GetFirstSubFeatureOfType<ICustomMagicEffectBasedOnCaster>();
            if (baseDefinition != null && caster != null)
            {
                result = baseDefinition.GetCustomEffect(caster) ?? original;
            }

            var modifiers = caster.GetFeaturesByType<IModifySpellEffect>();

            if (!modifiers.Empty())
            {
                result = modifiers.Aggregate(result.Copy(), (current, f) => f.ModifyEffect(spell, current));
            }

            return result;
        }

        /**Modifies spell description for GUI purposes. Uses only modifiers based on ICustomMagicEffectBasedOnCaster*/
        public static EffectDescription ModifySpellEffectGui(EffectDescription original, GuiSpellDefinition spell)
        {
            var result = original;
            var caster = Global.InspectedHero 
                         ?? Global.ActiveLevelUpHero
                         ?? Global.ActivePlayerCharacter?.RulesetCharacter;

            var baseDefinition = spell.SpellDefinition.GetFirstSubFeatureOfType<ICustomMagicEffectBasedOnCaster>();
            if (baseDefinition != null && caster != null)
            {
                result = baseDefinition.GetCustomEffect(caster) ?? original;
            }

            return result;
        }
        
        public static EffectDescription AddEffectForms(EffectDescription baseEffect, params EffectForm[] effectForms)
        {
            var newEffect = baseEffect.Copy();

            newEffect.EffectForms.AddRange(effectForms);

            return newEffect;
        }

        public static bool GetValidationErrors(
            IEnumerable<IFeatureDefinitionWithPrerequisites.Validate> validators, out List<string> errors)
        {
            errors = validators
                .Select(v => v())
                .Where(v => v != null)
                .ToList();
            return errors.Empty();
        }

        public static string UnCustomizeTag(string tag)
        {
            return tag.Replace("[Custom]", "");
        }

        public static string CustomizeTag(string tag)
        {
            return UnCustomizeTag(tag) + "[Custom]";
        }

        public static string GetSpellLearningTag(RulesetCharacterHero hero, string tag)
        {
            if (tag != null 
                && (tag.StartsWith(AttributeDefinitions.TagClass) || tag.StartsWith(AttributeDefinitions.TagSubclass)))
            {
                ServiceRepository.GetService<ICharacterBuildingService>()
                    .GetLastAssignedClassAndLevel(hero, out var lastClass, out var classLevel);

                if (LevelDownContext.IsLevelDown)
                {
                    classLevel -= 1;
                }

                if (classLevel > 0)
                {
                    return AttributeDefinitions.GetClassTag(lastClass, classLevel);
                }
            }

            return tag;
        }
    }
}
