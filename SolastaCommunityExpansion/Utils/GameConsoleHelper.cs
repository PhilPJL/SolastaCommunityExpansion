﻿using SolastaModApi.Infrastructure;

namespace SolastaCommunityExpansion.Utils
{
    public static class GameConsoleHelper
    {
        private const string DefaultUseText = "Feedback/&ActivatePowerLine";
        public static void LogCharacterUsedPower(RulesetCharacter character, FeatureDefinitionPower power, string text = DefaultUseText)
        {
            var abilityName = string.IsNullOrEmpty(power.ShortTitleOverride) ? power.GuiPresentation.Title : power.ShortTitleOverride;
            LogCharacterActivatesAbility(character, abilityName, text);
        }
        
        public static void LogCharacterActivatesAbility(RulesetCharacter character, string abilityName, string text = DefaultUseText)
        {
            var console = Gui.Game.GameConsole;
            var characterName = character is RulesetCharacterHero hero ? hero.DisplayName : character.Name;

            var entry = new GameConsoleEntry(text, console.GetField<ConsoleTableDefinition>("consoleTableDefinition"));
            entry.AddParameter(ConsoleStyleDuplet.ParameterType.Player, characterName);
            entry.AddParameter(ConsoleStyleDuplet.ParameterType.AttackSpellPower, abilityName);
            console.AddEntry(entry);
        }
        
        public static void LogCharacterAffectsTarget(RulesetCharacter character, RulesetCharacter target, string notificationTag, bool indent = false)
        {
            var console = Gui.Game.GameConsole;
            var text = $"Feedback/&NotifyEffect{notificationTag}Line";
            var entry = new GameConsoleEntry(text, console.GetField<ConsoleTableDefinition>("consoleTableDefinition"))
            {
                Indent = indent
            };

            console.InvokeMethod("AddCharacterEntry", character, entry);
            console.InvokeMethod("AddCharacterEntry",target, entry);
            console.AddEntry(entry);
        }
    }
}
