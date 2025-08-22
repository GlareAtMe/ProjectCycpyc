using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.DataModels.BetData
{
    // Чиста “проекція” майбутніх змін без мутації стану
    public sealed class RoundComputationResult
    {
        public RoundComputationResult(int roundIndex, string winnerPlayerId, IReadOnlyList<PerPlayerResolution> players) { 
            RoundIndex = roundIndex;
            WinnerPlayerId = winnerPlayerId;
            Players = players ?? Array.Empty<PerPlayerResolution>();
        }

        public int RoundIndex { get; private set; }
        public string WinnerPlayerId { get; private set; }
        public IReadOnlyList<PerPlayerResolution> Players { get; private set; }
    }
}
