﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static SolastaCommunityExpansion.Models.RespecContext;

namespace SolastaCommunityExpansion.Models
{
    internal static class LevelDownContext
    {
        public static bool IsLevelDown { get; set; } = false;

        public class FunctorLevelDown : Functor
        {
            public override IEnumerator Execute(
                FunctorParametersDescription functorParameters,
                FunctorExecutionContext context)
            {
                if (Global.IsMultiplayer)
                {
                    Gui.GuiService.ShowMessage(
                        MessageModal.Severity.Informative1,
                        "MainMenu/&ExportPdfTitle", "Message/&LevelDownMultiplayerAbortDescription",
                        "Message/&MessageOkTitle", string.Empty,
                        null, null);

                    yield break;
                }

                var state = -1;

                Gui.GuiService.ShowMessage(
                    MessageModal.Severity.Attention2,
                    "MainMenu/&ExportPdfTitle", "Message/&LevelDownConfirmationDescription",
                    "Message/&MessageYesTitle", "Message/&MessageNoTitle",
                    new MessageModal.MessageValidatedHandler(() => state = 1),
                    new MessageModal.MessageCancelledHandler(() => state = 0));

                while (state < 0)
                {
                    yield return null;
                }

                if (state > 0)
                {
                    if (functorParameters.RestingHero.ClassesHistory.Count > 1)
                    {
                        LevelDown(functorParameters.RestingHero);
                    }
                    else
                    {
                        yield return new FunctorRespec().Execute(functorParameters, context);
                    }
                }
            }
        }

        internal static void Load()
        {
            ServiceRepository.GetService<IFunctorService>().RegisterFunctor("LevelDown", new FunctorLevelDown());
        }

        internal static void ConfirmAndExecute(string filename)
        {
            var service = ServiceRepository.GetService<ICharacterPoolService>();

            service.LoadCharacter(filename, out var rulesetCharacterHero, out _);

            Gui.GuiService.ShowMessage(
                MessageModal.Severity.Attention2,
                "MainMenu/&ExportPdfTitle", "Message/&LevelDownConfirmationDescription",
                "Message/&MessageYesTitle", "Message/&MessageNoTitle",
                () => LevelDown(rulesetCharacterHero), null);
        }

        private static void RemoveFeaturesByTag(RulesetCharacterHero hero, CharacterClassDefinition classDefinition, string tag)
        {
            if (hero.ActiveFeatures.ContainsKey(tag))
            {
                CustomFeaturesContext.RecursiveRemoveCustomFeatures(hero, tag, hero.ActiveFeatures[tag]);
                CustomFeaturesContext.RemoveFeatures(hero, classDefinition, tag, hero.ActiveFeatures[tag]);

                hero.ActiveFeatures.Remove(tag);
            }
        }

        internal static void LevelDown(RulesetCharacterHero hero)
        {
            var indexLevel = hero.ClassesHistory.Count - 1;
            var characterClassDefinition = hero.ClassesHistory.Last();
            var classLevel = hero.ClassesAndLevels[characterClassDefinition];
            var classTag = AttributeDefinitions.GetClassTag(characterClassDefinition, classLevel);
            var subclassTag = string.Empty;

            IsLevelDown = true;

            var characterBuildingService = ServiceRepository.GetService<ICharacterBuildingService>();

            characterBuildingService.LevelUpCharacter(hero, false);

            hero.ClassesAndSubclasses.TryGetValue(characterClassDefinition, out var characterSubclassDefinition);

            if (characterSubclassDefinition != null)
            {
                subclassTag = AttributeDefinitions.GetSubclassTag(characterClassDefinition, classLevel, characterSubclassDefinition);
            }

            LevelUpContext.RegisterHero(hero, characterClassDefinition, characterSubclassDefinition);

            UnlearnSpells(hero, indexLevel);

            if (subclassTag != classTag)
            {
                RemoveFeaturesByTag(hero, characterClassDefinition, subclassTag);
                RemoveFeaturesByTag(hero, characterClassDefinition, CustomFeaturesContext.CustomizeTag(subclassTag));
            }

            RemoveFeaturesByTag(hero, characterClassDefinition, classTag);
            RemoveFeaturesByTag(hero, characterClassDefinition, CustomFeaturesContext.CustomizeTag(classTag));

            hero.RemoveClassLevel();
            hero.RefreshAll();
            hero.ComputeHitPoints(true);

            characterBuildingService.FinalizeCharacter(hero);

            LevelUpContext.UnregisterHero(hero);

            if (Gui.Game == null)
            {
                ServiceRepository.GetService<ICharacterPoolService>().SaveCharacter(hero, true);
            }
            
            IsLevelDown = false;
        }

        private static void UnlearnSpells(RulesetCharacterHero hero, int indexLevel)
        {
            var heroRepertoire = hero.SpellRepertoires.FirstOrDefault(x => LevelUpContext.IsRepertoireFromSelectedClassSubclass(hero, x));

            if (heroRepertoire == null)
            {
                return;
            }

            var cantripsToRemove = heroRepertoire.SpellCastingFeature.KnownCantrips[indexLevel] - heroRepertoire.SpellCastingFeature.KnownCantrips[indexLevel - 1];

            heroRepertoire.PreparedSpells.Clear();

            while (cantripsToRemove-- > 0)
            {
                heroRepertoire.KnownCantrips.RemoveAt(heroRepertoire.KnownCantrips.Count - 1);
            }

            switch (heroRepertoire.SpellCastingFeature.SpellKnowledge)
            {
                case RuleDefinitions.SpellKnowledge.Spellbook:
                    var scribbedSpellsToRemove = heroRepertoire.SpellCastingFeature.ScribedSpells[indexLevel];
                    var rulesetItemSpellbooks = new List<RulesetItemSpellbook>();

                    hero.CharacterInventory.BrowseAllCarriedItems(rulesetItemSpellbooks);

                    if (rulesetItemSpellbooks.Count > 0)
                    {
                        var rulesetItemSpellbook = rulesetItemSpellbooks[0];

                        while (scribbedSpellsToRemove-- > 0)
                        {
                            rulesetItemSpellbook.ScribedSpells.RemoveAt(rulesetItemSpellbook.ScribedSpells.Count - 1);
                        }
                    }

                    break;

                case RuleDefinitions.SpellKnowledge.Selection:
                    var spellsToRemove = heroRepertoire.SpellCastingFeature.KnownSpells[indexLevel] - heroRepertoire.SpellCastingFeature.KnownSpells[indexLevel - 1];

                    while (spellsToRemove-- > 0)
                    {
                        heroRepertoire.KnownSpells.RemoveAt(heroRepertoire.KnownSpells.Count - 1);
                    }

                    break;
            }
        }
    }
}
