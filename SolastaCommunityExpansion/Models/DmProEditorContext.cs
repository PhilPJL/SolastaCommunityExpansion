﻿using System.Collections.Generic;
using System.Linq;
using ModKit;
using SolastaModApi.Extensions;
using UnityEngine;

namespace SolastaCommunityExpansion.Models
{
    internal static class DmProEditorContext
    {
        private static readonly System.Guid GUID = new("bff53ba4bb694bf5a69b3ae280eec118");

        internal static readonly List<string> OutdoorRooms = new();

        internal enum ExtendedDungeonSize
        {
            Huge = UserLocationDefinitions.Size.Large + 1,
            Gargantuan
        }

        internal static void Load()
        {
            if (!Main.Settings.EnableDungeonMakerPro)
            {
                return;
            }

            UpdateAvailableDungeonSizes();
            UpdateCategories();
            LoadFlatRooms();
            UnleashGadgetsOnAllEnvironments();
            UnleashPropsOnAllEnvironments();
            UnleashRoomsOnAllEnvironments();
        }

        internal static int Compare(BaseBlueprint left, BaseBlueprint right)
        {
            var leftCategory = DatabaseRepository.GetDatabase<BlueprintCategory>().GetElement(left.Category);
            var rightCategory = DatabaseRepository.GetDatabase<BlueprintCategory>().GetElement(right.Category);
            var result = leftCategory.FormatTitle().CompareTo(rightCategory.FormatTitle());

            if (result == 0)
            {
                result = left.FormatTitle().CompareTo(right.FormatTitle());

                if (result == 0)
                {
                    result = left.GuiPresentation.SortOrder - right.GuiPresentation.SortOrder;
                }
            }

            return result;
        }

        private static void UpdateAvailableDungeonSizes()
        {
            var customDungeonSizes = new Dictionary<UserLocationDefinitions.Size, int>
            {
                { (UserLocationDefinitions.Size) ExtendedDungeonSize.Huge, 150 },
                { (UserLocationDefinitions.Size) ExtendedDungeonSize.Gargantuan, 200 }
            };

            UserLocationDefinitions.CellsBySize[UserLocationDefinitions.Size.Medium] = 75;

            foreach (var kvp in customDungeonSizes)
            {
                UserLocationDefinitions.CellsBySize.Add(kvp.Key, kvp.Value);
            }
        }

        private static void UpdateCategories()
        {
            var categories = new List<BlueprintCategory>();
            var dbBlueprintCategory = DatabaseRepository.GetDatabase<BlueprintCategory>();
            var dbEnvironmentDefinition = DatabaseRepository.GetDatabase<EnvironmentDefinition>();
            var emptyRoomsCategory = dbBlueprintCategory.GetElement("EmptyRooms");
            var flatRoomsCategory = Object.Instantiate(emptyRoomsCategory);

            flatRoomsCategory.name = "FlatRooms";
            flatRoomsCategory.SetGuid(SolastaModApi.GuidHelper.Create(GUID, flatRoomsCategory.name).ToString());
            flatRoomsCategory.GuiPresentation.Title = Gui.Format($"BlueprintCategory/&{flatRoomsCategory.name}Title").yellow();
            dbBlueprintCategory.Add(flatRoomsCategory);

            foreach (var blueprintCategory in dbBlueprintCategory)
            {
                foreach (var environmentDefinition in dbEnvironmentDefinition)
                {
                    var newBlueprintCategory = Object.Instantiate(blueprintCategory);
                    var environmentName = environmentDefinition.Name;
                    var categoryName = blueprintCategory.Name + "~" + environmentName + "~MOD";

                    newBlueprintCategory.name = categoryName;
                    newBlueprintCategory.SetGuid(SolastaModApi.GuidHelper.Create(GUID, newBlueprintCategory.name).ToString());
                    newBlueprintCategory.GuiPresentation.Title = Gui.Format(blueprintCategory.GuiPresentation.Title) + " " + Gui.Format(environmentDefinition.GuiPresentation.Title) + " [MODDED]".yellow();
                    categories.Add(newBlueprintCategory);
                }
            }

            categories.ForEach(x => dbBlueprintCategory.Add(x));
        }

        private static void LoadFlatRooms()
        {
            if (!Main.Settings.EnableDungeonMakerModdedContent)
            {
                return;
            }

            CreateFlatRooms(12); // from 12x12 to 144x144
            CreateSpecialFlatRoom("Drain_Big_24C_A", 12 + 1);
            CreateSpecialFlatRoom("Drain_Big_24C_B", 12 + 2);
        }

        private static void CreateFlatRooms(int maxMultiplier)
        {
            const string template = "Crossroad_12C";
            var dbRoomBlueprint = DatabaseRepository.GetDatabase<RoomBlueprint>();

            for (var multiplier = 1; multiplier <= maxMultiplier; multiplier++)
            {
                var flatRoom = Object.Instantiate(dbRoomBlueprint.GetElement(template));

                flatRoom.name = $"Flat{multiplier:D2}Room";
                flatRoom.SetGuid(SolastaModApi.GuidHelper.Create(GUID, flatRoom.name).ToString());
                flatRoom.GuiPresentation.Title = "Flat".yellow() + " Room";
                flatRoom.GuiPresentation.SetSortOrder(multiplier);
                flatRoom.GuiPresentation.SetHidden(true);
                flatRoom.SetCategory("FlatRooms");
                flatRoom.SetDimensions(new Vector2Int(flatRoom.Dimensions.x * multiplier, flatRoom.Dimensions.y * multiplier));
                flatRoom.SetCellInfos(new int[flatRoom.Dimensions.x * flatRoom.Dimensions.y]);
                flatRoom.SetWallSpriteReference(new UnityEngine.AddressableAssets.AssetReferenceSprite(""));
                flatRoom.SetWallAndOpeningSpriteReference(new UnityEngine.AddressableAssets.AssetReferenceSprite(""));

                for (var i = 0; i < flatRoom.CellInfos.Length; i++)
                {
                    flatRoom.CellInfos[i] = 1;
                }

                dbRoomBlueprint.Add(flatRoom);
            }
        }

        private static void CreateSpecialFlatRoom(string template, int sortOrder)
        {
            var dbRoomBlueprint = DatabaseRepository.GetDatabase<RoomBlueprint>();
            var flatRoom = Object.Instantiate(dbRoomBlueprint.GetElement(template));

            flatRoom.name = "Flat" + template;
            flatRoom.SetGuid(SolastaModApi.GuidHelper.Create(GUID, flatRoom.name).ToString());
            flatRoom.GuiPresentation.Title = "Flat".yellow() + " " + Gui.Format(flatRoom.GuiPresentation.Title);
            flatRoom.GuiPresentation.SetSortOrder(sortOrder);
            flatRoom.GuiPresentation.SetHidden(true);
            flatRoom.SetCategory("FlatRooms");

            for (var i = 0; i < flatRoom.CellInfos.Length; i++)
            {
                if (flatRoom.CellInfos[i] == 2 || flatRoom.CellInfos[i] == 3)
                {
                    flatRoom.CellInfos[i] = 1;
                }
            }

            dbRoomBlueprint.Add(flatRoom);
        }

        private static void UnleashGadgetsOnAllEnvironments()
        {
            var dbGadgetBlueprint = DatabaseRepository.GetDatabase<GadgetBlueprint>();
            var dbEnvironmentDefinition = DatabaseRepository.GetDatabase<EnvironmentDefinition>();
            var newGadgets = new List<GadgetBlueprint>();

            foreach (var gadgetBlueprint in dbGadgetBlueprint)
            {
                foreach (var prefabByEnvironment in gadgetBlueprint.PrefabsByEnvironment.Where(x => x.Environment != string.Empty))
                {
                    var environmentName = prefabByEnvironment.Environment;
                    var prefabEnvironmentDefinition = dbEnvironmentDefinition.GetElement(environmentName);
                    var newGadgetBlueprint = Object.Instantiate(gadgetBlueprint);
                    var categoryName = gadgetBlueprint.Category + "~" + environmentName + "~MOD";

                    newGadgetBlueprint.name = gadgetBlueprint.Name + "~" + environmentName + "~MOD";
                    newGadgetBlueprint.SetGuid(SolastaModApi.GuidHelper.Create(GUID, newGadgetBlueprint.name).ToString());
                    newGadgetBlueprint.GuiPresentation.Title = Gui.Format(gadgetBlueprint.GuiPresentation.Title) + " " + Gui.Format(prefabEnvironmentDefinition.GuiPresentation.Title).yellow();
                    newGadgetBlueprint.SetCategory(categoryName);
                    newGadgetBlueprint.PrefabsByEnvironment.Clear();

                    foreach (var environmentDefinition in dbEnvironmentDefinition)
                    {
                        var myPrefabByEnvironment = new BaseBlueprint.PrefabByEnvironmentDescription();

                        myPrefabByEnvironment.SetEnvironment(environmentDefinition.name);
                        myPrefabByEnvironment.SetPrefabReference(prefabByEnvironment.PrefabReference);
                        newGadgetBlueprint.PrefabsByEnvironment.Add(myPrefabByEnvironment);
                    }

                    newGadgets.TryAdd(newGadgetBlueprint);
                }
            }

            newGadgets.ForEach(x => dbGadgetBlueprint.Add(x));
            SetModdedGadgetsHiddenStatus();
        }

        private static void UnleashPropsOnAllEnvironments()
        {
            var dbPropBlueprint = DatabaseRepository.GetDatabase<PropBlueprint>();
            var dbEnvironmentDefinition = DatabaseRepository.GetDatabase<EnvironmentDefinition>();
            var newProps = new List<PropBlueprint>();

            foreach (var propBlueprint in dbPropBlueprint)
            {
                foreach (var prefabByEnvironment in propBlueprint.PrefabsByEnvironment.Where(x => x.Environment != string.Empty))
                {
                    var environmentName = prefabByEnvironment.Environment;
                    var prefabEnvironmentDefinition = dbEnvironmentDefinition.GetElement(environmentName);
                    var newPropBlueprint = Object.Instantiate(propBlueprint);
                    var categoryName = propBlueprint.Category + "~" + environmentName + "~MOD";

                    newPropBlueprint.name = propBlueprint.Name + "~" + environmentName + "~MOD";
                    newPropBlueprint.SetGuid(SolastaModApi.GuidHelper.Create(GUID, newPropBlueprint.name).ToString());
                    newPropBlueprint.GuiPresentation.Title = Gui.Format(propBlueprint.GuiPresentation.Title) + " " + Gui.Format(prefabEnvironmentDefinition.GuiPresentation.Title).yellow();
                    newPropBlueprint.SetCategory(categoryName);
                    newPropBlueprint.PrefabsByEnvironment.Clear();

                    foreach (EnvironmentDefinition environmentDefinition in dbEnvironmentDefinition)
                    {
                        var myPrefabByEnvironment = new BaseBlueprint.PrefabByEnvironmentDescription();

                        myPrefabByEnvironment.SetEnvironment(environmentDefinition.name);
                        myPrefabByEnvironment.SetPrefabReference(prefabByEnvironment.PrefabReference);
                        newPropBlueprint.PrefabsByEnvironment.Add(myPrefabByEnvironment);
                    }

                    newProps.TryAdd(newPropBlueprint);
                }
            }

            newProps.ForEach(x => dbPropBlueprint.Add(x));
            SetModdedPropsHiddenStatus();
        }

        private static void UnleashRoomsOnAllEnvironments()
        {
            var dbRoomBlueprint = DatabaseRepository.GetDatabase<RoomBlueprint>();
            var dbEnvironmentDefinition = DatabaseRepository.GetDatabase<EnvironmentDefinition>();
            var newRooms = new List<RoomBlueprint>();

            foreach (var roomBlueprint in dbRoomBlueprint
                .Where(x => x.Category == "EmptyRooms" || x.Category == "FlatRooms"))
            {
                foreach (var prefabByEnvironment in roomBlueprint.PrefabsByEnvironment
                    .Where(x => x.Environment != string.Empty))
                {
                    var environmentName = prefabByEnvironment.Environment;
                    var prefabEnvironmentDefinition = dbEnvironmentDefinition.GetElement(environmentName);
                    var newRoomBlueprint = Object.Instantiate(roomBlueprint);
                    var categoryName = roomBlueprint.Category + "~" + environmentName + "~MOD";

                    newRoomBlueprint.name = roomBlueprint.Name + "~" + environmentName + "~MOD";
                    newRoomBlueprint.SetGuid(SolastaModApi.GuidHelper.Create(GUID, newRoomBlueprint.name).ToString());
                    newRoomBlueprint.GuiPresentation.Title = Gui.Format(roomBlueprint.GuiPresentation.Title) + " " + Gui.Format(prefabEnvironmentDefinition.GuiPresentation.Title).yellow();
                    newRoomBlueprint.SetCategory(categoryName);
                    newRoomBlueprint.GuiPresentation.SetHidden(false);
                    newRoomBlueprint.PrefabsByEnvironment.Clear();

                    foreach (var environmentDefinition in dbEnvironmentDefinition
                        .Where(x => !prefabEnvironmentDefinition.Outdoor || x.Outdoor))
                    {
                        var myPrefabByEnvironment = new BaseBlueprint.PrefabByEnvironmentDescription();

                        myPrefabByEnvironment.SetEnvironment(environmentDefinition.name);
                        myPrefabByEnvironment.SetPrefabReference(prefabByEnvironment.PrefabReference);
                        newRoomBlueprint.PrefabsByEnvironment.Add(myPrefabByEnvironment);
                    }

                    newRooms.Add(newRoomBlueprint);

                    if (prefabEnvironmentDefinition.Outdoor)
                    {
                        OutdoorRooms.Add(newRoomBlueprint.Name);

                        if (!OutdoorRooms.Contains(roomBlueprint.Name))
                        {
                            OutdoorRooms.Add(roomBlueprint.Name);
                        }
                    }
                }
            }

            newRooms.ForEach(x => dbRoomBlueprint.Add(x));
            SetModdedRoomsHiddenStatus();
        }

        private static void SetModdedGadgetsHiddenStatus()
        {
            var dbGadgetBlueprint = DatabaseRepository.GetDatabase<GadgetBlueprint>();

            dbGadgetBlueprint
                .Where(x => x.Name.EndsWith("MOD")).ToList()
                .ForEach(y => y.GuiPresentation.SetHidden(!Main.Settings.EnableDungeonMakerModdedContent));
        }

        private static void SetModdedPropsHiddenStatus()
        {
            var dbPropBlueprint = DatabaseRepository.GetDatabase<PropBlueprint>();

            dbPropBlueprint
                .Where(x => x.Name.EndsWith("MOD")).ToList()
                .ForEach(y => y.GuiPresentation.SetHidden(!Main.Settings.EnableDungeonMakerModdedContent));
        }

        private static void SetModdedRoomsHiddenStatus()
        {
            var dbRoomBlueprint = DatabaseRepository.GetDatabase<RoomBlueprint>();

            dbRoomBlueprint
                .Where(x => x.Name.EndsWith("MOD")).ToList()
                .ForEach(y => y.GuiPresentation.SetHidden(!Main.Settings.EnableDungeonMakerModdedContent));

            dbRoomBlueprint
                .Where(x => x.Name.StartsWith("Flat")).ToList()
                .ForEach(y => y.GuiPresentation.SetHidden(!Main.Settings.EnableDungeonMakerModdedContent));
        }
    }
}
