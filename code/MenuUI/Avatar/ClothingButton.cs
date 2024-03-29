﻿
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Menu;

public class ClothingButton : Panel
{

	public Clothing Clothing { get; set; }
	public bool HasVariations { get; set; }

	public Panel ImagePanel;

	protected override void OnParametersSet()
	{
		base.OnParametersSet();

		if ( Clothing == null ) return;

		DeleteChildren( true );

		ImagePanel = new Panel( this, "image" );
		ImagePanel.Style.SetBackgroundImage( Clothing.Icon.Path );

		if( HasVariations )
		{
			AddClass( "has-variations" );
			Add.Icon( "palette", "variations-icon" );
		}
	}

	public override void Tick()
	{
		base.Tick();

		var parent = Ancestors.OfType<Avatar>().FirstOrDefault();
		if ( parent == null ) return;

		var equipped = parent.Container.Has( Clothing );
		var variationEquipped = HasVariations && parent.Container.Clothing.Any( x => x.Parent == Clothing );

		SetClass( "active", equipped || variationEquipped );
	}

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

		var parent = Ancestors.OfType<Avatar>().FirstOrDefault();
		if ( parent == null ) return;

		parent.Select( Clothing );
	}

}
