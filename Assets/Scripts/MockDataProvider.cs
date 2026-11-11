using System.Collections.Generic;
using UnityEngine;

public class MockDataProvider : IDataProvider
{
    public SignalData GetSignalById(int id)
    {
        if (id == 1) return new SignalData { id = 1, title = "Первый контакт", text = "Помогите, мы заперты..." };
        if (id == 2) return new SignalData { id = 2, title = "Ответ", text = "Мы поняли вас. Ждите." };
        return null;
    }

    public List<ResponseData> GetResponsesForSignal(int signalId)
    {
        if (signalId == 1)
        {
            return new List<ResponseData>
            {
                new ResponseData { id = 101, signalId = 1, text = "Помочь", nextSignalId = 2, batteryDelta = -5, connectionDelta = 10, setFlag = "helped" },
                new ResponseData { id = 102, signalId = 1, text = "Промолчать", nextSignalId = -1, endingTitle = "Изоляция", batteryDelta = -2, paranoiaDelta = 15 }
            };
        }
        return new List<ResponseData>();
    }
}