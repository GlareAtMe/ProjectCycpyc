using System;
using System.Collections.Generic;


// Чиста “проекція” майбутніх змін без мутації стану
public sealed class PerPlayerResolution
{

    public  PerPlayerResolution(
      string playerId,
      bool isWinner,
      IReadOnlyList<BetPunishments> betsPlaced,
      IReadOnlyList<BetPunishments> punishmentsToApply)
    {
        PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
        IsWinner = isWinner;
        BetsPlaced = betsPlaced ?? Array.Empty<BetPunishments>();
        PunishmentsToApply = punishmentsToApply ?? Array.Empty<BetPunishments>();
    }

    public string PlayerId { get; private set; }
    public bool IsWinner { get; private set; }
    public IReadOnlyList<BetPunishments> BetsPlaced { get; private set; }
    public IReadOnlyList<BetPunishments> PunishmentsToApply { get; private set; } // для програвших
}
