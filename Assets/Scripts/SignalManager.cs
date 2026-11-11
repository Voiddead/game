using UnityEngine;
using System.Collections.Generic;

public class SignalManager : MonoBehaviour
{
    public static SignalManager Instance { get; private set; }

    public SignalData CurrentSignal { get; private set; }
    public List<ResponseData> CurrentResponses { get; private set; }

    private IDataProvider dataProvider;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dataProvider = new MockDataProvider();
    }

    public bool LoadSignal(int signalId)
    {
        CurrentSignal = dataProvider.GetSignalById(signalId);

        if (CurrentSignal == null)
        {
            Debug.LogError($"Сигнал с ID {signalId} не найден!");
            return false;
        }

        CurrentResponses = dataProvider.GetResponsesForSignal(signalId);
        return true;
    }
}