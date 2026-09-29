using UnityEngine;

namespace FloatingItems
{
    // Dodaje przedmiotowi komponent Floating gry. Woda (WaterVolume) szuka na obiekcie z fizyka
    // komponentu IWaterInteractable i podaje mu poziom wody; Floating wypycha cialo do gory,
    // gdy srodek masy jest pod powierzchnia, i tlumi ruch - jak drewno w grze. Sila dziala tylko
    // u wlasciciela przedmiotu, tak samo jak w grze.
    internal static class Buoyancy
    {
        // Przedmiot, ktory plywa w grze od siebie - jego ustawienia sa wzorem dla reszty.
        private const string TemplateItem = "Wood";

        private static Floating _template;
        private static bool _templateMissingLogged;

        public static bool TryAdd(GameObject item)
        {
            // Juz obsluguje wode sam: przedmioty plywajace w grze (Floating) i ryby (Fish, plywaja
            // wlasna logika - wypychanie na powierzchnie by je zepsulo).
            if (item.GetComponent<IWaterInteractable>() != null)
                return false;
            // Floating korzysta z tych trzech bez sprawdzania; bez ciala (przedmiot zamieniony w
            // element budowli) nie ma czego unosic.
            if (item.GetComponent<Rigidbody>() == null || item.GetComponent<ZNetView>() == null ||
                item.GetComponentInChildren<Collider>() == null)
                return false;

            var floating = item.AddComponent<Floating>();
            var template = Template();
            if (template != null)
                CopySettings(template, floating);
            return true;
        }

        // Efekt powierzchni (m_surfaceEffects) zostaje pusty: to obiekt-dziecko prefabu wzorca,
        // wspolny dla wszystkich kopii - wlaczanie go z wielu przedmiotow przelaczaloby wzorzec.
        // Plusk przy wpadnieciu (m_impactEffects) to tylko lista prefabow do utworzenia, wiec moze
        // byc wspolna.
        private static void CopySettings(Floating from, Floating to)
        {
            to.m_waterLevelOffset = from.m_waterLevelOffset;
            to.m_forceDistance = from.m_forceDistance;
            to.m_force = from.m_force;
            to.m_balanceForceFraction = from.m_balanceForceFraction;
            to.m_damping = from.m_damping;
            to.m_impactEffects = from.m_impactEffects;
        }

        private static Floating Template()
        {
            if (_template != null)
                return _template;
            var db = ObjectDB.instance;
            if (db == null)
                return null;

            _template = FindTemplate(db);
            if (_template != null)
            {
                Plugin.Log.LogInfo($"Wzor plywania: {_template.name} (offset {_template.m_waterLevelOffset}, sila {_template.m_force}, " +
                                   $"zasieg {_template.m_forceDistance}, tlumienie {_template.m_damping}).");
            }
            else if (!_templateMissingLogged)
            {
                _templateMissingLogged = true;
                Plugin.Log.LogWarning("Zaden przedmiot gry nie ma komponentu Floating - uzywam jego domyslnych ustawien.");
            }
            return _template;
        }

        private static Floating FindTemplate(ObjectDB db)
        {
            var preferred = db.GetItemPrefab(TemplateItem);
            if (preferred != null && preferred.TryGetComponent(out Floating floating))
                return floating;
            foreach (var prefab in db.m_items)
                if (prefab != null && prefab.TryGetComponent(out floating))
                    return floating;
            return null;
        }
    }
}
