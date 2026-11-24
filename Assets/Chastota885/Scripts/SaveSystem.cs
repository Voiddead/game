using System;
using System.IO;
using UnityEngine;

namespace Chastota885
{
    public static class SaveSystem
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "chastota885_save.json");

        public static void Save(GameState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            string json = JsonUtility.ToJson(state.ToSaveData(), true);
            string temporaryPath = SavePath + ".tmp";
            File.WriteAllText(temporaryPath, json);

            if (File.Exists(SavePath)) File.Delete(SavePath);
            File.Move(temporaryPath, SavePath);
        }

        public static bool TryLoad(GameState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (!File.Exists(SavePath)) return false;

            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
                if (data == null) return false;
                state.LoadFrom(data);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError("Не удалось загрузить сохранение: " + exception.Message);
                return false;
            }
        }

        public static void DeleteSave()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }
    }
}
