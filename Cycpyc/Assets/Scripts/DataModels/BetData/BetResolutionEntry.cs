using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.DataModels.BetData
{
    public class BetResolutionEntry
    {

        public BetResolutionEntry(
            string playerId,
            bool isWinner,
            List<BetPunishments> appliedPunishments,
            List<BetPunishments> placedBets)
        {
            PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
            IsWinner = isWinner;
            AppliedPunishments = appliedPunishments;
            PlacedBets = placedBets;
        }

        public string PlayerId;
        public bool IsWinner;
        public List<BetPunishments> AppliedPunishments = new();
        public List<BetPunishments> PlacedBets = new();
    }
}
