using System.Collections.Generic;
using System.Linq;

namespace Chastota885
{
    public sealed class EndingEvaluator
    {
        private readonly IDataProvider dataProvider;

        public EndingEvaluator(IDataProvider dataProvider) { this.dataProvider = dataProvider; }

        public EndingCondition Evaluate(GameState state)
        {
            // Наибольший priority проверяется первым. При равенстве порядок в JSON.
            IEnumerable<EndingCondition> ordered =
                dataProvider.GetAllEndings().OrderByDescending(x => x.priority);

            foreach (EndingCondition ending in ordered)
                if (ending != null && ending.IsMet(state))
                    return ending;

            return null;
        }
    }
}
