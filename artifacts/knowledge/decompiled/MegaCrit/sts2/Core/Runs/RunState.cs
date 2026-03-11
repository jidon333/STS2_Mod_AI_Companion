using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.Odds;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Runs;

public class RunState : IRunState, ICardScope, IPlayerCollection
{
	[CompilerGenerated]
	private sealed class _003CIterateHookListeners_003Ed__104 : IEnumerable<AbstractModel>, IEnumerable, IEnumerator<AbstractModel>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private AbstractModel _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public RunState _003C_003E4__this;

		private CombatState childCombatState;

		public CombatState _003C_003E3__childCombatState;

		private List<AbstractModel>.Enumerator _003C_003E7__wrap1;

		private IEnumerator<AbstractModel> _003C_003E7__wrap2;

		AbstractModel IEnumerator<AbstractModel>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CIterateHookListeners_003Ed__104(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			switch (_003C_003E1__state)
			{
			case -3:
			case 1:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
				break;
			case -4:
			case 2:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally2();
				}
				break;
			}
			_003C_003E7__wrap1 = default(List<AbstractModel>.Enumerator);
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				RunState runState = _003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
				{
					_003C_003E1__state = -1;
					List<AbstractModel> list = new List<AbstractModel>(runState.Players.Count * 50);
					foreach (Player player in runState.Players)
					{
						if (!player.IsActiveForHooks)
						{
							continue;
						}
						foreach (CardModel card in player.Deck.Cards)
						{
							list.Add(card);
							if (card.Enchantment != null)
							{
								list.Add(card.Enchantment);
							}
						}
					}
					if (childCombatState == null)
					{
						foreach (Player player2 in runState.Players)
						{
							if (player2.IsActiveForHooks)
							{
								list.AddRange(player2.Relics.Where((RelicModel r) => !r.IsMelted));
								list.AddRange(player2.Potions);
							}
						}
						list.AddRange(runState.Modifiers);
						list.Add(runState.MultiplayerScalingModel);
					}
					_003C_003E7__wrap1 = list.GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_01b0;
				}
				case 1:
					_003C_003E1__state = -3;
					goto IL_01b0;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_021f;
					}
					IL_01b0:
					while (_003C_003E7__wrap1.MoveNext())
					{
						AbstractModel current4 = _003C_003E7__wrap1.Current;
						if (Contains(current4))
						{
							_003C_003E2__current = current4;
							_003C_003E1__state = 1;
							return true;
						}
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = default(List<AbstractModel>.Enumerator);
					if (childCombatState == null)
					{
						break;
					}
					_003C_003E7__wrap2 = childCombatState.IterateHookListeners().GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_021f;
					IL_021f:
					if (_003C_003E7__wrap2.MoveNext())
					{
						AbstractModel current5 = _003C_003E7__wrap2.Current;
						_003C_003E2__current = current5;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap2 = null;
					break;
				}
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003E7__wrap1).Dispose();
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<AbstractModel> IEnumerable<AbstractModel>.GetEnumerator()
		{
			_003CIterateHookListeners_003Ed__104 _003CIterateHookListeners_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CIterateHookListeners_003Ed__ = this;
			}
			else
			{
				_003CIterateHookListeners_003Ed__ = new _003CIterateHookListeners_003Ed__104(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			_003CIterateHookListeners_003Ed__.childCombatState = _003C_003E3__childCombatState;
			return _003CIterateHookListeners_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<AbstractModel>)this).GetEnumerator();
		}
	}

	private readonly List<Player> _players = new List<Player>();

	private int _currentActIndex;

	private readonly List<MapCoord> _visitedMapCoords = new List<MapCoord>();

	private readonly List<List<MapPointHistoryEntry>> _mapPointHistory = new List<List<MapPointHistoryEntry>>();

	private readonly List<AbstractRoom> _currentRooms = new List<AbstractRoom>();

	private readonly HashSet<ModelId> _visitedEventIds = new HashSet<ModelId>();

	private readonly List<CardModel> _allCards = new List<CardModel>();

	public IReadOnlyList<Player> Players => _players;

	public IReadOnlyList<ActModel> Acts { get; private set; }

	public int CurrentActIndex
	{
		get
		{
			return _currentActIndex;
		}
		set
		{
			if (_currentActIndex != value)
			{
				_visitedMapCoords.Clear();
				ActFloor = 0;
				_currentActIndex = value;
			}
		}
	}

	public ActModel Act => Acts[CurrentActIndex];

	public ActMap Map { get; set; } = NullActMap.Instance;


	public IReadOnlyList<MapCoord> VisitedMapCoords => _visitedMapCoords;

	public MapCoord? CurrentMapCoord
	{
		get
		{
			if (_visitedMapCoords.Count != 0)
			{
				return _visitedMapCoords.Last();
			}
			return null;
		}
	}

	public MapPoint? CurrentMapPoint
	{
		get
		{
			if (!CurrentMapCoord.HasValue)
			{
				return null;
			}
			return Map.GetPoint(CurrentMapCoord.Value);
		}
	}

	public RunLocation CurrentLocation => new RunLocation(CurrentMapCoord, CurrentActIndex);

	public int ActFloor { get; set; }

	public int TotalFloor => MapPointHistory.Sum((IReadOnlyList<MapPointHistoryEntry> c) => c.Count);

	public IReadOnlyList<IReadOnlyList<MapPointHistoryEntry>> MapPointHistory => _mapPointHistory;

	public MapPointHistoryEntry? CurrentMapPointHistoryEntry => MapPointHistory.LastOrDefault()?.LastOrDefault();

	public int CurrentRoomCount => _currentRooms.Count;

	public AbstractRoom? CurrentRoom => _currentRooms.LastOrDefault();

	public AbstractRoom? BaseRoom => _currentRooms.FirstOrDefault();

	public bool IsGameOver
	{
		get
		{
			if (Players.Count > 0)
			{
				return Players.All((Player p) => p.Creature.IsDead);
			}
			return false;
		}
	}

	public int AscensionLevel { get; init; }

	public RunRngSet Rng { get; init; }

	public RunOddsSet Odds { get; init; }

	public RelicGrabBag SharedRelicGrabBag { get; init; }

	public UnlockState UnlockState { get; init; }

	public IReadOnlySet<ModelId> VisitedEventIds => _visitedEventIds;

	public IReadOnlyList<ModifierModel> Modifiers { get; private set; }

	public ExtraRunFields ExtraFields { get; private set; } = new ExtraRunFields();


	public MultiplayerScalingModel MultiplayerScalingModel { get; private set; }

	public static RunState CreateForNewRun(IReadOnlyList<Player> players, IReadOnlyList<ActModel> acts, IReadOnlyList<ModifierModel> modifiers, int ascensionLevel, string seed)
	{
		RunRngSet runRngSet = new RunRngSet(seed);
		RunOddsSet odds = new RunOddsSet(runRngSet.UnknownMapPoint);
		RunState result = CreateShared(players, acts, modifiers, 0, runRngSet, odds, new RelicGrabBag(refreshAllowed: true), ascensionLevel);
		foreach (Player player in players)
		{
			player.InitializeSeed(seed);
			foreach (CardModel card in player.Deck.Cards)
			{
				card.AfterCreated();
			}
		}
		return result;
	}

	public static RunState FromSerializable(SerializableRun save)
	{
		List<SerializablePlayer> players = save.Players;
		List<Player> players2 = players.Select(Player.FromSerializable).ToList();
		RunRngSet runRngSet = RunRngSet.FromSave(save.SerializableRng);
		RunState runState = CreateShared(players2, save.Acts.Select(ActModel.FromSave).ToList(), save.Modifiers.Select(ModifierModel.FromSerializable).ToList(), save.CurrentActIndex, runRngSet, RunOddsSet.FromSerializable(save.SerializableOdds, runRngSet.UnknownMapPoint), RelicGrabBag.FromSerializable(save.SerializableSharedRelicGrabBag), save.Ascension);
		runState._visitedMapCoords.AddRange(save.VisitedMapCoords);
		runState._visitedEventIds.UnionWith(save.EventsSeen);
		runState._mapPointHistory.AddRange(new global::_003C_003Ez__ReadOnlyArray<List<MapPointHistoryEntry>>(save.MapPointHistory.ToArray()));
		runState.ExtraFields = ExtraRunFields.FromSerializable(save.ExtraFields);
		return runState;
	}

	public static RunState CreateForTest(IReadOnlyList<Player>? players = null, IReadOnlyList<ActModel>? acts = null, IReadOnlyList<ModifierModel>? modifiers = null, int ascensionLevel = 0, string? seed = null)
	{
		if (seed == null)
		{
			seed = SeedHelper.GetRandomSeed();
		}
		RunRngSet runRngSet = new RunRngSet(seed);
		RunState runState = CreateShared(players ?? new global::_003C_003Ez__ReadOnlySingleElementList<Player>(Player.CreateForNewRun<Deprived>(MegaCrit.Sts2.Core.Unlocks.UnlockState.all, 1uL)), (acts ?? ActModel.GetDefaultList()).Select((ActModel a) => a.ToMutable()).ToList(), modifiers ?? Array.Empty<ModifierModel>(), 0, runRngSet, new RunOddsSet(runRngSet.UnknownMapPoint), new RelicGrabBag(refreshAllowed: true), ascensionLevel);
		foreach (Player player in runState.Players)
		{
			player.InitializeSeed(seed);
		}
		return runState;
	}

	private static RunState CreateShared(IReadOnlyList<Player> players, IReadOnlyList<ActModel> acts, IReadOnlyList<ModifierModel> modifiers, int currentActIndex, RunRngSet rng, RunOddsSet odds, RelicGrabBag sharedRelicGrabBag, int ascensionLevel)
	{
		RunState runState = new RunState(players, acts, modifiers, currentActIndex, rng, odds, sharedRelicGrabBag, ascensionLevel);
		foreach (Player player in players)
		{
			player.RunState = runState;
			foreach (CardModel card in player.Deck.Cards)
			{
				runState.AddCard(card, player);
			}
		}
		runState.MultiplayerScalingModel = (MultiplayerScalingModel)ModelDb.Singleton<MultiplayerScalingModel>().MutableClone();
		runState.MultiplayerScalingModel.Initialize(runState);
		return runState;
	}

	private RunState(IReadOnlyList<Player> players, IReadOnlyList<ActModel> acts, IReadOnlyList<ModifierModel> modifiers, int currentActIndex, RunRngSet rng, RunOddsSet odds, RelicGrabBag sharedRelicGrabBag, int ascensionLevel)
	{
		foreach (ActModel act in acts)
		{
			act.AssertMutable();
		}
		_players.AddRange(players);
		Acts = acts;
		Modifiers = modifiers;
		CurrentActIndex = currentActIndex;
		Rng = rng;
		Odds = odds;
		SharedRelicGrabBag = sharedRelicGrabBag;
		UnlockState = new UnlockState(players.Select((Player p) => p.UnlockState));
		AscensionLevel = ascensionLevel;
	}

	public int GetPlayerSlotIndex(Player player)
	{
		return Players.IndexOf(player);
	}

	public int GetPlayerSlotIndex(ulong netId)
	{
		return Players.FirstIndex((Player p) => p.NetId == netId);
	}

	public Player? GetPlayer(ulong netId)
	{
		return Players.FirstOrDefault((Player p) => p.NetId == netId);
	}

	public T CreateCard<T>(Player owner) where T : CardModel
	{
		return (T)CreateCard(ModelDb.Card<T>(), owner);
	}

	public CardModel CreateCard(CardModel canonicalCard, Player owner)
	{
		CardModel cardModel = canonicalCard.ToMutable();
		AddCard(cardModel, owner);
		cardModel.AfterCreated();
		return cardModel;
	}

	public CardModel CloneCard(CardModel mutableCard)
	{
		CardModel cardModel = (CardModel)mutableCard.ClonePreservingMutability();
		AddCard(cardModel);
		return cardModel;
	}

	public void AddCard(CardModel card, Player owner)
	{
		if (!card.HasBeenRemovedFromState)
		{
			card.Owner = owner;
		}
		AddCard(card);
	}

	public void RemoveCard(CardModel card)
	{
		_allCards.Remove(card);
		card.Owner = null;
	}

	public bool ContainsCard(CardModel card)
	{
		return _allCards.Contains(card);
	}

	public CardModel LoadCard(SerializableCard serializableCard, Player owner)
	{
		CardModel cardModel = CardModel.FromSerializable(serializableCard);
		AddCard(cardModel, owner);
		return cardModel;
	}

	private void AddCard(CardModel card)
	{
		card.AssertMutable();
		if (card.HasBeenRemovedFromState)
		{
			if (!ContainsCard(card))
			{
				throw new InvalidOperationException($"Tried to add card {card} to RunState that has HasBeenRemovedFromState set as true, but it does not belong to this state!");
			}
			card.HasBeenRemovedFromState = false;
		}
		else
		{
			_allCards.Add(card);
		}
	}

	public bool AddVisitedMapCoord(MapCoord coord)
	{
		if (_visitedMapCoords.Contains(coord))
		{
			return false;
		}
		_visitedMapCoords.Add(coord);
		return true;
	}

	public AbstractRoom PopCurrentRoom()
	{
		if (_currentRooms.Count == 0)
		{
			throw new InvalidOperationException("Not in any rooms.");
		}
		AbstractRoom result = _currentRooms.Last();
		_currentRooms.RemoveAt(_currentRooms.Count - 1);
		return result;
	}

	public void PushRoom(AbstractRoom room)
	{
		if (_currentRooms.Contains(room))
		{
			throw new InvalidOperationException("Already in this room.");
		}
		_currentRooms.Add(room);
	}

	public void AddVisitedEvent(EventModel eventModel)
	{
		_visitedEventIds.Add(eventModel.Id);
	}

	public void AppendToMapPointHistory(MapPointType mapPointType, RoomType initialRoomType, ModelId? roomModelId)
	{
		if (_mapPointHistory.Count <= CurrentActIndex)
		{
			int num = CurrentActIndex + 1 - _mapPointHistory.Count;
			for (int i = 0; i < num; i++)
			{
				_mapPointHistory.Add(new List<MapPointHistoryEntry>());
			}
		}
		MapPointHistoryEntry mapPointHistoryEntry = new MapPointHistoryEntry(mapPointType, this);
		mapPointHistoryEntry.Rooms.Add(new MapPointRoomHistoryEntry
		{
			RoomType = initialRoomType,
			ModelId = roomModelId
		});
		_mapPointHistory[CurrentActIndex].Add(mapPointHistoryEntry);
	}

	public MapPointHistoryEntry? GetHistoryEntryFor(RunLocation location)
	{
		if (location.actIndex >= _mapPointHistory.Count || !location.coord.HasValue || location.coord?.row >= _mapPointHistory[location.actIndex].Count)
		{
			return null;
		}
		return _mapPointHistory[location.actIndex][location.coord.Value.row];
	}

	[IteratorStateMachine(typeof(_003CIterateHookListeners_003Ed__104))]
	public IEnumerable<AbstractModel> IterateHookListeners(CombatState? childCombatState)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CIterateHookListeners_003Ed__104(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__childCombatState = childCombatState
		};
	}

	public void AddPlayerDebug(Player player, int index)
	{
		if (index >= 0)
		{
			_players.Insert(index, player);
		}
		else
		{
			_players.Add(player);
		}
		player.InitializeSeed(Rng.StringSeed);
		foreach (CardModel card in player.Deck.Cards)
		{
			card.AfterCreated();
		}
		player.RunState = this;
		foreach (CardModel card2 in player.Deck.Cards)
		{
			AddCard(card2, player);
		}
		CurrentMapPointHistoryEntry.PlayerStats.Add(new PlayerMapPointHistoryEntry
		{
			PlayerId = player.NetId
		});
		if (RunManager.Instance.IsInProgress)
		{
			player.PopulateRelicGrabBagIfNecessary(Rng.UpFront);
			RunManager.Instance.ApplyAscensionEffects(player);
		}
	}

	public void SetActDebug(ActModel act)
	{
		act.AssertMutable();
		List<ActModel> list = Acts.ToList();
		list[CurrentActIndex] = act;
		Acts = list;
	}

	public void ClearVisitedMapCoordsDebug()
	{
		_visitedMapCoords.Clear();
		ActFloor = 0;
	}

	public void AddModifierDebug(ModifierModel modifier)
	{
		IReadOnlyList<ModifierModel> modifiers = Modifiers;
		int num = 0;
		ModifierModel[] array = new ModifierModel[1 + modifiers.Count];
		foreach (ModifierModel item in modifiers)
		{
			array[num] = item;
			num++;
		}
		array[num] = modifier;
		Modifiers = new global::_003C_003Ez__ReadOnlyArray<ModifierModel>(array);
	}

	private static bool Contains(AbstractModel model)
	{
		if (!(model is RelicModel relicModel))
		{
			if (!(model is PotionModel potionModel))
			{
				if (!(model is CardModel cardModel))
				{
					if (!(model is EnchantmentModel enchantmentModel))
					{
						if (!(model is AchievementModel))
						{
							if (!(model is ModifierModel))
							{
								if (model is MultiplayerScalingModel)
								{
									return true;
								}
								throw new ArgumentOutOfRangeException("model", model, $"Invalid model type {model.GetType()} ({model})");
							}
							return true;
						}
						return true;
					}
					return enchantmentModel.HasCard && !enchantmentModel.Card.HasBeenRemovedFromState && enchantmentModel.Card.Owner.IsActiveForHooks;
				}
				return !cardModel.HasBeenRemovedFromState && cardModel.Owner.IsActiveForHooks;
			}
			return !potionModel.HasBeenRemovedFromState && potionModel.Owner.IsActiveForHooks;
		}
		return !relicModel.HasBeenRemovedFromState && relicModel.Owner.IsActiveForHooks;
	}
}
