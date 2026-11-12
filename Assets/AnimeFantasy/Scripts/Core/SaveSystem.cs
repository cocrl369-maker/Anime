using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnimeFantasy
{
    [Serializable]
    public class HeroProgress
    {
        public string id;
        public int level = 1;
        public int xp;
    }

    [Serializable]
    public class SaveData
    {
        /// <summary>Index des nächsten (noch nicht geschafften) Kapitels.</summary>
        public int stage;
        public List<HeroProgress> heroes = new List<HeroProgress>();

        public HeroProgress GetHero(string id)
        {
            var h = heroes.Find(x => x.id == id);
            if (h == null)
            {
                h = new HeroProgress { id = id };
                heroes.Add(h);
            }
            return h;
        }
    }

    /// <summary>Speichert den Fortschritt als JSON in den PlayerPrefs (funktioniert auf Android ohne Rechte).</summary>
    public static class SaveSystem
    {
        const string Key = "AnimeFantasy.Save.v1";

        public static SaveData Load()
        {
            try
            {
                var json = PlayerPrefs.GetString(Key, "");
                if (!string.IsNullOrEmpty(json))
                {
                    var data = JsonUtility.FromJson<SaveData>(json);
                    if (data != null) return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Spielstand konnte nicht geladen werden: " + e.Message);
            }
            return new SaveData();
        }

        public static void Save(SaveData data)
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public static SaveData Reset()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
            return new SaveData();
        }
    }

    public static class Progression
    {
        public static int XpToNext(int level) => 40 + (level - 1) * 30;

        public static List<string> AddXp(SaveData save, int xp)
        {
            var messages = new List<string>();
            foreach (var id in GameDatabase.HeroIds)
            {
                var h = save.GetHero(id);
                h.xp += xp;
                while (h.xp >= XpToNext(h.level))
                {
                    h.xp -= XpToNext(h.level);
                    h.level++;
                    messages.Add(GameDatabase.GetCharacter(id).Name + " erreicht Level " + h.level + "!");
                }
            }
            return messages;
        }
    }
}
