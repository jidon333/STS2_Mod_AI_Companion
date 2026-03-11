using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Saves;

public class EnemyStats
{
	[JsonPropertyName("enemy_id")]
	public required ModelId Id { get; init; }

	[JsonPropertyName("fight_stats")]
	public List<FightStats> FightStats { get; init; } = new List<FightStats>();


	[JsonIgnore]
	public int TotalWins
	{
		get
		{
			if (FightStats.Count == 0)
			{
				return 0;
			}
			return FightStats.Sum((FightStats f) => f.Wins);
		}
	}

	[JsonIgnore]
	public int TotalLosses
	{
		get
		{
			if (FightStats.Count == 0)
			{
				return 0;
			}
			return FightStats.Sum((FightStats f) => f.Losses);
		}
	}

	public void IncrementWin(ModelId characterId)
	{
		ModelId characterId2 = characterId;
		FightStats fightStats = FightStats.First((FightStats fight) => fight.Character == characterId2);
		fightStats.Wins++;
		Log.Info($"{characterId2} has killed a {Id}. That's {fightStats.Wins} kills");
	}

	public void IncrementLoss(ModelId characterId)
	{
		ModelId characterId2 = characterId;
		FightStats fightStats = FightStats.First((FightStats fight) => fight.Character == characterId2);
		fightStats.Losses++;
		Log.Info($"{characterId2} has died to a {Id}. That's {fightStats.Losses} losses");
	}
}
