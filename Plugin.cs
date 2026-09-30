using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace FloatingItems
{
    // Wyrzucone przedmioty unosza sie na wodzie zamiast tonac. Nie ma tu wlasnej fizyki: kazdy
    // przedmiot dostaje komponent Floating gry - ten sam, dzieki ktoremu w grze plywa drewno - z
    // ustawieniami skopiowanymi z przedmiotu, ktory plywa w grze od siebie.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.ab5olutezer0.valheim.floatingitems";
        public const string PluginName = "Floating Items";
        public const string PluginVersion = "1.0.1";

        internal static ManualLogSource Log;
        private static ConfigEntry<string> _excludedItems;
        private static HashSet<string> _excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Do wersji 1.0.0 identyfikator wtyczki zaczynal sie od "com.michal". Plik konfiguracji
        // nosi nazwe identyfikatora, wiec stary plik przenosimy pod nowa nazwe - gracz nie traci
        // ustawien. Wywolywane przed pierwszym Config.Bind.
        private const string LegacyPluginGUID = "com.michal.valheim.floatingitems";

        private void MigrateLegacyConfig()
        {
            string legacyPath = System.IO.Path.Combine(Paths.ConfigPath, LegacyPluginGUID + ".cfg");
            if (System.IO.File.Exists(Config.ConfigFilePath) || !System.IO.File.Exists(legacyPath))
                return;
            try
            {
                System.IO.File.Move(legacyPath, Config.ConfigFilePath);
                Config.Reload();
                Logger.LogInfo($"Przeniesiono ustawienia: {legacyPath} -> {Config.ConfigFilePath}");
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Nie udalo sie przeniesc ustawien ({legacyPath}): {e}");
            }
        }

        private void Awake()
        {
            Log = Logger;
            // Gra prosi mody o ustawienie tej flagi: w menu pojawia sie napis, ze gra jest
            // zmodowana (Iron Gate wymaga oznaczania modow jako nieoficjalnych).
            Game.isModded = true;
            MigrateLegacyConfig();
            _excludedItems = Config.Bind("General", "ExcludedItems", "",
                "Comma-separated prefab names of items that should keep sinking, e.g. \"Coins, IronScrap\". " +
                "Applies to items dropped after the change.");
            _excludedItems.SettingChanged += (_, __) => LoadExcluded();
            LoadExcluded();
            new Harmony(PluginGUID).PatchAll(typeof(Plugin).Assembly);
        }

        private static void LoadExcluded()
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in (_excludedItems.Value ?? "").Split(','))
            {
                var name = part.Trim();
                if (name.Length > 0)
                    names.Add(name);
            }
            _excluded = names;
        }

        // Przed Awake przedmiotu: gra zapamietuje w nim komponent Floating (m.in. do sprawdzania
        // smoly), wiec musi go juz zastac. Patch na egzemplarzach, nie na prefabach - obejmuje tez
        // przedmioty dodane przez inne mody, niezaleznie od tego, kiedy je zarejestrowaly.
        [HarmonyPatch(typeof(ItemDrop), "Awake")]
        private static class ItemDrop_Awake_Patch
        {
            private static void Prefix(ItemDrop __instance)
            {
                try
                {
                    var item = __instance.gameObject;
                    if (!_excluded.Contains(Utils.GetPrefabName(item)))
                        Buoyancy.TryAdd(item);
                }
                catch (Exception e)
                {
                    Log.LogError($"Nie udalo sie dodac plywania do '{__instance.name}': {e}");
                }
            }
        }
    }
}
