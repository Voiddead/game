using System.Collections.Generic;

public interface IDataProvider
{
    SignalData GetSignalById(int id);
    List<ResponseData> GetResponsesForSignal(int signalId);
}