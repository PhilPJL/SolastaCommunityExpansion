﻿//using Newtonsoft.Json;
//using System.IO;
//using System.Reflection;
//using static UnityModManagerNet.UnityModManager;
//using System;

namespace ModKit {
    public interface IUpdatableSettings {
        void AddMissingKeys(IUpdatableSettings from);
    }

    //internal static class ModSettings {
    //    public static void SaveSettings<T>(this ModEntry modEntry, string fileName, T settings) {
    //        var userConfigFolder = modEntry.Path + "UserSettings";
    //        Directory.CreateDirectory(userConfigFolder);
    //        var userPath = $"{userConfigFolder}{Path.DirectorySeparatorChar}{fileName}";
    //        File.WriteAllText(userPath, JsonConvert.SerializeObject(settings, Formatting.Indented));
    //    }
    //    public static void LoadSettings<T>(this ModEntry modEntry, string fileName, ref T settings) where T : IUpdatableSettings, new() {
    //        var assembly = Assembly.GetExecutingAssembly();
    //        var userConfigFolder = modEntry.Path + "UserSettings";
    //        Directory.CreateDirectory(userConfigFolder);
    //        var userPath = $"{userConfigFolder}{Path.DirectorySeparatorChar}{fileName}";
    //        try {
    //            foreach (var res in assembly.GetManifestResourceNames()) {
    //                //Logger.Log("found resource: " + res);
    //                if (res.Contains(fileName)) {
    //                    var stream = assembly.GetManifestResourceStream(res);
    //                    using StreamReader reader = new(stream);
    //                    var text = reader.ReadToEnd();
    //                    //Logger.Log($"read: {text}");
    //                    settings = JsonConvert.DeserializeObject<T>(text);

    //                    //Logger.Log($"read settings: {string.Join(Environment.NewLine, settings)}");
    //                }
    //            }
    //        }
    //        catch {
    //            settings = new T { };
    //        }
    //        if (File.Exists(userPath)) {
    //            using var reader = File.OpenText(userPath);
    //            try {
    //                var userSettings = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
    //                userSettings.AddMissingKeys(settings);
    //                settings = userSettings;
    //            }
    //            catch {
    //                try { File.Copy(userPath, userConfigFolder + $"{Path.DirectorySeparatorChar}BROKEN_{fileName}", true); }
    //                catch {  }
    //            }
    //        }
    //        File.WriteAllText(userPath, JsonConvert.SerializeObject(settings, Formatting.Indented));
    //    }
    //}
}
