﻿using System;
using System.Collections.Generic;
using System.Linq;
using static FeatureDefinitionCastSpell;
using static SolastaCommunityExpansion.Classes.Warlock.WarlockSpells;
using static SolastaCommunityExpansion.Level20.SpellsHelper;
using static SolastaModApi.DatabaseHelper.CharacterClassDefinitions;
using static SolastaModApi.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaCommunityExpansion.Models.IntegrationContext;

namespace SolastaCommunityExpansion.Models
{
    public enum CasterType
    {
        None = 0,
        Full = 2,
        Half = 4,
        HalfRoundUp = 5,
        OneThird = 6
    }

    public static class SharedSpellsContext
    {
        public static Dictionary<string, BaseDefinition> RecoverySlots { get; } = new()
        {
            { "TinkererSpellStoringItem", TinkererClass },
            { "ArtificerInfusionSpellRefuelingRing", TinkererClass },
            { "PowerAlchemistSpellBonusRecovery", TinkererClass },
            { "PowerCircleLandNaturalRecovery", Druid },
            { "PowerWizardArcaneRecovery", Wizard },
            { "PowerSpellMasterBonusRecovery", Wizard }
        };

        public static Dictionary<CharacterClassDefinition, CasterType> ClassCasterType { get; } = new()
        {
            { Cleric, CasterType.Full },
            { Druid, CasterType.Full },
            { Sorcerer, CasterType.Full },
            { Wizard, CasterType.Full },
            { Paladin, CasterType.Half },
            { Ranger, CasterType.Half }
            // added during load
            //{ TinkererClass, CasterType.HalfRoundUp },
            //{ WitchClass, CasterType.Full },
        };

        public static Dictionary<CharacterSubclassDefinition, CasterType> SubclassCasterType { get; } = new()
        {
            { MartialSpellblade, CasterType.OneThird },
            { RoguishShadowCaster, CasterType.OneThird }
            // added during load
            //{ ConArtistSubclass, CasterType.OneThird }, // ChrisJohnDigital
            //{ SpellShieldSubclass, CasterType.OneThird } // ChrisJohnDigital
        };

        public class CasterLevelContext
        {
            private readonly Dictionary<CasterType, int> levels;

            public CasterLevelContext()
            {
                levels = new()
                {
                    { CasterType.None, 0 },
                    { CasterType.Full, 0 },
                    { CasterType.Half, 0 },
                    { CasterType.HalfRoundUp, 0 },
                    { CasterType.OneThird, 0 },
                };
            }

            public void IncrementCasterLevel(CasterType casterType, int increment) => levels[casterType] += increment;

            public int GetCasterLevel()
            {
                var casterLevel = 0;

                // Full Casters
                casterLevel += levels[CasterType.Full];

                // Tinkerer / ...
                if (levels[CasterType.HalfRoundUp] == 1)
                {
                    casterLevel++;
                }
                // Half Casters
                else
                {
                    casterLevel += (int)Math.Floor(levels[CasterType.HalfRoundUp] / 2.0);
                }

                casterLevel += (int)Math.Floor(levels[CasterType.Half] / 2.0);

                // Con Artist / ...
                casterLevel += (int)Math.Floor(levels[CasterType.OneThird] / 3.0);

                return casterLevel;
            }
        }

        private static CasterType GetCasterTypeForClassOrSubclass(CharacterClassDefinition characterClassDefinition, CharacterSubclassDefinition characterSubclassDefinition)
        {
            if (characterClassDefinition != null && ClassCasterType.ContainsKey(characterClassDefinition))
            {
                return ClassCasterType[characterClassDefinition];
            }

            if (characterSubclassDefinition != null && SubclassCasterType.ContainsKey(characterSubclassDefinition))
            {
                return SubclassCasterType[characterSubclassDefinition];
            }

            return CasterType.None;
        }

        public static RulesetCharacterHero GetHero(string name)
        {
            // try to get hero from game campaign
            var gameCampaign = Gui.GameCampaign;

            if (gameCampaign != null)
            {
                var gameCampaignCharacter = gameCampaign.Party.CharactersList.Find(x => x.RulesetCharacter.Name == name);

                if (gameCampaignCharacter != null
                    && gameCampaignCharacter.RulesetCharacter is RulesetCharacterHero rulesetCharacterHero)
                {
                    return rulesetCharacterHero;
                }
            }

            // otherwise gets hero from level up
            var hero = Global.ActiveLevelUpHero;

            if (hero != null)
            {
                return hero;
            }

            // finally falls back to inspection [when browsing hero in char pool]
            return Global.InspectedHero;
        }

        public static bool IsWarlock(CharacterClassDefinition characterClassDefinition) =>
            characterClassDefinition == WarlockClass;

        // need the null check for companions who don't have repertoires
        public static bool IsMulticaster(RulesetCharacterHero rulesetCharacterHero) =>
            rulesetCharacterHero != null
            && rulesetCharacterHero.SpellRepertoires
                .Count(sr => sr.SpellCastingFeature.SpellCastingOrigin != CastingOrigin.Race) > 1;

        // need the null check for companions who don't have repertoires
        public static bool IsSharedcaster(RulesetCharacterHero rulesetCharacterHero) =>
            rulesetCharacterHero != null
            && rulesetCharacterHero.SpellRepertoires
                .Where(sr => sr.SpellCastingClass != WarlockClass)
                .Count(sr => sr.SpellCastingFeature.SpellCastingOrigin != CastingOrigin.Race) > 1;

        // need the null check for companions who don't have repertoires
        private static int GetWarlockLevel(RulesetCharacterHero rulesetCharacterHero)
        {
            if (rulesetCharacterHero == null)
            {
                return 0;
            }

            var warlockLevel = 0;
            var warlock = rulesetCharacterHero.ClassesAndLevels.Keys.FirstOrDefault(x => x == WarlockClass);

            if (warlock != null)
            {
                warlockLevel = rulesetCharacterHero.ClassesAndLevels[warlock];
            }

            return warlockLevel;
        }

        public static int GetWarlockSpellLevel(RulesetCharacterHero rulesetCharacterHero)
        {
            var warlockLevel = GetWarlockLevel(rulesetCharacterHero);

            if (warlockLevel > 0)
            {
                return WarlockCastingSlots[warlockLevel - 1].Slots.IndexOf(0);
            }

            return 0;
        }

        public static int GetWarlockMaxSlots(RulesetCharacterHero rulesetCharacterHero)
        {
            var warlockLevel = GetWarlockLevel(rulesetCharacterHero);

            if (warlockLevel > 0)
            {
                return WarlockCastingSlots[warlockLevel - 1].Slots[0];
            }

            return 0;
        }

        public static RulesetSpellRepertoire GetWarlockSpellRepertoire(RulesetCharacterHero rulesetCharacterHero) =>
            rulesetCharacterHero.SpellRepertoires.FirstOrDefault(x => IsWarlock(x.SpellCastingClass));

        public static int GetSharedCasterLevel(RulesetCharacterHero rulesetCharacterHero)
        {
            if (rulesetCharacterHero == null || rulesetCharacterHero.ClassesAndLevels == null)
            {
                return 0;
            }

            var casterLevelContext = new CasterLevelContext();

            foreach (var classAndLevel in rulesetCharacterHero.ClassesAndLevels)
            {
                var currentCharacterClassDefinition = classAndLevel.Key;

                rulesetCharacterHero.ClassesAndSubclasses.TryGetValue(currentCharacterClassDefinition, out var currentCharacterSubclassDefinition);

                var casterType = GetCasterTypeForClassOrSubclass(currentCharacterClassDefinition, currentCharacterSubclassDefinition);

                casterLevelContext.IncrementCasterLevel(casterType, classAndLevel.Value);
            }

            return casterLevelContext.GetCasterLevel();
        }

        public static int GetSharedSpellLevel(RulesetCharacterHero rulesetCharacterHero)
        {
            if (!IsSharedcaster(rulesetCharacterHero))
            {
                var repertoire = rulesetCharacterHero.SpellRepertoires
                    .Find(x => x.SpellCastingFeature.SpellCastingOrigin != CastingOrigin.Race && x.SpellCastingClass != WarlockClass);

                return GetClassSpellLevel(repertoire);
            }

            var sharedCasterLevel = GetSharedCasterLevel(rulesetCharacterHero);

            if (sharedCasterLevel > 0)
            {
                return FullCastingSlots[sharedCasterLevel - 1].Slots.IndexOf(0);
            }

            return 0;
        }

        public static int GetClassSpellLevel(RulesetSpellRepertoire spellRepertoire)
        {
            if (spellRepertoire != null && spellRepertoire.SpellCastingFeature.SlotsPerLevels != null)
            {
                var slotsPerLevel = spellRepertoire.SpellCastingFeature.SlotsPerLevels[spellRepertoire.SpellCastingLevel - 1];

                return slotsPerLevel.Slots.IndexOf(0);
            }

            return 0;
        }

        public static void Load()
        {
            ClassCasterType.Add(TinkererClass, CasterType.HalfRoundUp);
            ClassCasterType.Add(WitchClass, CasterType.Full);
            SubclassCasterType.Add(ConArtistSubclass, CasterType.OneThird);
            SubclassCasterType.Add(SpellShieldSubclass, CasterType.OneThird);
        }

        public const int MC_PACT_MAGIC_SLOT_TAB_INDEX = -1;
    }
}
