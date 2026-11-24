using System.Collections.Generic;

namespace Chastota885
{
    public interface IDataProvider
    {
        IReadOnlyList<SignalData> GetAllSignals();
        IReadOnlyList<EndingCondition> GetAllEndings();
        SignalData GetSignalById(string id);
    }
}
