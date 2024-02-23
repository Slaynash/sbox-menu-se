using Sandbox;
using Sandbox.UI.Construct;
using System;

public class MenuOverlay : RootPanel
{
	public static MenuOverlay Instance;

	public static void Init()
	{
		Shutdown();

		Instance = new MenuOverlay();
	}


	public static void Shutdown()
	{
		Instance?.Delete();
		Instance = null;
	}

	public Panel PopupCanvasTop;
	Panel PopupCanvas;

	static Queue<Panel> Popups = new Queue<Panel>();
	static Panel CurrentPopup;

	public MenuOverlay()
	{
		StyleSheet.Load( "/styles/overlay.scss" );

		PopupCanvas = Add.Panel( "popup_canvas" );
		PopupCanvasTop = Add.Panel( "popup_canvas_top" );

		AddChild<VersionOverlay>();
		AddChild<LoadingOverlay>();
		AddChild<MicOverlay>();
	}

	/// <summary>
	/// Dismiss the current popup
	/// </summary>
	public static async Task SkipPopup()
	{
		if ( CurrentPopup == null ) return;

		CurrentPopup.Delete();

		await GameTask.DelayRealtimeSeconds( 0.3f );

		CurrentPopup = null;

		_ = SwitchPopup();
	}

	/// <summary>
	/// Dismiss the current popup
	/// </summary>
	static async Task SwitchPopup()
	{
		if ( Popups.Count == 0 )
			return;

		CurrentPopup = Popups.Dequeue();
		CurrentPopup.Parent = Instance.PopupCanvas;

		var popup = CurrentPopup;

		if ( CurrentPopup.HasClass( "has-options" ) )
			await GameTask.DelayRealtimeSeconds( 6.0f );

		await GameTask.DelayRealtimeSeconds( 4.0f );


		if ( CurrentPopup != popup )
			return;

		await SkipPopup();

	}

	/// <summary>
	/// Dismiss the current popup
	/// </summary>
	public static void AddPopup( Panel popup )
	{
		popup.AddClass( "popup" );
		popup.AddClass( "hidden" );

		Popups.Enqueue( popup );

		if ( CurrentPopup == null )
		{
			_ = SwitchPopup();
		}
	}

	/// <summary>
	/// Add a message
	/// </summary>
	public static void Message( string message, string icon = "info" )
	{
		var popup = new Panel( null, "has-message" );
		if ( popup == null ) return;

		popup.Add.Icon( icon );

		popup.Add.Label( message, "message" );
		popup.AddEventListener( "onmousedown", () => _ = SkipPopup() );

		AddPopup( popup );
	}

	/// <summary>
	/// Add a message
	/// </summary>
	public static void Message( string message, Texture image )
	{
		var popup = new Panel( null, "has-message" );
		if ( popup == null ) return;

		var icon = popup.Add.Panel();
		icon.AddClass( "iconpanel" );
		icon.Style.SetBackgroundImage( image );

		popup.Add.Label( message, "message" );
		popup.AddEventListener( "onmousedown", () => _ = SkipPopup() );

		AddPopup( popup );
	}

	/// <summary>
	/// Add a message
	/// </summary>
	public static void Message( string message, string subtitle, Texture image )
	{
		var popup = new Panel( null, "has-message" );
		if ( popup == null ) return;

		var icon = popup.Add.Panel();
		icon.AddClass( "iconpanel" );
		icon.Style.SetBackgroundImage( image );

		popup.Add.Label( message, "message" );

		if ( subtitle != null )
		{
			popup.Add.Label( subtitle, "subtitle" );
			popup.AddClass( "has-subtitle" );
		}

		popup.AddEventListener( "onmousedown", () => _ = SkipPopup() );

		AddPopup( popup );
	}

	/// <summary>
	/// Add a question
	/// </summary>
	public static void Question( string message, string icon, Action yes, Action no )
	{
		var popup = new Panel( null, "has-message has-options" );
		if ( popup == null ) return;

		popup.Add.Icon( icon );
		popup.Add.Label( message, "message" );

		var options = popup.Add.Panel( "options" );

		options.Add.ButtonWithIcon( null, "close", null, () => { no?.Invoke(); _ = SkipPopup(); } );
		options.Add.ButtonWithIcon( null, "done", null, () => { yes?.Invoke(); _ = SkipPopup(); } );

		popup.Add.Panel( "progress-bar" );

		AddPopup( popup );
	}
}
