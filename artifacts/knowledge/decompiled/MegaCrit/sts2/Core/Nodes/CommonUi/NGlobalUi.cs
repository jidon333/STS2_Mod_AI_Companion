using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NGlobalUi.cs")]
public class NGlobalUi : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnWindowChange = "OnWindowChange";

		public static readonly StringName ReparentCard = "ReparentCard";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName TopBar = "TopBar";

		public static readonly StringName Overlays = "Overlays";

		public static readonly StringName CapstoneContainer = "CapstoneContainer";

		public static readonly StringName RelicInventory = "RelicInventory";

		public static readonly StringName EventCardPreviewContainer = "EventCardPreviewContainer";

		public static readonly StringName CardPreviewContainer = "CardPreviewContainer";

		public static readonly StringName MessyCardPreviewContainer = "MessyCardPreviewContainer";

		public static readonly StringName GridCardPreviewContainer = "GridCardPreviewContainer";

		public static readonly StringName AboveTopBarVfxContainer = "AboveTopBarVfxContainer";

		public static readonly StringName MapScreen = "MapScreen";

		public static readonly StringName MultiplayerPlayerContainer = "MultiplayerPlayerContainer";

		public static readonly StringName TimeoutOverlay = "TimeoutOverlay";

		public static readonly StringName SubmenuStack = "SubmenuStack";

		public static readonly StringName TargetManager = "TargetManager";

		public static readonly StringName _window = "_window";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const float _maxNarrowRatio = 1.3333334f;

	private const float _maxWideRatio = 2.3888888f;

	private Window _window;

	public NTopBar TopBar { get; private set; }

	public NOverlayStack Overlays { get; private set; }

	public NCapstoneContainer CapstoneContainer { get; private set; }

	public NRelicInventory RelicInventory { get; private set; }

	public Control EventCardPreviewContainer { get; private set; }

	public Control CardPreviewContainer { get; private set; }

	public NMessyCardPreviewContainer MessyCardPreviewContainer { get; private set; }

	public NGridCardPreviewContainer GridCardPreviewContainer { get; private set; }

	public Control AboveTopBarVfxContainer { get; private set; }

	public NMapScreen MapScreen { get; private set; }

	public NMultiplayerPlayerStateContainer MultiplayerPlayerContainer { get; private set; }

	public NMultiplayerTimeoutOverlay TimeoutOverlay { get; private set; }

	public NCapstoneSubmenuStack SubmenuStack { get; private set; }

	public NTargetManager TargetManager { get; private set; }

	public override void _Ready()
	{
		_window = GetTree().Root;
		_window.Connect(Viewport.SignalName.SizeChanged, Callable.From(OnWindowChange));
		EventCardPreviewContainer = GetNode<Control>("%EventCardPreviewContainer");
		CardPreviewContainer = GetNode<Control>("%CardPreviewContainer");
		MessyCardPreviewContainer = GetNode<NMessyCardPreviewContainer>("%MessyCardPreviewContainer");
		GridCardPreviewContainer = GetNode<NGridCardPreviewContainer>("%GridCardPreviewContainer");
		TopBar = GetNode<NTopBar>("%TopBar");
		Overlays = GetNode<NOverlayStack>("%OverlayScreensContainer");
		CapstoneContainer = GetNode<NCapstoneContainer>("%CapstoneScreenContainer");
		MapScreen = GetNode<NMapScreen>("%MapScreen");
		SubmenuStack = GetNode<NCapstoneSubmenuStack>("%CapstoneSubmenuStack");
		RelicInventory = GetNode<NRelicInventory>("%RelicInventory");
		MultiplayerPlayerContainer = GetNode<NMultiplayerPlayerStateContainer>("%MultiplayerPlayerContainer");
		TargetManager = GetNode<NTargetManager>("TargetManager");
		TimeoutOverlay = GetNode<NMultiplayerTimeoutOverlay>("%MultiplayerTimeoutOverlay");
		AboveTopBarVfxContainer = GetNode<Control>("%AboveTopBarVfxContainer");
	}

	private void OnWindowChange()
	{
		if (SaveManager.Instance.SettingsSave.AspectRatioSetting == AspectRatioSetting.Auto)
		{
			float num = (float)_window.Size.X / (float)_window.Size.Y;
			if (num > 2.3888888f)
			{
				_window.ContentScaleAspect = Window.ContentScaleAspectEnum.KeepWidth;
				_window.ContentScaleSize = new Vector2I(2580, 1080);
			}
			else if (num < 1.3333334f)
			{
				_window.ContentScaleAspect = Window.ContentScaleAspectEnum.KeepHeight;
				_window.ContentScaleSize = new Vector2I(1680, 1260);
			}
			else
			{
				_window.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;
				_window.ContentScaleSize = new Vector2I(1680, 1080);
			}
		}
	}

	public void ReparentCard(NCard card)
	{
		Vector2 globalPosition = card.GlobalPosition;
		card.GetParent()?.RemoveChildSafely(card);
		TopBar.TrailContainer.AddChildSafely(card);
		card.GlobalPosition = globalPosition;
	}

	public void Initialize(RunState runState)
	{
		TopBar.Initialize(runState);
		MultiplayerPlayerContainer.Initialize(runState);
		RelicInventory.Initialize(runState);
		MapScreen.Initialize(runState);
		TimeoutOverlay.Initialize(RunManager.Instance.NetService, isGameLevel: false);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ReparentCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnWindowChange && args.Count == 0)
		{
			OnWindowChange();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReparentCard && args.Count == 1)
		{
			ReparentCard(VariantUtils.ConvertTo<NCard>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnWindowChange)
		{
			return true;
		}
		if (method == MethodName.ReparentCard)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.TopBar)
		{
			TopBar = VariantUtils.ConvertTo<NTopBar>(in value);
			return true;
		}
		if (name == PropertyName.Overlays)
		{
			Overlays = VariantUtils.ConvertTo<NOverlayStack>(in value);
			return true;
		}
		if (name == PropertyName.CapstoneContainer)
		{
			CapstoneContainer = VariantUtils.ConvertTo<NCapstoneContainer>(in value);
			return true;
		}
		if (name == PropertyName.RelicInventory)
		{
			RelicInventory = VariantUtils.ConvertTo<NRelicInventory>(in value);
			return true;
		}
		if (name == PropertyName.EventCardPreviewContainer)
		{
			EventCardPreviewContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.CardPreviewContainer)
		{
			CardPreviewContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.MessyCardPreviewContainer)
		{
			MessyCardPreviewContainer = VariantUtils.ConvertTo<NMessyCardPreviewContainer>(in value);
			return true;
		}
		if (name == PropertyName.GridCardPreviewContainer)
		{
			GridCardPreviewContainer = VariantUtils.ConvertTo<NGridCardPreviewContainer>(in value);
			return true;
		}
		if (name == PropertyName.AboveTopBarVfxContainer)
		{
			AboveTopBarVfxContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.MapScreen)
		{
			MapScreen = VariantUtils.ConvertTo<NMapScreen>(in value);
			return true;
		}
		if (name == PropertyName.MultiplayerPlayerContainer)
		{
			MultiplayerPlayerContainer = VariantUtils.ConvertTo<NMultiplayerPlayerStateContainer>(in value);
			return true;
		}
		if (name == PropertyName.TimeoutOverlay)
		{
			TimeoutOverlay = VariantUtils.ConvertTo<NMultiplayerTimeoutOverlay>(in value);
			return true;
		}
		if (name == PropertyName.SubmenuStack)
		{
			SubmenuStack = VariantUtils.ConvertTo<NCapstoneSubmenuStack>(in value);
			return true;
		}
		if (name == PropertyName.TargetManager)
		{
			TargetManager = VariantUtils.ConvertTo<NTargetManager>(in value);
			return true;
		}
		if (name == PropertyName._window)
		{
			_window = VariantUtils.ConvertTo<Window>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.TopBar)
		{
			NTopBar from = TopBar;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Overlays)
		{
			NOverlayStack from2 = Overlays;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.CapstoneContainer)
		{
			NCapstoneContainer from3 = CapstoneContainer;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.RelicInventory)
		{
			NRelicInventory from4 = RelicInventory;
			value = VariantUtils.CreateFrom(in from4);
			return true;
		}
		Control from5;
		if (name == PropertyName.EventCardPreviewContainer)
		{
			from5 = EventCardPreviewContainer;
			value = VariantUtils.CreateFrom(in from5);
			return true;
		}
		if (name == PropertyName.CardPreviewContainer)
		{
			from5 = CardPreviewContainer;
			value = VariantUtils.CreateFrom(in from5);
			return true;
		}
		if (name == PropertyName.MessyCardPreviewContainer)
		{
			NMessyCardPreviewContainer from6 = MessyCardPreviewContainer;
			value = VariantUtils.CreateFrom(in from6);
			return true;
		}
		if (name == PropertyName.GridCardPreviewContainer)
		{
			NGridCardPreviewContainer from7 = GridCardPreviewContainer;
			value = VariantUtils.CreateFrom(in from7);
			return true;
		}
		if (name == PropertyName.AboveTopBarVfxContainer)
		{
			from5 = AboveTopBarVfxContainer;
			value = VariantUtils.CreateFrom(in from5);
			return true;
		}
		if (name == PropertyName.MapScreen)
		{
			NMapScreen from8 = MapScreen;
			value = VariantUtils.CreateFrom(in from8);
			return true;
		}
		if (name == PropertyName.MultiplayerPlayerContainer)
		{
			NMultiplayerPlayerStateContainer from9 = MultiplayerPlayerContainer;
			value = VariantUtils.CreateFrom(in from9);
			return true;
		}
		if (name == PropertyName.TimeoutOverlay)
		{
			NMultiplayerTimeoutOverlay from10 = TimeoutOverlay;
			value = VariantUtils.CreateFrom(in from10);
			return true;
		}
		if (name == PropertyName.SubmenuStack)
		{
			NCapstoneSubmenuStack from11 = SubmenuStack;
			value = VariantUtils.CreateFrom(in from11);
			return true;
		}
		if (name == PropertyName.TargetManager)
		{
			NTargetManager from12 = TargetManager;
			value = VariantUtils.CreateFrom(in from12);
			return true;
		}
		if (name == PropertyName._window)
		{
			value = VariantUtils.CreateFrom(in _window);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._window, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.TopBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Overlays, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CapstoneContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.RelicInventory, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.EventCardPreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CardPreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.MessyCardPreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.GridCardPreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.AboveTopBarVfxContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.MapScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.MultiplayerPlayerContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.TimeoutOverlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.SubmenuStack, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.TargetManager, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		StringName topBar = PropertyName.TopBar;
		NTopBar from = TopBar;
		info.AddProperty(topBar, Variant.From(in from));
		StringName overlays = PropertyName.Overlays;
		NOverlayStack from2 = Overlays;
		info.AddProperty(overlays, Variant.From(in from2));
		StringName capstoneContainer = PropertyName.CapstoneContainer;
		NCapstoneContainer from3 = CapstoneContainer;
		info.AddProperty(capstoneContainer, Variant.From(in from3));
		StringName relicInventory = PropertyName.RelicInventory;
		NRelicInventory from4 = RelicInventory;
		info.AddProperty(relicInventory, Variant.From(in from4));
		StringName eventCardPreviewContainer = PropertyName.EventCardPreviewContainer;
		Control from5 = EventCardPreviewContainer;
		info.AddProperty(eventCardPreviewContainer, Variant.From(in from5));
		StringName cardPreviewContainer = PropertyName.CardPreviewContainer;
		from5 = CardPreviewContainer;
		info.AddProperty(cardPreviewContainer, Variant.From(in from5));
		StringName messyCardPreviewContainer = PropertyName.MessyCardPreviewContainer;
		NMessyCardPreviewContainer from6 = MessyCardPreviewContainer;
		info.AddProperty(messyCardPreviewContainer, Variant.From(in from6));
		StringName gridCardPreviewContainer = PropertyName.GridCardPreviewContainer;
		NGridCardPreviewContainer from7 = GridCardPreviewContainer;
		info.AddProperty(gridCardPreviewContainer, Variant.From(in from7));
		StringName aboveTopBarVfxContainer = PropertyName.AboveTopBarVfxContainer;
		from5 = AboveTopBarVfxContainer;
		info.AddProperty(aboveTopBarVfxContainer, Variant.From(in from5));
		StringName mapScreen = PropertyName.MapScreen;
		NMapScreen from8 = MapScreen;
		info.AddProperty(mapScreen, Variant.From(in from8));
		StringName multiplayerPlayerContainer = PropertyName.MultiplayerPlayerContainer;
		NMultiplayerPlayerStateContainer from9 = MultiplayerPlayerContainer;
		info.AddProperty(multiplayerPlayerContainer, Variant.From(in from9));
		StringName timeoutOverlay = PropertyName.TimeoutOverlay;
		NMultiplayerTimeoutOverlay from10 = TimeoutOverlay;
		info.AddProperty(timeoutOverlay, Variant.From(in from10));
		StringName submenuStack = PropertyName.SubmenuStack;
		NCapstoneSubmenuStack from11 = SubmenuStack;
		info.AddProperty(submenuStack, Variant.From(in from11));
		StringName targetManager = PropertyName.TargetManager;
		NTargetManager from12 = TargetManager;
		info.AddProperty(targetManager, Variant.From(in from12));
		info.AddProperty(PropertyName._window, Variant.From(in _window));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.TopBar, out var value))
		{
			TopBar = value.As<NTopBar>();
		}
		if (info.TryGetProperty(PropertyName.Overlays, out var value2))
		{
			Overlays = value2.As<NOverlayStack>();
		}
		if (info.TryGetProperty(PropertyName.CapstoneContainer, out var value3))
		{
			CapstoneContainer = value3.As<NCapstoneContainer>();
		}
		if (info.TryGetProperty(PropertyName.RelicInventory, out var value4))
		{
			RelicInventory = value4.As<NRelicInventory>();
		}
		if (info.TryGetProperty(PropertyName.EventCardPreviewContainer, out var value5))
		{
			EventCardPreviewContainer = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.CardPreviewContainer, out var value6))
		{
			CardPreviewContainer = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.MessyCardPreviewContainer, out var value7))
		{
			MessyCardPreviewContainer = value7.As<NMessyCardPreviewContainer>();
		}
		if (info.TryGetProperty(PropertyName.GridCardPreviewContainer, out var value8))
		{
			GridCardPreviewContainer = value8.As<NGridCardPreviewContainer>();
		}
		if (info.TryGetProperty(PropertyName.AboveTopBarVfxContainer, out var value9))
		{
			AboveTopBarVfxContainer = value9.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.MapScreen, out var value10))
		{
			MapScreen = value10.As<NMapScreen>();
		}
		if (info.TryGetProperty(PropertyName.MultiplayerPlayerContainer, out var value11))
		{
			MultiplayerPlayerContainer = value11.As<NMultiplayerPlayerStateContainer>();
		}
		if (info.TryGetProperty(PropertyName.TimeoutOverlay, out var value12))
		{
			TimeoutOverlay = value12.As<NMultiplayerTimeoutOverlay>();
		}
		if (info.TryGetProperty(PropertyName.SubmenuStack, out var value13))
		{
			SubmenuStack = value13.As<NCapstoneSubmenuStack>();
		}
		if (info.TryGetProperty(PropertyName.TargetManager, out var value14))
		{
			TargetManager = value14.As<NTargetManager>();
		}
		if (info.TryGetProperty(PropertyName._window, out var value15))
		{
			_window = value15.As<Window>();
		}
	}
}
