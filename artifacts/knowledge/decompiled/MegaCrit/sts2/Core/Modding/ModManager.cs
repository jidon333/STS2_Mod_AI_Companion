using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Platform.Steam;
using MegaCrit.Sts2.Core.Saves;
using Steamworks;

namespace MegaCrit.Sts2.Core.Modding;

public static class ModManager
{
	public delegate void MetricsUploadHook(SerializableRun run, bool isVictory, ulong localPlayerId);

	[CompilerGenerated]
	private sealed class _003CGetModdedLocTables_003Ed__24 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string language;

		public string _003C_003E3__language;

		private string file;

		public string _003C_003E3__file;

		private List<Mod>.Enumerator _003C_003E7__wrap1;

		string IEnumerator<string>.Current
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
		public _003CGetModdedLocTables_003Ed__24(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = System.Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = default(List<Mod>.Enumerator);
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = _mods.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap1.MoveNext())
				{
					Mod current = _003C_003E7__wrap1.Current;
					if (current.wasLoaded)
					{
						string path = $"res://{current.manifest.pckName}/localization/{language}/{file}";
						if (ResourceLoader.Exists(path))
						{
							_003C_003E2__current = path;
							_003C_003E1__state = 1;
							return true;
						}
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = default(List<Mod>.Enumerator);
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

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003CGetModdedLocTables_003Ed__24 _003CGetModdedLocTables_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == System.Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetModdedLocTables_003Ed__ = this;
			}
			else
			{
				_003CGetModdedLocTables_003Ed__ = new _003CGetModdedLocTables_003Ed__24(0);
			}
			_003CGetModdedLocTables_003Ed__.language = _003C_003E3__language;
			_003CGetModdedLocTables_003Ed__.file = _003C_003E3__file;
			return _003CGetModdedLocTables_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	private static List<Mod> _mods = new List<Mod>();

	private static List<Mod> _loadedMods = new List<Mod>();

	private static bool _initialized;

	private static Callback<ItemInstalled_t>? _steamItemInstalledCallback;

	public static IReadOnlyList<Mod> AllMods => _mods;

	public static IReadOnlyList<Mod> LoadedMods => _loadedMods;

	public static bool PlayerAgreedToModLoading => SaveManager.Instance.SettingsSave.ModSettings?.PlayerAgreedToModLoading ?? false;

	public static event Action<Mod>? OnModDetected;

	public static event MetricsUploadHook? OnMetricsUpload;

	public static void Initialize()
	{
		if (CommandLineHelper.HasArg("nomods"))
		{
			Log.Info("'nomods' passed as executable argument, skipping mod initialization");
			return;
		}
		AppDomain.CurrentDomain.AssemblyResolve += HandleAssemblyResolveFailure;
		string executablePath = OS.GetExecutablePath();
		string directoryName = Path.GetDirectoryName(executablePath);
		using DirAccess dirAccess = DirAccess.Open(Path.Combine(directoryName, "mods"));
		if (dirAccess != null)
		{
			LoadModsInDirRecursive(dirAccess, ModSource.ModsDirectory);
		}
		if (SteamInitializer.Initialized)
		{
			InitializeSteamMods();
		}
		_loadedMods = _mods.Where((Mod m) => m.wasLoaded).ToList();
		if (_loadedMods.Count > 0)
		{
			Log.Info($" --- RUNNING MODDED! --- Loaded {_loadedMods.Count} mods ({_mods.Count} total)");
		}
		_initialized = true;
	}

	private static void LoadModsInDirRecursive(DirAccess dirAccess, ModSource source)
	{
		string[] files = dirAccess.GetFiles();
		foreach (string text in files)
		{
			if (text.EndsWith(".pck"))
			{
				Log.Info("Found mod pck file " + dirAccess.GetCurrentDir() + "/" + text);
				TryLoadModFromPck(text, dirAccess, source);
			}
		}
		string[] directories = dirAccess.GetDirectories();
		foreach (string path in directories)
		{
			using DirAccess dirAccess2 = DirAccess.Open(Path.Combine(dirAccess.GetCurrentDir(), path));
			if (dirAccess2 != null)
			{
				LoadModsInDirRecursive(dirAccess2, source);
			}
		}
	}

	private static void InitializeSteamMods()
	{
		uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
		PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
		numSubscribedItems = SteamUGC.GetSubscribedItems(array, numSubscribedItems);
		for (int i = 0; i < numSubscribedItems; i++)
		{
			PublishedFileId_t workshopItemId = array[i];
			TryLoadModFromSteam(workshopItemId);
		}
		_steamItemInstalledCallback = Callback<ItemInstalled_t>.Create(OnSteamWorkshopItemInstalled);
	}

	private static void TryLoadModFromSteam(PublishedFileId_t workshopItemId)
	{
		if (!SteamUGC.GetItemInstallInfo(workshopItemId, out var punSizeOnDisk, out var pchFolder, 256u, out var punTimeStamp))
		{
			Log.Warn($"Could not get Steam Workshop item install info for item {workshopItemId.m_PublishedFileId}");
			return;
		}
		Log.Info($"Looking for mods to load from Steam Workshop mod {workshopItemId.m_PublishedFileId} in {pchFolder} (size {punSizeOnDisk}, last modified {punTimeStamp})");
		using DirAccess dirAccess = DirAccess.Open(pchFolder);
		if (dirAccess == null)
		{
			Log.Warn("Could not open Steam Workshop folder: " + pchFolder);
		}
		else
		{
			LoadModsInDirRecursive(dirAccess, ModSource.SteamWorkshop);
		}
	}

	private static void OnSteamWorkshopItemInstalled(ItemInstalled_t ev)
	{
		if ((ulong)ev.m_unAppID.m_AppId == 2868840)
		{
			Log.Info($"Detected new Steam Workshop item installation, id: {ev.m_nPublishedFileId.m_PublishedFileId}");
			TryLoadModFromSteam(ev.m_nPublishedFileId);
		}
	}

	private static void TryLoadModFromPck(string pckFilename, DirAccess dirAccess, ModSource source)
	{
		Assembly assembly = null;
		string pckName = Path.GetFileNameWithoutExtension(pckFilename);
		bool flag = SaveManager.Instance.SettingsSave.ModSettings?.IsModDisabled(pckName, source) ?? false;
		bool flag2 = _mods.Any((Mod m) => m.manifest?.pckName == pckName);
		if (!PlayerAgreedToModLoading || flag || flag2 || _initialized)
		{
			if (_initialized)
			{
				Log.Info("Skipping loading mod " + pckFilename + ", can't load mods at runtime");
			}
			else if (flag)
			{
				Log.Info("Skipping loading mod " + pckFilename + ", it is set to disabled in settings");
			}
			else if (!PlayerAgreedToModLoading)
			{
				Log.Info("Skipping loading mod " + pckFilename + ", user has not yet seen the mods warning");
			}
			else if (flag2)
			{
				Log.Warn("Tried to load mod with PCK name " + pckName + ", but a mod is already loaded with that name!");
			}
			Mod mod = new Mod
			{
				pckName = pckName,
				modSource = source,
				manifest = null,
				wasLoaded = false
			};
			_mods.Add(mod);
			ModManager.OnModDetected?.Invoke(mod);
			return;
		}
		try
		{
			if (!File.Exists(Path.Combine(dirAccess.GetCurrentDir(), pckFilename)))
			{
				throw new InvalidOperationException("PCK not found at path " + pckFilename + "!");
			}
			string text = pckName + ".dll";
			if (dirAccess.FileExists(text))
			{
				Log.Info("Loading assembly DLL " + text);
				AssemblyLoadContext loadContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
				if (loadContext != null)
				{
					assembly = loadContext.LoadFromAssemblyPath(Path.Combine(dirAccess.GetCurrentDir(), text));
				}
			}
			if (!ProjectSettings.LoadResourcePack(Path.Combine(dirAccess.GetCurrentDir(), pckFilename)))
			{
				throw new InvalidOperationException("Godot errored while loading PCK file " + pckName + "!");
			}
			if (!ResourceLoader.Exists("res://mod_manifest.json"))
			{
				throw new InvalidOperationException(pckFilename + " did not supply a mod manifest!");
			}
			using FileAccessStream utf8Json = new FileAccessStream("res://mod_manifest.json", Godot.FileAccess.ModeFlags.Read);
			ModManifest modManifest = JsonSerializer.Deserialize(utf8Json, JsonSerializationUtility.GetTypeInfo<ModManifest>());
			if (modManifest == null)
			{
				throw new InvalidOperationException("JSON deserialization returned null when trying to deserialize mod manifest!");
			}
			if (!string.Equals(modManifest.pckName, pckName, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException($"PCK name in mod manifest {modManifest.pckName} does not match the pck {pckName} we're currently loading!");
			}
			bool? assemblyLoadedSuccessfully = null;
			if (assembly != null)
			{
				assemblyLoadedSuccessfully = true;
				List<Type> list = (from t in assembly.GetTypes()
					where t.GetCustomAttribute<ModInitializerAttribute>() != null
					select t).ToList();
				if (list.Count > 0)
				{
					foreach (Type item in list)
					{
						Log.Info($"Calling initializer method of type {item} for {assembly}");
						bool flag3 = CallModInitializer(item);
						assemblyLoadedSuccessfully = assemblyLoadedSuccessfully.Value && flag3;
					}
				}
				else
				{
					try
					{
						Log.Info($"No ModInitializerAttribute detected. Calling Harmony.PatchAll for {assembly}");
						Harmony harmony = new Harmony((modManifest.author ?? "unknown") + "." + pckName);
						harmony.PatchAll(assembly);
					}
					catch (Exception value)
					{
						Log.Error($"Exception caught while trying to run PatchAll on assembly {assembly}:\n{value}");
						assemblyLoadedSuccessfully = false;
					}
				}
			}
			Log.Info($"Finished mod initialization for '{modManifest.name}' ({modManifest.pckName}).");
			Mod mod2 = new Mod
			{
				pckName = pckName,
				modSource = source,
				manifest = modManifest,
				wasLoaded = true,
				assembly = assembly,
				assemblyLoadedSuccessfully = assemblyLoadedSuccessfully
			};
			_mods.Add(mod2);
			ModManager.OnModDetected?.Invoke(mod2);
		}
		catch (Exception value2)
		{
			Log.Error($"Error loading mod {pckFilename}: {value2}");
		}
	}

	private static bool CallModInitializer(Type initializerType)
	{
		ModInitializerAttribute customAttribute = initializerType.GetCustomAttribute<ModInitializerAttribute>();
		MethodInfo method = initializerType.GetMethod(customAttribute.initializerMethod, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (method == null)
		{
			method = initializerType.GetMethod(customAttribute.initializerMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				Log.Error($"Tried to call mod initializer {initializerType.Name}.{customAttribute.initializerMethod} but it's not static! Declare it to be static");
			}
			else
			{
				Log.Error($"Found mod initializer class of type {initializerType}, but it does not contain the method {customAttribute.initializerMethod} declared in the ModInitializerAttribute!");
			}
			return false;
		}
		try
		{
			Log.Info($"Calling initializer method {initializerType.Name}.{customAttribute.initializerMethod}...");
			method.Invoke(null, null);
		}
		catch (Exception value)
		{
			Log.Error($"Exception thrown when calling mod initializer of type {initializerType}: {value}");
			return false;
		}
		return true;
	}

	[IteratorStateMachine(typeof(_003CGetModdedLocTables_003Ed__24))]
	public static IEnumerable<string> GetModdedLocTables(string language, string file)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetModdedLocTables_003Ed__24(-2)
		{
			_003C_003E3__language = language,
			_003C_003E3__file = file
		};
	}

	public static List<string>? GetModNameList()
	{
		if (LoadedMods.Count == 0)
		{
			return null;
		}
		return LoadedMods.Select((Mod m) => m.manifest.pckName + m.manifest.version).ToList();
	}

	private static Assembly HandleAssemblyResolveFailure(object? source, ResolveEventArgs ev)
	{
		if (ev.Name.StartsWith("sts2,"))
		{
			Log.Info($"Failed to resolve assembly '{ev.Name}' but it looks like the STS2 assembly. Resolving using {Assembly.GetExecutingAssembly()}");
			return Assembly.GetExecutingAssembly();
		}
		if (ev.Name.StartsWith("0Harmony,"))
		{
			Log.Info($"Failed to resolve assembly '{ev.Name}' but it looks like the Harmony assembly. Resolving using {typeof(Harmony).Assembly}");
			return typeof(Harmony).Assembly;
		}
		return null;
	}

	public static void CallMetricsHooks(SerializableRun run, bool isVictory, ulong localPlayerId)
	{
		ModManager.OnMetricsUpload?.Invoke(run, isVictory, localPlayerId);
	}

	public static void Dispose()
	{
		_steamItemInstalledCallback?.Dispose();
	}
}
