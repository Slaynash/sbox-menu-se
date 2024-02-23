
using Sandbox;
using Sandbox.Menu;
using Sandbox.UI.Construct;
using System;

public partial class MainMenuPanel : RootPanel
{
	public static MainMenuPanel Instance { get; private set; }

	public Panel Popup { get; protected set; }
	public Label PopupTitle { get; protected set; }
	public Label PopupMessage { get; protected set; }

	public NavHostPanel Navigator { get; protected set; }

	public MainMenuPanel()
	{
		Popup = Add.Panel( "popup" );
		{
			var inner = Popup.Add.Panel( "inner" );
			PopupTitle = inner.Add.Label( "...", "title" );
			PopupMessage = inner.Add.Label( "...", "message" );
			inner.Add.Button( "Close", () => SetClass( "popup-active", false ) );
		}

		Instance = this;
	}

	protected override int BuildHash() => HashCode.Combine( Sandbox.LoadingScreen.IsVisible, Sandbox.LoadingScreen.Title );

	internal void ShowPopup( string type, string title, string subtitle )
	{
		AddClass( "popup-active" );

		PopupTitle.Text = title;
		PopupMessage.Text = subtitle;
	}

	GameClosing gameClosingPanel;

	public override void Tick()
	{
		base.Tick();

		if ( Game.InGame )
		{
			var startDelay = 0.2f;
			var holdDelay = 1.5f;

			if ( MenuUtility.EscapeTime > startDelay )
			{
				var et = MenuUtility.EscapeTime - startDelay;
				if ( !gameClosingPanel.IsValid() )
				{
					gameClosingPanel = new GameClosing();
					gameClosingPanel.Parent = MenuOverlay.Instance.PopupCanvasTop;
				}

				gameClosingPanel.Progress = (et / holdDelay);
				gameClosingPanel.StateHasChanged();

				if ( gameClosingPanel.Progress > 1 )
					Game.Close();
			}
			else
			{
				gameClosingPanel?.Delete();
				gameClosingPanel = null;
			}
		}
		else
		{
			gameClosingPanel?.Delete();
			gameClosingPanel = null;
		}

		if ( !IsVisible ) return;

		//
		// Alex: in VR, we don't want to show the menu controls while playing,
		// because the overlay will block all input to the game
		//
		if ( IsVR )
		{
			MenuSystem.Instance.SetVRMenuScreen( !Game.InGame );
		}

		SetClass( "is-vr", IsVR );
		SetClass( "has-streamer-account", Sandbox.MenuEngine.Account.HasLinkedStreamerServices );
		SetClass( "has-tools", Game.IsEditor );
	}

	public override void OnButtonEvent( ButtonEvent e )
	{
		if ( e.Pressed && e.Button == "escape" && Game.InGame )
		{
			MenuSystem.Instance.SetMenuScreen( !MenuSystem.Instance.IsMenuScreenVisible() );
		}

		base.OnButtonEvent( e );
	}
}
