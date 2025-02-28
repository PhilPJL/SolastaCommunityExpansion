﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using SolastaCommunityExpansion.Subclasses;
using SolastaCommunityExpansion.Subclasses.Barbarian;
using SolastaCommunityExpansion.Subclasses.Druid;
using SolastaCommunityExpansion.Subclasses.Fighter;
using SolastaCommunityExpansion.Subclasses.Ranger;
using SolastaCommunityExpansion.Subclasses.Rogue;
using SolastaCommunityExpansion.Subclasses.Wizard;

namespace SolastaCommunityExpansion.Models
{
    internal static class SubclassesContext
    {
        private static Dictionary<CharacterSubclassDefinition, FeatureDefinitionSubclassChoice> SubclassesChoiceList { get; set; } = new();

        internal static HashSet<CharacterSubclassDefinition> Subclasses { get; private set; } = new();

        private static void SortSubclassesFeatures()
        {
            var dbCharacterSubclassDefinition = DatabaseRepository.GetDatabase<CharacterSubclassDefinition>();

            foreach (var characterSubclassDefinition in dbCharacterSubclassDefinition)
            {
                characterSubclassDefinition.FeatureUnlocks.Sort((a, b) =>
                {
                    var result = a.Level - b.Level;

                    if (result == 0)
                    {
                        result = a.FeatureDefinition.FormatTitle().CompareTo(b.FeatureDefinition.FormatTitle());
                    }

                    return result;
                });
            }
        }

        internal static void Load()
        {
            LoadSubclass(new ArcaneFighter());
            LoadSubclass(new Arcanist());
            LoadSubclass(new CircleOfTheForestGuardian());
            LoadSubclass(new ConArtist());
            LoadSubclass(new LifeTransmuter());
            LoadSubclass(new MasterManipulator());
            LoadSubclass(new Opportunist());
            LoadSubclass(new PathOfTheLight());
            LoadSubclass(new SpellMaster());
            LoadSubclass(new SpellShield());
            LoadSubclass(new Tactician());

            Subclasses = Subclasses.OrderBy(x => x.FormatTitle()).ToHashSet();

            if (Main.Settings.EnableSortingFutureFeatures)
            {
                SortSubclassesFeatures();
            }

            ArcaneFighter.UpdateEnchantWeapon();
            ConArtist.UpdateSpellDCBoost();
            MasterManipulator.UpdateSpellDCBoost();
            SpellMaster.UpdateBonusRecovery();
        }

        private static void LoadSubclass(AbstractSubclass subclassBuilder)
        {
            var subclass = subclassBuilder.GetSubclass();

            if (!Subclasses.Contains(subclass))
            {
                SubclassesChoiceList.Add(subclass, subclassBuilder.GetSubclassChoiceList());
                Subclasses.Add(subclass);
            }

            UpdateSubclassVisibility(subclass);
        }

        private static void UpdateSubclassVisibility(CharacterSubclassDefinition characterSubclassDefinition)
        {
            var name = characterSubclassDefinition.Name;
            var choiceList = SubclassesChoiceList[characterSubclassDefinition];

            if (Main.Settings.SubclassEnabled.Contains(name))
            {
                choiceList.Subclasses.TryAdd(name);
            }
            else
            {
                choiceList.Subclasses.Remove(name);
            }
        }

        internal static void Switch(CharacterSubclassDefinition characterSubclassDefinition, bool active)
        {
            if (!Subclasses.Contains(characterSubclassDefinition))
            {
                return;
            }

            var name = characterSubclassDefinition.Name;

            if (active)
            {
                Main.Settings.SubclassEnabled.TryAdd(name);
            }
            else
            {
                Main.Settings.SubclassEnabled.Remove(name);
            }

            UpdateSubclassVisibility(characterSubclassDefinition);
        }
    }
}
