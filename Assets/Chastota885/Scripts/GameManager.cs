using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chastota885
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; }
        public SignalManager Signals { get; private set; }

        public event Action<SignalData> SignalFound;
        public event Action<string> StatusChanged;
        public event Action<EndingCondition> GameEnded;
        public event Action StateChanged;

        private IDataProvider dataProvider;
        private EndingEvaluator endingEvaluator;
        private SignalData currentSignal;
        private bool initialized;

        public SignalData CurrentSignal => currentSignal;
        public IReadOnlyList<ResponseData> CurrentResponses =>
            currentSignal == null ? Array.Empty<ResponseData>() : currentSignal.responses;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            try
            {
                dataProvider = new JsonDataProvider();
                State = new GameState();
                Signals = new SignalManager(dataProvider, State);
                endingEvaluator = new EndingEvaluator(dataProvider);
                State.Changed += HandleStateChanged;
                initialized = true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                StatusChanged?.Invoke("Ошибка инициализации. Проверьте content.json и Console.");
            }
        }

        private void Start()
        {
            if (!initialized) return;
            if (!SaveSystem.TryLoad(State)) State.Reset();
            RestoreOrStart();
        }

        private void OnDestroy()
        {
            if (State != null) State.Changed -= HandleStateChanged;
            if (Instance == this) Instance = null;
        }

        private void HandleStateChanged() { StateChanged?.Invoke(); }

        public void StartNewGame()
        {
            if (!initialized) return;
            SaveSystem.DeleteSave();
            State.Reset();
            currentSignal = null;
            StatusChanged?.Invoke("День 1. Настройте радио и найдите сигнал.");
            SaveSystem.Save(State);
        }

        private void RestoreOrStart()
        {
            if (!string.IsNullOrEmpty(State.FinalEndingId))
            {
                EndingCondition ending = FindEnding(State.FinalEndingId);
                if (ending != null) GameEnded?.Invoke(ending);
                return;
            }

            if (!string.IsNullOrEmpty(State.CurrentSignalId))
            {
                SignalData savedSignal = dataProvider.GetSignalById(State.CurrentSignalId);
                if (savedSignal != null && !State.HasAnsweredToday)
                {
                    currentSignal = savedSignal;
                    SignalFound?.Invoke(currentSignal);
                    return;
                }
            }

            StatusChanged?.Invoke(State.HasAnsweredToday
                ? "На сегодня эфир исчерпан. Можно перейти к следующему дню."
                : "Радио готово. Настройте частоту.");
        }

        public void TuneFrequency(float frequency)
        {
            if (!initialized || State == null) return;
            if (State.HasAnsweredToday)
            {
                StatusChanged?.Invoke("Сегодня вы уже ответили на сигнал. Дождитесь следующего дня.");
                return;
            }
            if (!string.IsNullOrEmpty(State.FinalEndingId)) return;

            SignalData found = Signals.FindSignal(frequency);
            if (found == null)
            {
                StatusChanged?.Invoke("На этой частоте только шум. Попробуйте настроить радио точнее.");
                return;
            }

            currentSignal = found;
            State.SetCurrentSignal(found.id);
            SaveSystem.Save(State);
            SignalFound?.Invoke(found);
            StatusChanged?.Invoke("Сигнал пойман: " + found.title);
        }

        public void SelectResponse(string responseId)
        {
            if (currentSignal == null || State.HasAnsweredToday) return;

            ResponseData response = currentSignal.responses.Find(x => x.id == responseId);
            if (response == null)
            {
                StatusChanged?.Invoke("Ответ не найден.");
                return;
            }

            if (State.Battery < response.batteryCost)
            {
                StatusChanged?.Invoke("Недостаточно заряда батареи для ответа.");
                return;
            }

            // Стоимость ответа списывается отдельно от сюжетных эффектов.
            State.ApplyEffect(GameParameter.Battery, -response.batteryCost);

            if (response.effects != null)
                foreach (EffectData effect in response.effects)
                    if (effect != null) State.ApplyEffect(effect.parameter, effect.amount);

            if (response.flagsToAdd != null)
                foreach (string flag in response.flagsToAdd) State.AddFlag(flag);
            if (response.flagsToRemove != null)
                foreach (string flag in response.flagsToRemove) State.RemoveFlag(flag);

            State.CompleteSignal(currentSignal.id);
            currentSignal = null;

            EndingCondition ending = endingEvaluator.Evaluate(State);
            if (ending != null)
            {
                State.SetEnding(ending.id);
                SaveSystem.Save(State);
                GameEnded?.Invoke(ending);
                return;
            }

            SaveSystem.Save(State);
            StatusChanged?.Invoke("Ответ отправлен. День завершён.");
            if (response.advanceDay) AdvanceToNextDay();
        }

        public void AdvanceToNextDay()
        {
            if (!State.HasAnsweredToday)
            {
                StatusChanged?.Invoke("Сначала необходимо принять сигнал и ответить на него.");
                return;
            }

            State.AdvanceDay();
            SaveSystem.Save(State);
            StatusChanged?.Invoke("Наступил день " + State.CurrentDay + ". Настройте радио.");
        }

        public IReadOnlyList<SignalData> GetAvailableSignals()
        {
            return Signals == null ? Array.Empty<SignalData>() : Signals.GetAvailableSignals();
        }

        private EndingCondition FindEnding(string id)
        {
            foreach (EndingCondition ending in dataProvider.GetAllEndings())
                if (ending.id == id) return ending;
            return null;
        }
    }
}
