using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Ancients;

public class AncientDialogueSet
{
	[CompilerGenerated]
	private sealed class _003CGetAllDialogues_003Ed__12 : IEnumerable<AncientDialogue>, IEnumerable, IEnumerator<AncientDialogue>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private AncientDialogue _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public AncientDialogueSet _003C_003E4__this;

		private Dictionary<string, IReadOnlyList<AncientDialogue>>.ValueCollection.Enumerator _003C_003E7__wrap1;

		private IEnumerator<AncientDialogue> _003C_003E7__wrap2;

		AncientDialogue IEnumerator<AncientDialogue>.Current
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
		public _003CGetAllDialogues_003Ed__12(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			switch (num)
			{
			case -4:
			case -3:
			case 2:
				try
				{
					if (num == -4 || num == 2)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
				break;
			case -5:
			case 3:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally3();
				}
				break;
			}
			_003C_003E7__wrap1 = default(Dictionary<string, IReadOnlyList<AncientDialogue>>.ValueCollection.Enumerator);
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				AncientDialogueSet ancientDialogueSet = _003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					if (ancientDialogueSet.FirstVisitEverDialogue != null)
					{
						_003C_003E2__current = ancientDialogueSet.FirstVisitEverDialogue;
						_003C_003E1__state = 1;
						return true;
					}
					goto IL_005b;
				case 1:
					_003C_003E1__state = -1;
					goto IL_005b;
				case 2:
					_003C_003E1__state = -4;
					goto IL_00c8;
				case 3:
					{
						_003C_003E1__state = -5;
						break;
					}
					IL_005b:
					_003C_003E7__wrap1 = ancientDialogueSet.CharacterDialogues.Values.GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_00e2;
					IL_00e2:
					if (_003C_003E7__wrap1.MoveNext())
					{
						IReadOnlyList<AncientDialogue> current = _003C_003E7__wrap1.Current;
						_003C_003E7__wrap2 = current.GetEnumerator();
						_003C_003E1__state = -4;
						goto IL_00c8;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = default(Dictionary<string, IReadOnlyList<AncientDialogue>>.ValueCollection.Enumerator);
					_003C_003E7__wrap2 = ancientDialogueSet.AgnosticDialogues.GetEnumerator();
					_003C_003E1__state = -5;
					break;
					IL_00c8:
					if (_003C_003E7__wrap2.MoveNext())
					{
						AncientDialogue current2 = _003C_003E7__wrap2.Current;
						_003C_003E2__current = current2;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap2 = null;
					goto IL_00e2;
				}
				if (_003C_003E7__wrap2.MoveNext())
				{
					AncientDialogue current3 = _003C_003E7__wrap2.Current;
					_003C_003E2__current = current3;
					_003C_003E1__state = 3;
					return true;
				}
				_003C_003Em__Finally3();
				_003C_003E7__wrap2 = null;
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
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		private void _003C_003Em__Finally3()
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
		IEnumerator<AncientDialogue> IEnumerable<AncientDialogue>.GetEnumerator()
		{
			_003CGetAllDialogues_003Ed__12 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CGetAllDialogues_003Ed__12(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<AncientDialogue>)this).GetEnumerator();
		}
	}

	public required AncientDialogue? FirstVisitEverDialogue { get; init; }

	public required Dictionary<string, IReadOnlyList<AncientDialogue>> CharacterDialogues { get; init; }

	public required IReadOnlyList<AncientDialogue> AgnosticDialogues { get; init; } = Array.Empty<AncientDialogue>();


	[IteratorStateMachine(typeof(_003CGetAllDialogues_003Ed__12))]
	public IEnumerable<AncientDialogue> GetAllDialogues()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetAllDialogues_003Ed__12(-2)
		{
			_003C_003E4__this = this
		};
	}

	public IEnumerable<AncientDialogue> GetValidDialogues(ModelId characterId, int charVisits, int totalVisits, bool allowAnyCharacterDialogues)
	{
		if (totalVisits == 0 && FirstVisitEverDialogue != null)
		{
			return new global::_003C_003Ez__ReadOnlySingleElementList<AncientDialogue>(FirstVisitEverDialogue);
		}
		IReadOnlyList<AncientDialogue> readOnlyList = null;
		if (CharacterDialogues.TryGetValue(characterId.Entry, out IReadOnlyList<AncientDialogue> value))
		{
			readOnlyList = value;
			List<AncientDialogue> list = readOnlyList.Where((AncientDialogue d) => d.VisitIndex == charVisits).ToList();
			if (list.Count > 0)
			{
				return list;
			}
		}
		if (allowAnyCharacterDialogues)
		{
			List<AncientDialogue> list2 = AgnosticDialogues.Where((AncientDialogue d) => d.VisitIndex == charVisits).ToList();
			if (list2.Count > 0)
			{
				return list2;
			}
		}
		List<AncientDialogue> list3 = new List<AncientDialogue>();
		if (readOnlyList != null)
		{
			AddRepeatingDialogues(readOnlyList, list3, charVisits);
		}
		if (allowAnyCharacterDialogues)
		{
			AddRepeatingDialogues(AgnosticDialogues, list3, charVisits);
		}
		return list3;
	}

	public void PopulateLocKeys(string ancientEntry)
	{
		FirstVisitEverDialogue?.PopulateLines(ancientEntry, "firstVisitEver", 0);
		foreach (KeyValuePair<string, IReadOnlyList<AncientDialogue>> characterDialogue in CharacterDialogues)
		{
			characterDialogue.Deconstruct(out var key, out var value);
			string charEntry = key;
			IReadOnlyList<AncientDialogue> readOnlyList = value;
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				readOnlyList[i].PopulateLines(ancientEntry, charEntry, i);
			}
		}
		for (int j = 0; j < AgnosticDialogues.Count; j++)
		{
			AgnosticDialogues[j].PopulateLines(ancientEntry, "ANY", j);
		}
		foreach (AncientDialogue allDialogue in GetAllDialogues())
		{
			for (int k = 0; k < allDialogue.Lines.Count - 1; k++)
			{
				AncientDialogueLine ancientDialogueLine = allDialogue.Lines[k];
				string locEntryKey = ancientDialogueLine.LineText.LocEntryKey;
				string text = locEntryKey.Substring(0, locEntryKey.LastIndexOf('.'));
				string locEntryKey2 = text + ".next";
				ancientDialogueLine.NextButtonText = new LocString("ancients", locEntryKey2);
			}
		}
	}

	private static void AddRepeatingDialogues(IEnumerable<AncientDialogue> source, List<AncientDialogue> destination, int charVisits)
	{
		foreach (AncientDialogue item in source)
		{
			if (item.IsRepeating && (!item.VisitIndex.HasValue || !(charVisits < item.VisitIndex)))
			{
				destination.Add(item);
			}
		}
	}
}
