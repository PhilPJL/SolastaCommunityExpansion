﻿using System;
using System.Collections.Generic;
using System.Linq;
using SolastaCommunityExpansion.Builders;
using SolastaCommunityExpansion.Builders.Features;
using SolastaCommunityExpansion.CustomDefinitions;
using SolastaModApi.Extensions;
using static SolastaModApi.DatabaseHelper;
using static SolastaModApi.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaModApi.DatabaseHelper.ConditionDefinitions;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionAdditionalDamages;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionPowers;

namespace SolastaCommunityExpansion.Subclasses.Barbarian
{
    internal class PathOfTheLight : AbstractSubclass
    {
        private static readonly Guid SubclassNamespace = new("c2067110-5086-45c0-b0c2-4c140599605c");
        private const string IlluminatedConditionName = "PathOfTheLightIlluminatedCondition";
        private const string IlluminatingStrikeName = "PathOfTheLightIlluminatingStrike";
        private const string IlluminatingBurstName = "PathOfTheLightIlluminatingBurst";

        private static readonly List<ConditionDefinition> InvisibleConditions =
            new()
            {
                ConditionInvisibleBase,
                ConditionInvisible,
                ConditionInvisibleGreater
            };

        private static readonly Dictionary<int, int> LightsProtectionAmountHealedByClassLevel = new()
        {
            { 6, 3 },
            { 7, 3 },
            { 8, 4 },
            { 9, 4 },
            { 10, 5 },
            { 11, 5 },
            { 12, 6 },
            { 13, 6 },
            { 14, 7 },
            { 15, 7 },
            { 16, 8 },
            { 17, 8 },
            { 18, 9 },
            { 19, 9 },
            { 20, 10 }
        };

        internal override FeatureDefinitionSubclassChoice GetSubclassChoiceList()
        {
            return FeatureDefinitionSubclassChoices.SubclassChoiceBarbarianPrimalPath;
        }

        internal override CharacterSubclassDefinition GetSubclass()
        {
            return Subclass;
        }

        private CharacterSubclassDefinition Subclass { get; } = CharacterSubclassDefinitionBuilder
            .Create("PathOfTheLight", SubclassNamespace)
            .SetGuiPresentation("BarbarianPathOfTheLight", Category.Subclass, DomainSun.GuiPresentation.SpriteReference)
            .AddFeatureAtLevel(IlluminatingStrike, 3)
            .AddFeatureAtLevel(PierceTheDarkness, 3)
            .AddFeatureAtLevel(LightsProtection, 6)
            .AddFeatureAtLevel(EyesOfTruth, 10)
            .AddFeatureAtLevel(IlluminatingStrikeImprovement, 10)
            .AddFeatureAtLevel(IlluminatingBurst, 14)
            .AddToDB();

        private static ConditionDefinitionIlluminated illuminatedCondition;
        private static ConditionDefinitionIlluminated IlluminatedCondition => illuminatedCondition ??= ConditionDefinitionIlluminatedBuilder
            .Create(IlluminatedConditionName, SubclassNamespace)
            .SetGuiPresentation("BarbarianPathOfTheLightIlluminatedCondition", Category.Subclass, ConditionBranded.GuiPresentation.SpriteReference)
            .SetAllowMultipleInstances(true)
            .SetConditionType(RuleDefinitions.ConditionType.Detrimental)
            .SetDuration(RuleDefinitions.DurationType.Irrelevant, 1, false) // don't validate inconsistent data
            .SetSilent(Silent.WhenAdded)
            .SetSpecialDuration(true)
            .AddFeatures(DisadvantageAgainstNonSource, PreventInvisibility)
            .AddToDB();

        private static FeatureDefinition illuminatingStrike;
        private static FeatureDefinition IlluminatingStrike => illuminatingStrike ??= FeatureDefinitionFeatureSetBuilder
            .Create("PathOfTheLightIlluminatingStrikeFeatureSet", SubclassNamespace)
            .SetGuiPresentation("BarbarianPathOfTheLightIlluminatingStrike", Category.Subclass)
            .SetEnumerateInDescription(false)
            .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Union)
            .SetUniqueChoices(false)
            .AddFeatureSet(IlluminatingStrikeInitiatorBuilder
                .Create("PathOfTheLightIlluminatingStrikeInitiator", SubclassNamespace, IlluminatedCondition)
                .SetGuiPresentationNoContent(true)
                .AddToDB())
            .AddToDB();

        // Dummy feature to show in UI
        private static FeatureDefinition illuminatingStrikeImprovement;
        private static FeatureDefinition IlluminatingStrikeImprovement => illuminatingStrikeImprovement ??= FeatureDefinitionBuilder
            .Create("PathOfTheLightIlluminatingStrikeImprovement", SubclassNamespace)
            .SetGuiPresentation("BarbarianPathOfTheLightIlluminatingStrikeImprovement", Category.Subclass)
            .AddToDB();

        private static FeatureDefinition pierceTheDarkness;
        private static FeatureDefinition PierceTheDarkness => pierceTheDarkness ??= FeatureDefinitionFeatureSetBuilder
            .Create("PathOfTheLightPierceTheDarkness", SubclassNamespace)
            .SetGuiPresentation("BarbarianPathOfTheLightPierceTheDarkness", Category.Subclass)
            .SetEnumerateInDescription(false)
            .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Union)
            .SetUniqueChoices(false)
            .AddFeatureSet(FeatureDefinitionSenses.SenseSuperiorDarkvision)
            .AddToDB();

        private static FeatureDefinition lightsProtection;
        private static FeatureDefinition LightsProtection => lightsProtection ??= FeatureDefinitionFeatureSetBuilder
            .Create("PathOfTheLightLightsProtection", SubclassNamespace)
            .SetGuiPresentation("BarbarianPathOfTheLightLightsProtection", Category.Subclass)
            .SetEnumerateInDescription(false)
            .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Union)
            .SetUniqueChoices(false)
            .AddFeatureSet(
                FeatureDefinitionOpportunityAttackImmunityIfAttackerHasConditionBuilder
                    .Create("PathOfTheLightLightsProtectionOpportunityAttackImmunity", SubclassNamespace)
                    .SetGuiPresentationNoContent()
                    .SetConditionName(IlluminatedConditionName)
                    .AddToDB())
            .AddToDB();

        private static void ApplyLightsProtectionHealing(ulong sourceGuid)
        {
            if (RulesetEntity.GetEntity<RulesetCharacter>(sourceGuid) is not RulesetCharacterHero conditionSource || conditionSource.IsDead)
            {
                return;
            }

            if (!conditionSource.ClassesAndLevels.TryGetValue(CharacterClassDefinitions.Barbarian, out int levelsInClass))
            {
                // Character doesn't have levels in class
                return;
            }

            if (!LightsProtectionAmountHealedByClassLevel.TryGetValue(levelsInClass, out int amountHealed))
            {
                // Character doesn't heal at the current level
                return;
            }

            if (amountHealed > 0)
            {
                conditionSource.ReceiveHealing(amountHealed, notify: true, sourceGuid);
            }
        }

        private static FeatureDefinition eyesOfTruth;
        private static FeatureDefinition EyesOfTruth => eyesOfTruth ??= CreateEyesOfTruth();

        private static FeatureDefinition CreateEyesOfTruth()
        {
            var seeingInvisibleCondition = ConditionDefinitionBuilder
                .Create("PathOfTheLightEyesOfTruthSeeingInvisible", SubclassNamespace)
                .SetGuiPresentation("BarbarianPathOfTheLightSeeingInvisibleCondition", Category.Subclass, ConditionSeeInvisibility.GuiPresentation.SpriteReference)
                .SetAllowMultipleInstances(false)
                .SetConditionType(RuleDefinitions.ConditionType.Beneficial)
                .SetDuration(RuleDefinitions.DurationType.Permanent, 1, false) // don't validate inconsistent data
                .SetSilent(Silent.WhenAddedOrRemoved)
                .AddFeatures(FeatureDefinitionSenses.SenseSeeInvisible16)
                .AddToDB();

            var seeInvisibleEffectBuilder = new EffectDescriptionBuilder();

            var seeInvisibleConditionForm = new EffectForm
            {
                FormType = EffectForm.EffectFormType.Condition,
                ConditionForm = new ConditionForm
                {
                    Operation = ConditionForm.ConditionOperation.Add,
                    ConditionDefinition = seeingInvisibleCondition
                }
            };

            seeInvisibleEffectBuilder
                .SetDurationData(RuleDefinitions.DurationType.Permanent, 1, RuleDefinitions.TurnOccurenceType.StartOfTurn)
                .SetTargetingData(RuleDefinitions.Side.Ally, RuleDefinitions.RangeType.Self, 1, RuleDefinitions.TargetType.Self, 1, 0, ActionDefinitions.ItemSelectionType.None)
                .AddEffectForm(seeInvisibleConditionForm);

            var seeInvisiblePower = FeatureDefinitionPowerBuilder
                .Create("PathOfTheLightEyesOfTruthPower", SubclassNamespace)
                .SetGuiPresentation("BarbarianPathOfTheLightEyesOfTruth", Category.Subclass, SpellDefinitions.SeeInvisibility.GuiPresentation.SpriteReference)
                .SetShowCasting(false)
                .SetEffectDescription(seeInvisibleEffectBuilder.Build())
                .SetRechargeRate(RuleDefinitions.RechargeRate.AtWill)
                .SetActivationTime(RuleDefinitions.ActivationTime.Permanent)
                .AddToDB();

            return FeatureDefinitionFeatureSetBuilder
                .Create("PathOfTheLightEyesOfTruth", SubclassNamespace)
                .SetGuiPresentation("BarbarianPathOfTheLightEyesOfTruth", Category.Subclass)
                .SetEnumerateInDescription(false)
                .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Union)
                .SetUniqueChoices(false)
                .AddFeatureSet(seeInvisiblePower)
                .AddToDB();
        }

        private static FeatureDefinition illuminatingBurst;
        private static FeatureDefinition IlluminatingBurst => illuminatingBurst ??= FeatureDefinitionFeatureSetBuilder
            .Create("PathOfTheLightIlluminatingBurstFeatureSet", SubclassNamespace)
            .SetGuiPresentation("BarbarianPathOfTheLightIlluminatingBurst", Category.Subclass)
            .SetEnumerateInDescription(false)
            .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Union)
            .SetUniqueChoices(false)
            .SetFeatureSet(
                IlluminatingBurstInitiatorBuilder
                    .Create("PathOfTheLightIlluminatingBurstInitiator", SubclassNamespace, IlluminatingBurstSuppressedCondition)
                    .SetGuiPresentationNoContent(true)
                    .AddToDB(),
                PowerIlluminatingBurstBuilder
                    .Create(IlluminatingBurstName, SubclassNamespace, IlluminatedCondition, IlluminatingBurstSuppressedCondition)
                    .SetGuiPresentation("BarbarianPathOfTheLightIlluminatingBurstPower", Category.Subclass, PowerDomainSunHeraldOfTheSun.GuiPresentation.SpriteReference)
                    .AddToDB(),
                CreateIlluminatingBurstSuppressor())
            .AddToDB();

        // TODO: create IlluminatingBurstSuppressorBuilder
        private static FeatureDefinition CreateIlluminatingBurstSuppressor()
        {
            // TODO: use EffectFormBuilder
            var suppressIlluminatingBurst = new EffectForm
            {
                FormType = EffectForm.EffectFormType.Condition,
                ConditionForm = new ConditionForm
                {
                    Operation = ConditionForm.ConditionOperation.Add,
                    ConditionDefinition = IlluminatingBurstSuppressedCondition
                }
            };

            var suppressIlluminatingBurstEffect = EffectDescriptionBuilder
                .Create()
                .SetDurationData(RuleDefinitions.DurationType.Permanent, 1, RuleDefinitions.TurnOccurenceType.StartOfTurn)
                .SetTargetingData(RuleDefinitions.Side.Ally, RuleDefinitions.RangeType.Self, 1, RuleDefinitions.TargetType.Self, 1, 0, ActionDefinitions.ItemSelectionType.None)
                .SetRecurrentEffect(RuleDefinitions.RecurrentEffect.OnActivation | RuleDefinitions.RecurrentEffect.OnTurnStart)
                .AddEffectForm(suppressIlluminatingBurst)
                .Build();

            return FeatureDefinitionPowerBuilder
                .Create("PathOfTheLightIlluminatingBurstSuppressor", SubclassNamespace)
                .SetGuiPresentationNoContent(true)
                .SetActivationTime(RuleDefinitions.ActivationTime.Permanent)
                .SetRechargeRate(RuleDefinitions.RechargeRate.AtWill)
                .SetEffectDescription(suppressIlluminatingBurstEffect)
                .AddToDB();
        }

        private static FeatureDefinitionAttackDisadvantageAgainstNonSource disadvantageAgainstNonSource;
        private static FeatureDefinitionAttackDisadvantageAgainstNonSource DisadvantageAgainstNonSource => disadvantageAgainstNonSource ??=
            FeatureDefinitionAttackDisadvantageAgainstNonSourceBuilder
                .Create("PathOfTheLightIlluminatedDisadvantage", SubclassNamespace)
                .SetGuiPresentation("Feature/&NoContentTitle", "Subclass/&BarbarianPathOfTheLightIlluminatedDisadvantageDescription")
                .SetConditionName(IlluminatedConditionName)
                .AddToDB();

        // Prevents a creature from turning invisible by "granting" immunity to invisibility
        private static FeatureDefinition preventInvisibility;
        private static FeatureDefinition PreventInvisibility => preventInvisibility ??= FeatureDefinitionFeatureSetBuilder
            .Create("PathOfTheLightIlluminatedPreventInvisibility", SubclassNamespace)
            .SetGuiPresentation("Feature/&NoContentTitle", "Subclass/&BarbarianPathOfTheLightIlluminatedPreventInvisibilityDescription")
            .SetEnumerateInDescription(false)
            .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Union)
            .SetUniqueChoices(false)
            .AddFeatureSet(InvisibleConditions
                .Select(ic => FeatureDefinitionConditionAffinityBuilder
                    .Create("PathOfTheLightIlluminatedPreventInvisibility" + ic.Name, SubclassNamespace)
                    .SetGuiPresentationNoContent()
                    .SetConditionAffinityType(RuleDefinitions.ConditionAffinityType.Immunity)
                    .SetConditionType(ic)
                    .AddToDB()))
            .AddToDB();

        private static ConditionDefinition illuminatingBurstSuppressedCondition;
        private static ConditionDefinition IlluminatingBurstSuppressedCondition => illuminatingBurstSuppressedCondition ??= ConditionDefinitionBuilder
            .Create("PathOfTheLightIlluminatingBurstSuppressedCondition", SubclassNamespace)
            .SetGuiPresentationNoContent(true)
            .SetAllowMultipleInstances(false)
            .SetConditionType(RuleDefinitions.ConditionType.Neutral)
            .SetDuration(RuleDefinitions.DurationType.Permanent, 1, false) // don't validate inconsistent data
            .SetSilent(Silent.WhenAddedOrRemoved)
            .AddToDB();

        private static void HandleAfterIlluminatedConditionRemoved(RulesetActor removedFrom)
        {
            if (removedFrom is not RulesetCharacter character)
            {
                return;
            }

            // Intentionally *includes* conditions that have Illuminated as their parent (like the Illuminating Burst condition)
            if (!character.HasConditionOfTypeOrSubType(IlluminatedConditionName)
                && (character.PersonalLightSource?.SourceName == IlluminatingStrikeName || character.PersonalLightSource?.SourceName == IlluminatingBurstName))
            {
                var visibilityService = ServiceRepository.GetService<IGameLocationVisibilityService>();

                visibilityService.RemoveCharacterLightSource(GameLocationCharacter.GetFromActor(removedFrom), character.PersonalLightSource);
                character.PersonalLightSource = null;
            }
        }

        // Helper classes

        private sealed class ConditionDefinitionIlluminated : ConditionDefinition, IConditionRemovedOnSourceTurnStart, INotifyConditionRemoval
        {
            public void AfterConditionRemoved(RulesetActor removedFrom, RulesetCondition rulesetCondition)
            {
                HandleAfterIlluminatedConditionRemoved(removedFrom);
            }

            public void BeforeDyingWithCondition(RulesetActor rulesetActor, RulesetCondition rulesetCondition)
            {
                ApplyLightsProtectionHealing(rulesetCondition.SourceGuid);
            }
        }

        private sealed class ConditionDefinitionIlluminatedBuilder
            : ConditionDefinitionBuilder<ConditionDefinitionIlluminated, ConditionDefinitionIlluminatedBuilder>
        {
            internal ConditionDefinitionIlluminatedBuilder(string name, Guid guidNamespace) : base(name, guidNamespace) { }
        }

        private sealed class ConditionDefinitionIlluminatedByBurst : ConditionDefinition, INotifyConditionRemoval
        {
            public void AfterConditionRemoved(RulesetActor removedFrom, RulesetCondition rulesetCondition)
            {
                HandleAfterIlluminatedConditionRemoved(removedFrom);
            }

            public void BeforeDyingWithCondition(RulesetActor rulesetActor, RulesetCondition rulesetCondition)
            {
                ApplyLightsProtectionHealing(rulesetCondition.SourceGuid);
            }
        }

        private sealed class ConditionDefinitionIlluminatedByBurstBuilder
            : ConditionDefinitionBuilder<ConditionDefinitionIlluminatedByBurst, ConditionDefinitionIlluminatedByBurstBuilder>
        {
            internal ConditionDefinitionIlluminatedByBurstBuilder(string name, Guid guidNamespace) : base(name, guidNamespace) { }
        }

        private sealed class FeatureDefinitionAdditionalDamageIlluminatingStrike : FeatureDefinitionAdditionalDamage, IClassHoldingFeature
        {
            // Allows Illuminating Strike damage to scale with barbarian level
            public CharacterClassDefinition Class => CharacterClassDefinitions.Barbarian;
        }

        private sealed class IlluminatingStrikeFeatureBuilder : FeatureDefinitionAdditionalDamageBuilder<FeatureDefinitionAdditionalDamageIlluminatingStrike, IlluminatingStrikeFeatureBuilder>
        {
            private IlluminatingStrikeFeatureBuilder(string name, Guid guidNamespace, ConditionDefinition illuminatedCondition) : base(name, guidNamespace)
            {
                Definition
                    .SetAdditionalDamageType(RuleDefinitions.AdditionalDamageType.Specific)
                    .SetSpecificDamageType("DamageRadiant")
                    .SetTriggerCondition(RuleDefinitions.AdditionalDamageTriggerCondition.AlwaysActive)
                    .SetDamageValueDetermination(RuleDefinitions.AdditionalDamageValueDetermination.Die)
                    .SetDamageDiceNumber(1)
                    .SetDamageDieType(RuleDefinitions.DieType.D6)
                    .SetDamageSaveAffinity(RuleDefinitions.EffectSavingThrowType.None)
                    .SetLimitedUsage(RuleDefinitions.FeatureLimitedUsage.OnceInMyturn)
                    .SetNotificationTag("BarbarianPathOfTheLightIlluminatingStrike")
                    .SetRequiredProperty(RuleDefinitions.AdditionalDamageRequiredProperty.None)
                    .SetAddLightSource(true)
                    .SetLightSourceForm(CreateIlluminatedLightSource());

                SetAdvancement(
                    RuleDefinitions.AdditionalDamageAdvancement.ClassLevel,
                    (3, 1),
                    (4, 1),
                    (5, 1),
                    (6, 1),
                    (7, 1),
                    (8, 1),
                    (9, 1),
                    (10, 2),
                    (11, 2),
                    (12, 2),
                    (13, 2),
                    (14, 2),
                    (15, 2),
                    (16, 2),
                    (17, 2),
                    (18, 2),
                    (19, 2),
                    (20, 2)
                );

                Definition.ConditionOperations.Add(
                    new ConditionOperationDescription
                    {
                        Operation = ConditionOperationDescription.ConditionOperation.Add,
                        ConditionDefinition = illuminatedCondition
                    });

                foreach (ConditionDefinition invisibleCondition in InvisibleConditions)
                {
                    Definition.ConditionOperations.Add(
                        new ConditionOperationDescription
                        {
                            Operation = ConditionOperationDescription.ConditionOperation.Remove,
                            ConditionDefinition = invisibleCondition
                        });
                }
            }

            public static IlluminatingStrikeFeatureBuilder Create(string name, Guid guidNamespace, ConditionDefinition illuminatedCondition)
            {
                return new IlluminatingStrikeFeatureBuilder(name, guidNamespace, illuminatedCondition);
            }

            private static LightSourceForm CreateIlluminatedLightSource()
            {
                EffectForm faerieFireLightSource = SpellDefinitions.FaerieFire.EffectDescription.GetFirstFormOfType(EffectForm.EffectFormType.LightSource);

                var lightSourceForm = new LightSourceForm();
                lightSourceForm.Copy(faerieFireLightSource.LightSourceForm);

                lightSourceForm
                    .SetBrightRange(4)
                    .SetDimAdditionalRange(4);

                return lightSourceForm;
            }
        }

        /// <summary>
        /// Builds the power that enables Illuminating Strike while you're raging.
        /// </summary>
        private sealed class IlluminatingStrikeInitiatorBuilder : FeatureDefinitionPowerBuilder
        {
            private IlluminatingStrikeInitiatorBuilder(string name, Guid guidNamespace, ConditionDefinition illuminatedCondition) : base(name, guidNamespace)
            {
                Definition
                    .SetActivationTime(RuleDefinitions.ActivationTime.OnRageStartAutomatic)
                    .SetEffectDescription(CreatePowerEffect(illuminatedCondition))
                    .SetRechargeRate(RuleDefinitions.RechargeRate.AtWill)
                    .SetShowCasting(false);
            }

            public static IlluminatingStrikeInitiatorBuilder Create(string name, Guid guidNamespace, ConditionDefinition illuminatedCondition)
            {
                return new IlluminatingStrikeInitiatorBuilder(name, guidNamespace, illuminatedCondition);
            }

            private static EffectDescription CreatePowerEffect(ConditionDefinition illuminatedCondition)
            {
                var initiatorCondition = ConditionDefinitionBuilder
                    .Create("PathOfTheLightIlluminatingStrikeInitiatorCondition", SubclassNamespace)
                    .SetGuiPresentationNoContent(true)
                    .SetAllowMultipleInstances(false)
                    .SetConditionType(RuleDefinitions.ConditionType.Beneficial)
                    .SetDuration(RuleDefinitions.DurationType.Minute, 1)
                    .SetTerminateWhenRemoved(true)
                   .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetSpecialInterruptions(RuleDefinitions.ConditionInterruption.RageStop)
                    .SetFeatures(
                        IlluminatingStrikeFeatureBuilder
                            .Create(IlluminatingStrikeName, SubclassNamespace, illuminatedCondition)
                            .SetGuiPresentationNoContent(AdditionalDamageDomainLifeDivineStrike.GuiPresentation.SpriteReference)
                            .AddToDB())
                    .AddToDB();

                var enableIlluminatingStrike = new EffectForm
                {
                    FormType = EffectForm.EffectFormType.Condition,
                    ConditionForm = new ConditionForm
                    {
                        Operation = ConditionForm.ConditionOperation.Add,
                        ConditionDefinition = initiatorCondition
                    }
                };

                var effectDescriptionBuilder = new EffectDescriptionBuilder();

                effectDescriptionBuilder
                    .SetDurationData(RuleDefinitions.DurationType.Minute, 1, RuleDefinitions.TurnOccurenceType.StartOfTurn)
                    .AddEffectForm(enableIlluminatingStrike);

                return effectDescriptionBuilder.Build();
            }
        }

        private sealed class PowerIlluminatingBurst : FeatureDefinitionPower, IStartOfTurnRecharge
        {
            public bool IsRechargeSilent => true;
        }

        private sealed class PowerIlluminatingBurstBuilder : FeatureDefinitionPowerBuilder<PowerIlluminatingBurst, PowerIlluminatingBurstBuilder>
        {
            private PowerIlluminatingBurstBuilder(string name, Guid guidNamespace, ConditionDefinition illuminatedCondition, ConditionDefinition illuminatingBurstSuppressedCondition) : base(name, guidNamespace)
            {
                Definition
                    .SetActivationTime(RuleDefinitions.ActivationTime.NoCost)
                    .SetEffectDescription(CreatePowerEffect(illuminatedCondition))
                    .SetRechargeRate(RuleDefinitions.RechargeRate.OneMinute) // Actually recharges at the start of your turn, using IStartOfTurnRecharge
                    .SetFixedUsesPerRecharge(1)
                    .SetUsesDetermination(RuleDefinitions.UsesDetermination.Fixed)
                    .SetCostPerUse(1)
                    .SetShowCasting(false)
                    .SetDisableIfConditionIsOwned(illuminatingBurstSuppressedCondition); // Only enabled on the turn you enter a rage
            }

            public static PowerIlluminatingBurstBuilder Create(string name, Guid guidNamespace, ConditionDefinition illuminatedCondition, ConditionDefinition illuminatingBurstSuppressedCondition)
            {
                return new PowerIlluminatingBurstBuilder(name, guidNamespace, illuminatedCondition, illuminatingBurstSuppressedCondition);
            }

            private static EffectDescription CreatePowerEffect(ConditionDefinition illuminatedCondition)
            {
                var effectDescriptionBuilder = new EffectDescriptionBuilder();

                var dealDamage = new EffectForm
                {
                    FormType = EffectForm.EffectFormType.Damage,
                    DamageForm = new DamageForm
                    {
                        DamageType = "DamageRadiant",
                        DiceNumber = 4,
                        DieType = RuleDefinitions.DieType.D6
                    },
                    SavingThrowAffinity = RuleDefinitions.EffectSavingThrowType.Negates
                };

                var illuminatedByBurstCondition = ConditionDefinitionIlluminatedByBurstBuilder
                    .Create("PathOfTheLightIlluminatedByBurstCondition", SubclassNamespace)
                    .SetGuiPresentation("BarbarianPathOfTheLightIlluminatedCondition", Category.Subclass, ConditionBranded.GuiPresentation.SpriteReference)
                    .SetAllowMultipleInstances(true)
                    .SetConditionType(RuleDefinitions.ConditionType.Detrimental)
                    .SetDuration(RuleDefinitions.DurationType.Minute, 1)
                    .SetParentCondition(illuminatedCondition)
                    .SetSilent(Silent.WhenAdded)
                    .AddToDB();

                var addIlluminatedCondition = new EffectForm
                {
                    FormType = EffectForm.EffectFormType.Condition,
                    ConditionForm = new ConditionForm
                    {
                        Operation = ConditionForm.ConditionOperation.Add,
                        ConditionDefinition = illuminatedByBurstCondition
                    },
                    CanSaveToCancel = true,
                    SaveOccurence = RuleDefinitions.TurnOccurenceType.EndOfTurn,
                    SavingThrowAffinity = RuleDefinitions.EffectSavingThrowType.Negates
                };

                EffectForm faerieFireLightSource = SpellDefinitions.FaerieFire.EffectDescription.GetFirstFormOfType(EffectForm.EffectFormType.LightSource);

                var lightSourceForm = faerieFireLightSource.LightSourceForm
                    .Copy()
                    .SetBrightRange(4)
                    .SetDimAdditionalRange(4);

                var addLightSource = new EffectForm
                {
                    FormType = EffectForm.EffectFormType.LightSource,
                    SavingThrowAffinity = RuleDefinitions.EffectSavingThrowType.Negates
                };

                addLightSource.SetLightSourceForm(lightSourceForm);

                effectDescriptionBuilder
                    .SetSavingThrowData(
                        hasSavingThrow: true,
                        disableSavingThrowOnAllies: false,
                        savingThrowAbility: "Constitution",
                        ignoreCover: false,
                        RuleDefinitions.EffectDifficultyClassComputation.AbilityScoreAndProficiency,
                        savingThrowDifficultyAbility: "Constitution",
                        fixedSavingThrowDifficultyClass: 10,
                        advantageForEnemies: false,
                        new List<SaveAffinityBySenseDescription>())
                    .SetDurationData(
                        RuleDefinitions.DurationType.Minute,
                        durationParameter: 1,
                        RuleDefinitions.TurnOccurenceType.EndOfTurn)
                    .SetTargetingData(
                        RuleDefinitions.Side.Enemy,
                        RuleDefinitions.RangeType.Distance,
                        rangeParameter: 6,
                        RuleDefinitions.TargetType.IndividualsUnique,
                        targetParameter: 3,
                        targetParameter2: 0,
                        ActionDefinitions.ItemSelectionType.None)
                    .SetSpeed(
                        RuleDefinitions.SpeedType.CellsPerSeconds,
                        speedParameter: 9.5f)
                    .SetParticleEffectParameters(SpellDefinitions.GuidingBolt.EffectDescription.EffectParticleParameters)
                    .AddEffectForm(dealDamage)
                    .AddEffectForm(addIlluminatedCondition)
                    .AddEffectForm(addLightSource);

                return effectDescriptionBuilder.Build();
            }
        }

        /// <summary>
        /// Builds the power that enables Illuminating Burst on the turn you enter a rage (by removing the condition disabling it).
        /// </summary>
        private sealed class IlluminatingBurstInitiatorBuilder : FeatureDefinitionPowerBuilder
        {
            private IlluminatingBurstInitiatorBuilder(string name, Guid guidNamespace, ConditionDefinition illuminatingBurstSuppressedCondition) : base(name, guidNamespace)
            {
                Definition
                    .SetActivationTime(RuleDefinitions.ActivationTime.OnRageStartAutomatic)
                    .SetEffectDescription(CreatePowerEffect(illuminatingBurstSuppressedCondition))
                    .SetRechargeRate(RuleDefinitions.RechargeRate.AtWill)
                    .SetShowCasting(false);
            }

            public static IlluminatingBurstInitiatorBuilder Create(string name, Guid guidNamespace, ConditionDefinition illuminatingBurstSuppressedCondition)
            {
                return new IlluminatingBurstInitiatorBuilder(name, guidNamespace, illuminatingBurstSuppressedCondition);
            }

            private static EffectDescription CreatePowerEffect(ConditionDefinition illuminatingBurstSuppressedCondition)
            {
                var enableIlluminatingBurst = new EffectForm
                {
                    FormType = EffectForm.EffectFormType.Condition,
                    ConditionForm = new ConditionForm
                    {
                        Operation = ConditionForm.ConditionOperation.Remove,
                        ConditionDefinition = illuminatingBurstSuppressedCondition
                    }
                };

                var effectDescriptionBuilder = new EffectDescriptionBuilder();

                effectDescriptionBuilder
                    .SetDurationData(RuleDefinitions.DurationType.Round, 1, RuleDefinitions.TurnOccurenceType.EndOfTurn)
                    .AddEffectForm(enableIlluminatingBurst);

                return effectDescriptionBuilder.Build();
            }
        }
    }
}
