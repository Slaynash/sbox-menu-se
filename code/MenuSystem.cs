global using Sandbox.Menu;
global using Sandbox.UI;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using static Sandbox.Internal.GlobalGameNamespace;
using Sandbox;
using Sandbox.UI.Dev;
using Sandbox.VR;
using System;

[Library]
public partial class MenuSystem : Sandbox.Internal.IMenuSystem
{
	public static MenuSystem Instance;

	MainMenuPanel Menu;
	DevLayer Dev;

	VROverlayPanel VRMenu;

	public Action<Package> OnPackageSelected { get; set; }

	[ConVar( "show_version_overlay" )]
	public static bool ShowOverlay { get; set; } = true;

	public void Init()
	{
		Instance = this;

		// Creation order is important
		// Panel created first will be on top

		Dev = new DevLayer();
		MenuOverlay.Init();
		Menu = new MainMenuPanel();

		// Make a seperate instance of the menu for VR, lets you still use dev stuff on desktop
		// Let's us whack up the resolution / scale too so it looks good
		if ( Game.IsRunningInVR )
		{
			VRMenu = new VROverlayPanel( new MainMenuPanel() )
			{
				Transform = new Transform( Vector3.Forward * 38.0f + Vector3.Up * 58.0f ),
				Width = 40.0f,
				Curvature = 0.2f,
			};
		}
	}

	public void Shutdown()
	{
		MenuOverlay.Shutdown();

		VRMenu?.Dispose();
		VRMenu = null;

		Menu?.Delete();
		Menu = null;

		Dev?.Delete();
		Dev = null;
	}

	public void Tick()
	{
		Menu?.SetClass( "has-game", Game.InGame );

	}

	public void SetVRMenuScreen( bool show )
	{
		if ( VRMenu != null )
		{
			VRMenu.Visible = show;
		}
	}

	public void SetMenuScreen( bool show )
	{
		Menu.SetClass( "hidden", !show );

		if ( show )
		{
			Menu.Focus();
		}
		else
		{
			Menu.Blur();
		}

		if ( VRMenu != null )
		{
			VRMenu.Visible = show;
		}
	}

	public bool IsMenuScreenVisible()
	{
		return Menu.IsVisible;
	}

	public void Popup( string type, string title, string subtitle )
	{
		Menu?.ShowPopup( type, title, subtitle );
	}

	public string Url
	{
		get => MainMenuPanel.Instance.Navigator.CurrentUrl;
		set => MainMenuPanel.Instance.Navigator.Navigate( value );
	}

	public bool ForceCursorVisible => DeveloperMode.Open;
}
