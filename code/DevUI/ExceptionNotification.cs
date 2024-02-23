using Sandbox.UI.Construct;
using System.Linq;

namespace Sandbox.UI.Dev
{
	public class ExceptionNotification : Panel
	{
		Label message;
		RealTimeSince TimeSinceLastError;

		public ExceptionNotification()
		{
			Add.Icon( "error" );

			message = Add.Label( "Something went wrong! This is an exception notice!", "message" );
			SetClass( "hidden", true );
			TimeSinceLastError = 100;
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "hidden", TimeSinceLastError > 15 );
			SetClass( "fresh", TimeSinceLastError < 0.3f );
		}

		protected override void OnMouseDown( MousePanelEvent e )
		{
			base.OnMouseDown( e );

			TimeSinceLastError = 100;
		}

		internal void OnException( LogEvent entry )
		{
			message.Text = entry.Message?.Split( '\n', System.StringSplitOptions.RemoveEmptyEntries ).FirstOrDefault()?.Trim() ?? "null"; 
			TimeSinceLastError = 0;
		}
	}
}
