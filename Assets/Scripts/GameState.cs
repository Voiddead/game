using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; }

    // События для UI
    public delegate void SignalLoadedEvent(SignalData signal, System.Collections.Generic.List<ResponseData> responses);
    public event SignalLoadedEvent OnSignalLoaded;

    public delegate void GameEndedEvent(string endingText);
    public event GameEndedEvent OnGameEnded;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        State = new GameState();
    }

    void Start()
    {
        LoadNextSignal(1);
    }

    public void LoadNextSignal(int signalId)
    {
        if (SignalManager.Instance.LoadSignal(signalId))
        {
            OnSignalLoaded?.Invoke(SignalManager.Instance.CurrentSignal, SignalManager.Instance.CurrentResponses);
        }
        else
        {
            EndGame("Ошибка: сигнал не найден");
        }
    }

    public void SelectResponse(ResponseData chosenResponse)
    {
        State.ApplyEffect("connection", chosenResponse.connectionDelta);
        State.ApplyEffect("paranoia", chosenResponse.paranoiaDelta);
        State.ApplyEffect("battery", chosenResponse.batteryDelta);

        if (!string.IsNullOrEmpty(chosenResponse.setFlag))
            State.AddFlag(chosenResponse.setFlag);

        if (!string.IsNullOrEmpty(chosenResponse.endingTitle) || chosenResponse.nextSignalId == -1)
        {
            EndGame($"Вы достигли концовки: {chosenResponse.endingTitle}");
            return;
        }

        LoadNextSignal(chosenResponse.nextSignalId);
    }

    private void EndGame(string text)
    {
        Debug.Log($"Игра окончена: {text}");
        OnGameEnded?.Invoke(text);
    }
}