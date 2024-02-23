﻿using System.Linq;

namespace Sandbox.UI.Dev
{
	public class ConsoleOverlay : Panel
	{
		[ConVar( "consoleoverlay", Help = "Enable the console to draw at the top of the screen all the time", Saved = true )]
		public static bool ConsoleOverlayEnabled { get; set; }

		internal Panel Output;

		public ConsoleOverlay()
		{
			Output = Add.Panel( "output" );
			MenuUtility.AddLogger( OnConsoleMessage );
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "hidden", !ConsoleOverlayEnabled );
		}

		private void OnConsoleMessage( LogEvent e )
		{
			if ( !ConsoleOverlayEnabled )
				return;

			var entry = Output.AddChild<ConsoleEntry>();
			entry.SetLogEvent( e );
			entry.DeleteIn( 8 );

			var c = Output.Children.Count();

			for ( int i = 0; i < c - 10; i++ )
			{
				Output.Children.First().Delete( true );
			}
		}
	}
}
