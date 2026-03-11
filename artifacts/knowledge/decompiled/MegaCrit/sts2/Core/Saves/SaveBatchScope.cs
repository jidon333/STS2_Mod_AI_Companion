using System;
using System.Runtime.CompilerServices;

namespace MegaCrit.Sts2.Core.Saves;

public readonly struct SaveBatchScope : IDisposable
{
	[CompilerGenerated]
	private readonly SaveManager _003CsaveManager_003EP;

	public SaveBatchScope(SaveManager saveManager)
	{
		_003CsaveManager_003EP = saveManager;
	}

	public void Dispose()
	{
		_003CsaveManager_003EP.EndSaveBatch();
	}
}
