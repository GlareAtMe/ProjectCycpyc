using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Assets.Scripts.DataModels.BetData
{
    public class RoundResult
    {
        public int RoundIndex;
        public DateTime UtcResolvedAt;
        public List<BetResolutionEntry> Entries = new();
    }
}
