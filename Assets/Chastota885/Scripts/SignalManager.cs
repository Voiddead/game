using System.Collections.Generic;

namespace Chastota885
{
    public sealed class SignalManager
    {
        private readonly IDataProvider dataProvider;
        private readonly GameState state;

        public SignalManager(IDataProvider dataProvider, GameState state)
        {
            this.dataProvider = dataProvider;
            this.state = state;
        }

        public SignalData FindSignal(float tunedFrequency)
        {
            if (state.HasAnsweredToday || !string.IsNullOrEmpty(state.FinalEndingId))
                return null;

            foreach (SignalData signal in dataProvider.GetAllSignals())
            {
                if (signal == null || state.IsSignalCompleted(signal.id)) continue;
                if (signal.availability == null || !signal.availability.IsMet(state)) continue;
                if (System.Math.Abs(signal.frequency - tunedFrequency) <= signal.tolerance)
                    return signal;
            }
            return null;
        }

        public IReadOnlyList<SignalData> GetAvailableSignals()
        {
            List<SignalData> result = new List<SignalData>();
            if (state.HasAnsweredToday || !string.IsNullOrEmpty(state.FinalEndingId)) return result;

            foreach (SignalData signal in dataProvider.GetAllSignals())
                if (signal != null && !state.IsSignalCompleted(signal.id) &&
                    signal.availability != null && signal.availability.IsMet(state))
                    result.Add(signal);
            return result;
        }
    }
}
