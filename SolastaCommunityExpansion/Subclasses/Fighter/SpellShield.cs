﻿using System;
using SolastaCommunityExpansion.Builders;
using SolastaCommunityExpansion.Builders.Features;
using static SolastaModApi.DatabaseHelper;
using static SolastaModApi.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaModApi.DatabaseHelper.ConditionDefinitions;

namespace SolastaCommunityExpansion.Subclasses.Fighter
{
    internal class SpellShield : AbstractSubclass
    {
        private static Guid SubclassNamespace = new("d4732dc2-c4f9-4a35-a12a-ae2d7858ff74");
        private readonly CharacterSubclassDefinition Subclass;

        internal override FeatureDefinitionSubclassChoice GetSubclassChoiceList()
        {
            return FeatureDefinitionSubclassChoices.SubclassChoiceFighterMartialArchetypes;
        }
        internal override CharacterSubclassDefinition GetSubclass()
        {
            return Subclass;
        }

        internal SpellShield()
        {
            FeatureDefinitionMagicAffinity magicAffinity = FeatureDefinitionMagicAffinityBuilder
                .Create("MagicAffinityFighterSpellShield", SubclassNamespace)
                .SetGuiPresentation(Category.Subclass)
                .SetConcentrationModifiers(RuleDefinitions.ConcentrationAffinity.Advantage, 0)
                .SetHandsFullCastingModifiers(true, true, true)
                .SetCastingModifiers(0, RuleDefinitions.SpellParamsModifierType.None, 0, RuleDefinitions.SpellParamsModifierType.FlatValue, true, false, false)
                .AddToDB();

            FeatureDefinitionCastSpellBuilder spellCasting = FeatureDefinitionCastSpellBuilder
                .Create("CastSpellSpellShield", SubclassNamespace)
                .SetGuiPresentation("FighterSpellShieldSpellcasting", Category.Subclass)
                .SetSpellCastingOrigin(FeatureDefinitionCastSpell.CastingOrigin.Subclass)
                .SetSpellCastingAbility(AttributeDefinitions.Intelligence)
                .SetSpellList(SpellListDefinitions.SpellListWizard)
                .AddRestrictedSchool(SchoolOfMagicDefinitions.SchoolAbjuration)
                .AddRestrictedSchool(SchoolOfMagicDefinitions.SchoolTransmutation)
                .AddRestrictedSchool(SchoolOfMagicDefinitions.SchoolNecromancy)
                .AddRestrictedSchool(SchoolOfMagicDefinitions.SchoolIllusion)
                .SetSpellKnowledge(RuleDefinitions.SpellKnowledge.Selection)
                .SetSpellReadyness(RuleDefinitions.SpellReadyness.AllKnown)
                .SetSlotsRecharge(RuleDefinitions.RechargeRate.LongRest)
                .SetKnownCantrips(3, 3, FeatureDefinitionCastSpellBuilder.CasterProgression.THIRD_CASTER)
                .SetKnownSpells(4, 3, FeatureDefinitionCastSpellBuilder.CasterProgression.THIRD_CASTER)
                .SetSlotsPerLevel(3, FeatureDefinitionCastSpellBuilder.CasterProgression.THIRD_CASTER);

            FeatureDefinitionSavingThrowAffinity spellShieldResistance = FeatureDefinitionSavingThrowAffinityBuilder
                .Create("SpellShieldSpellResistance", SubclassNamespace)
                .SetGuiPresentation("FighterSpellShieldSpellResistance", Category.Subclass)
                .SetAffinities(RuleDefinitions.CharacterSavingThrowAffinity.Advantage, true,
                    AttributeDefinitions.Strength,
                    AttributeDefinitions.Dexterity,
                    AttributeDefinitions.Constitution,
                    AttributeDefinitions.Wisdom,
                    AttributeDefinitions.Intelligence,
                    AttributeDefinitions.Charisma)
                .AddToDB();

            // or maybe some boost to the spell shield spells?

            FeatureDefinitionAdditionalAction bonusSpell = FeatureDefinitionAdditionalActionBuilder
                .Create("SpellShieldAdditionalAction", SubclassNamespace)
                .SetGuiPresentation(Category.Subclass)
                .SetActionType(ActionDefinitions.ActionType.Main)
                .SetRestrictedActions(ActionDefinitions.Id.CastMain)
                .SetMaxAttacksNumber(-1)
                .SetTriggerCondition(RuleDefinitions.AdditionalActionTriggerCondition.HasDownedAnEnemy)
                .AddToDB();

            ConditionDefinition deflectionCondition = ConditionDefinitionBuilder
                .Create("ConditionSpellShieldArcaneDeflection", SubclassNamespace)
                .SetGuiPresentation(Category.Subclass)
                .AddFeatures(FeatureDefinitionAttributeModifierBuilder
                    .Create("AttributeSpellShieldArcaneDeflection", SubclassNamespace)
                    .SetModifier(FeatureDefinitionAttributeModifier.AttributeModifierOperation.Additive, AttributeDefinitions.ArmorClass, 3)
                    .SetGuiPresentation("ConditionSpellShieldArcaneDeflection", Category.Subclass, ConditionShielded.GuiPresentation.SpriteReference)
                    .AddToDB())
                .SetConditionType(RuleDefinitions.ConditionType.Beneficial)
                .SetAllowMultipleInstances(false)
                .SetDuration(RuleDefinitions.DurationType.Round, 1)
                .AddToDB();

            var arcaneDeflection = EffectDescriptionBuilder
                .Create()
                .SetTargetingData(RuleDefinitions.Side.Ally, RuleDefinitions.RangeType.Self, 1, RuleDefinitions.TargetType.Self, 1, 0, ActionDefinitions.ItemSelectionType.None)
                .AddEffectForm(EffectFormBuilder
                    .Create()
                    .CreatedByCharacter()
                    .SetConditionForm(deflectionCondition, ConditionForm.ConditionOperation.Add, true, true)
                    .Build())
                .Build();

            FeatureDefinitionPower arcaneDeflectionPower = FeatureDefinitionPowerBuilder
                .Create("PowerSpellShieldArcaneDeflection", SubclassNamespace)
                .SetGuiPresentation(Category.Subclass, ConditionShielded.GuiPresentation.SpriteReference)
                .Configure(
                    0, RuleDefinitions.UsesDetermination.AbilityBonusPlusFixed, AttributeDefinitions.Intelligence, RuleDefinitions.ActivationTime.Reaction, 0, RuleDefinitions.RechargeRate.AtWill,
                    false, false, AttributeDefinitions.Intelligence, arcaneDeflection, false /* unique instance */)
                .AddToDB();

            var actionAffinitySpellShieldRangedDefense = FeatureDefinitionActionAffinityBuilder
                .Create(FeatureDefinitionActionAffinitys.ActionAffinityTraditionGreenMageLeafScales, "ActionAffinitySpellShieldRangedDefense", SubclassNamespace)
                .SetGuiPresentation("PowerSpellShieldRangedDeflection", Category.Subclass)
                .AddToDB();

            // Make Spell Shield subclass

            Subclass = CharacterSubclassDefinitionBuilder
                .Create("FighterSpellShield", SubclassNamespace)
                .SetGuiPresentation(Category.Subclass, DomainBattle.GuiPresentation.SpriteReference)
                .AddFeatureAtLevel(magicAffinity, 3)
                .AddFeatureAtLevel(spellCasting.AddToDB(), 3)
                .AddFeatureAtLevel(spellShieldResistance, 7)
                .AddFeatureAtLevel(bonusSpell, 10)
                .AddFeatureAtLevel(arcaneDeflectionPower, 15)
                .AddFeatureAtLevel(actionAffinitySpellShieldRangedDefense, 18).AddToDB();
        }
    }
}
