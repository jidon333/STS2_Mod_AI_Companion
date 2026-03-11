using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Commands;

public static class OstyCmd
{
	public static async Task<SummonResult> Summon(PlayerChoiceContext choiceContext, Player summoner, decimal amount, AbstractModel? source)
	{
		Player summoner2 = summoner;
		CombatState combatState = summoner2.Creature.CombatState;
		amount = Hook.ModifySummonAmount(combatState, summoner2, amount, source);
		if (amount == 0m)
		{
			return new SummonResult(summoner2.Osty, 0m);
		}
		if (CombatManager.Instance.IsInProgress)
		{
			SfxCmd.Play("event:/sfx/characters/necrobinder/necrobinder_summon");
		}
		Creature osty = combatState.Allies.FirstOrDefault((Creature c) => c.Monster is Osty && c.PetOwner == summoner2);
		if (summoner2.IsOstyAlive)
		{
			await CreatureCmd.GainMaxHp(summoner2.Osty, amount);
		}
		else
		{
			bool isReviving = osty != null;
			if (isReviving)
			{
				if (osty.IsAlive)
				{
					throw new InvalidOperationException("We shouldn't make it here if Osty is still alive!");
				}
				summoner2.PlayerCombatState.AddPetInternal(osty);
			}
			else
			{
				osty = await PlayerCmd.AddPet<Osty>(summoner2);
				NCreature ostyNode = NCombatRoom.Instance?.GetCreatureNode(osty);
				if (ostyNode != null)
				{
					ostyNode.Modulate = Colors.Transparent;
					Tween tween = ostyNode.CreateTween();
					tween.TweenProperty(ostyNode, "modulate:a", 1, 0.3499999940395355).From(0);
					ostyNode.StartReviveAnim();
				}
				await PowerCmd.Apply<DieForYouPower>(osty, 1m, null, null);
				ostyNode?.TrackBlockStatus(summoner2.Creature);
			}
			await CreatureCmd.SetMaxHp(osty, amount);
			await CreatureCmd.Heal(osty, amount, isReviving);
			if (isReviving)
			{
				await Hook.AfterOstyRevived(combatState, osty);
			}
		}
		if (TestMode.IsOff)
		{
			NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(osty);
			nCreature.OstyScaleToSize(osty.MaxHp, 0.75f);
		}
		CombatManager.Instance.History.Summoned(combatState, (int)amount, summoner2);
		await Hook.AfterSummon(combatState, choiceContext, summoner2, amount);
		return new SummonResult(summoner2.Osty, amount);
	}
}
