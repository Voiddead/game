using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Chastota885
{
    // Сейчас игра читает контент из StreamingAssets/content.json.
    // Позже можно добавить SqliteDataProvider : IDataProvider, не меняя GameManager.
    public sealed class JsonDataProvider : IDataProvider
    {
        private readonly GameContent content;
        private readonly Dictionary<string, SignalData> signalsById =
            new Dictionary<string, SignalData>(StringComparer.Ordinal);

        public JsonDataProvider()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "content.json");
            if (!File.Exists(path))
                throw new FileNotFoundException("Не найден файл игрового контента: " + path);

            string json = File.ReadAllText(path);
            content = JsonUtility.FromJson<GameContent>(json);
            if (content == null) throw new InvalidDataException("Не удалось прочитать content.json.");

            if (content.signals == null) content.signals = new List<SignalData>();
            if (content.endings == null) content.endings = new List<EndingCondition>();

            foreach (SignalData signal in content.signals)
            {
                if (signal == null || string.IsNullOrWhiteSpace(signal.id))
                    throw new InvalidDataException("У каждого сигнала должен быть непустой id.");
                if (signalsById.ContainsKey(signal.id))
                    throw new InvalidDataException("Повторяется id сигнала: " + signal.id);
                if (signal.responses == null) signal.responses = new List<ResponseData>();
                if (signal.availability == null) signal.availability = new SignalCondition();
                signalsById.Add(signal.id, signal);
            }

            foreach (EndingCondition ending in content.endings)
                if (ending == null || string.IsNullOrWhiteSpace(ending.id))
                    throw new InvalidDataException("У каждой концовки должен быть непустой id.");
        }

        public IReadOnlyList<SignalData> GetAllSignals() { return content.signals; }
        public IReadOnlyList<EndingCondition> GetAllEndings() { return content.endings; }

        public SignalData GetSignalById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            signalsById.TryGetValue(id, out SignalData signal);
            return signal;
        }
    }
}
