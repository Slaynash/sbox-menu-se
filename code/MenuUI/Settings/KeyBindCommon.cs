
using Sandbox;
using Sandbox.UI;
using System;

namespace Menu.Settings;

public class KeyBindCommon : Label
{
	public InputAction Action { get; set; }
	public string BindGroup { get; set; }
	public int Slot { get; set; }

	bool Rebinding { get; set; }
	string TargetKey { get; set; }

	public KeyBindCommon()
	{
		AcceptsFocus = true;
		BindClass( "active", () => Rebinding );
		BindClass( "with-highlight", () => !string.IsNullOrEmpty( TargetKey ) );
	}

	public override void Tick()
	{
		if ( Rebinding && TargetKey == null )
		{
			Text = "PRESS A BUTTON";
			return;
		}

		if ( TargetKey  != null )
		{
			Text = TargetKey.ToUpperInvariant();
			return;
		}

		var str = MenuUtility.Input.GetBind( BindGroup, Action.Name, Slot, out var isDefault );

		if ( str == null && Slot == 0 )
		{
			str = Action.KeyboardCode;
			isDefault = true;
		}

		if ( str == null )
		{
			Text = "";
			return;
		}

		str = str.ToUpperInvariant();

		if ( isDefault )
		{
			Text = $"{str} (default)";
		}
		else
		{
			Text = str;
		}
	}

	protected override void OnMouseUp( MousePanelEvent e )
	{
		base.OnMouseUp( e );

		if ( e.Button == "mouseleft" )
		{
			Rebinding = true;
			TargetKey = null;

			MenuUtility.Input.TrapButtons( ( buttons ) =>
			{
				if ( buttons.Contains( "ESCAPE", StringComparer.OrdinalIgnoreCase ) )
				{
					Rebinding = false;
					TargetKey = null;
					CreateEvent( "onchange" );
					return;
				}

				TargetKey = string.Join( " + ", buttons.OrderBy( x => x ) );
				Rebinding = false;
				CreateEvent( "onchange" );
			} );
		}

		if ( e.Button == "mouseright" )
		{
			MenuUtility.Input.SetBind( BindGroup, Action.Name, null, Slot );
			TargetKey = null;
			CreateEvent( "onchange" );
		}
	}

	public void Apply()
	{
		if ( string.IsNullOrEmpty( TargetKey ) )
			return;

		MenuUtility.Input.SetBind( BindGroup, Action.Name, TargetKey, Slot );
		TargetKey = null;
	}

	public void Cancel()
	{
		TargetKey = null;
	}
}
