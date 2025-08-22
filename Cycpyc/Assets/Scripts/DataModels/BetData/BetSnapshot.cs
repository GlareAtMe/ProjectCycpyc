using System.Collections.Generic;


namespace Assets.Scripts.DataModels.BetData
{
    // Фіксація ставок на момент ConfirmBets
    public class BetSnapshot
    {
       public BetSnapshot(int roundIndex, Dictionary<string, List<BetPunishments>> betsByPlayerId)
        {
            RoundIndex = roundIndex;
            BetsByPlayerId = betsByPlayerId;
        }

        public int RoundIndex { get; private set; }
        public Dictionary<string, List<BetPunishments>> BetsByPlayerId { get; private set; }

    }
}
