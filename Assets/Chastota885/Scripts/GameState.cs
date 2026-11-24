using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chastota885
{
    public sealed class GameState
    {
        public const int MinValue = 0;
        public const int MaxValue = 100;

        public int CurrentDay { get; private set; }
        public int Connection { get; private set; }
        public int Paranoia { get; private set; }
        public int Battery { get; private set; }
        public bool HasAnsweredToday { get; private set; }
        public string CurrentSignalId { get; private set; }
        public string FinalEndingId { get; private set; }

        private readonly HashSet<string> activeFlags = new HashSet<string>();
        private readonly HashSet<string> completedSignals = new HashSet<string>();

        public event Action Changed;

        public GameState()
        {
            Reset();
        }

        public void Reset()
        {
            CurrentDay = 1;
            Connection = 50;
            Paranoia = 50;
            Battery = 100;
            HasAnsweredToday = false;
            CurrentSignalId = null;
            FinalEndingId = null;
            activeFlags.Clear();
            completedSignals.Clear();
            NotifyChanged();
        }

        public int GetValue(GameParameter parameter)
        {
            switch (parameter)
            {
                case GameParameter.Connection: return Connection;
                case GameParameter.Paranoia: return Paranoia;
                case GameParameter.Battery: return Battery;
                default: throw new ArgumentOutOfRangeException(nameof(parameter));
            }
        }

        public void ApplyEffect(GameParameter parameter, int amount)
        {
            int value = Mathf.Clamp(GetValue(parameter) + amount, MinValue, MaxValue);
            SetValue(parameter, value);
        }

        public void SetValue(GameParameter parameter, int value)
        {
            value = Mathf.Clamp(value, MinValue, MaxValue);
            switch (parameter)
            {
                case GameParameter.Connection: Connection = value; break;
                case GameParameter.Paranoia: Paranoia = value; break;
                case GameParameter.Battery: Battery = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(parameter));
            }
            NotifyChanged();
        }

        public bool HasFlag(string flag)
        {
            return !string.IsNullOrWhiteSpace(flag) && activeFlags.Contains(flag);
        }

        public void AddFlag(string flag)
        {
            if (!string.IsNullOrWhiteSpace(flag) && activeFlags.Add(flag)) NotifyChanged();
        }

        public void RemoveFlag(string flag)
        {
            if (!string.IsNullOrWhiteSpace(flag) && activeFlags.Remove(flag)) NotifyChanged();
        }

        public bool IsSignalCompleted(string signalId)
        {
            return !string.IsNullOrWhiteSpace(signalId) && completedSignals.Contains(signalId);
        }

        public void SetCurrentSignal(string signalId)
        {
            CurrentSignalId = signalId;
            NotifyChanged();
        }

        public void CompleteSignal(string signalId)
        {
            if (!string.IsNullOrWhiteSpace(signalId)) completedSignals.Add(signalId);
            CurrentSignalId = null;
            HasAnsweredToday = true;
            NotifyChanged();
        }

        public void AdvanceDay()
        {
            CurrentDay++;
            HasAnsweredToday = false;
            CurrentSignalId = null;
            NotifyChanged();
        }

        public void SetEnding(string endingId)
        {
            FinalEndingId = endingId;
            CurrentSignalId = null;
            NotifyChanged();
        }

        public SaveData ToSaveData()
        {
            SaveData data = new SaveData
            {
                currentDay = CurrentDay,
                connection = Connection,
                paranoia = Paranoia,
                battery = Battery,
                hasAnsweredToday = HasAnsweredToday,
                currentSignalId = CurrentSignalId,
                finalEndingId = FinalEndingId,
                activeFlags = new List<string>(activeFlags),
                completedSignalIds = new List<string>(completedSignals)
            };
            return data;
        }

        public void LoadFrom(SaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            CurrentDay = Mathf.Max(1, data.currentDay);
            Connection = Mathf.Clamp(data.connection, MinValue, MaxValue);
            Paranoia = Mathf.Clamp(data.paranoia, MinValue, MaxValue);
            Battery = Mathf.Clamp(data.battery, MinValue, MaxValue);
            HasAnsweredToday = data.hasAnsweredToday;
            CurrentSignalId = data.currentSignalId;
            FinalEndingId = data.finalEndingId;
            activeFlags.Clear();
            completedSignals.Clear();
            if (data.activeFlags != null)
                foreach (string flag in data.activeFlags)
                    if (!string.IsNullOrWhiteSpace(flag)) activeFlags.Add(flag);
            if (data.completedSignalIds != null)
                foreach (string id in data.completedSignalIds)
                    if (!string.IsNullOrWhiteSpace(id)) completedSignals.Add(id);
            NotifyChanged();
        }

        private void NotifyChanged() { Changed?.Invoke(); }
    }
}
