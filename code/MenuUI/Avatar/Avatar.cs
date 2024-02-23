
using Sandbox;
namespace Menu;

public partial class Avatar : Panel
{

	record struct ItemGroup( string subcategory, List<Clothing> clothing );
	record struct ClothingCategory( Clothing.ClothingCategory category, string title, string icon, Vector3? position = default );

	List<ClothingCategory> Categories = new( new[]
	{
		new ClothingCategory( Clothing.ClothingCategory.Skin, "Skin", "emoji_people" ),
		new ClothingCategory( Clothing.ClothingCategory.Facial, "Facial", "sentiment_very_satisfied", new( 0, 0, 8 ) ),
		new ClothingCategory( Clothing.ClothingCategory.Hair, "Hair", "face", new( 0, 0, 8 ) ),
		new ClothingCategory( Clothing.ClothingCategory.Hat, "Hat", "add_reaction", new( 0, 0, 8 ) ),
		new ClothingCategory( Clothing.ClothingCategory.Tops, "Tops", "personal_injury" ),
		new ClothingCategory( Clothing.ClothingCategory.Gloves, "Gloves", "front_hand" ),
		new ClothingCategory( Clothing.ClothingCategory.Bottoms, "Bottoms", "airline_seat_legroom_reduced" ),
		new ClothingCategory( Clothing.ClothingCategory.Footwear, "Footwear", "do_not_step" ),
	} );

	public Panel ClothingCanvas { get; set; }
	public ModelViewer ModelViewer { get; set; }

	public ClothingContainer Container = new();
	public List<SceneModel> ClothingModels = new();

	public Clothing.ClothingCategory CurrentCategory { get; set; }

	SceneMap scenemap;
	string originalValue;
	RealTimeSince timeSinceSave = 0;
	ModelViewer.Orbit orbitCamera;
	float fieldOfView = 50.0f;
	float targetDistance = 25;
	bool displayingVariations = false;
	List<ItemGroup> ItemGroups = new();

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( firstTime )
		{
			orbitCamera = new ModelViewer.Orbit( Vector3.Up * 48.0f, Vector3.Backward + Vector3.Down * 0.2f, 100.0f );

			ModelViewer.SetModel( "models/citizen/citizen.vmdl" );
			ModelViewer.CameraController = orbitCamera;
			ModelViewer.Camera.Name = "Avatar Setup";

			scenemap = new SceneMap( ModelViewer.World, "maps/scenemaps/menu_avatar/avatar_menu_map" );

			ChangeCategory( Clothing.ClothingCategory.Skin );

			Load();
		}
	}

	public void ChangeCategory( Clothing.ClothingCategory category )
	{
		CurrentCategory = category;

		ItemGroups.Clear();

		var subcategoryGroups = ResourceLibrary.GetAll<Clothing>()
			.Where( x => x.Category == category && x.Parent == null )
			.OrderBy( x => x.SubCategory )
			.GroupBy( x => x.SubCategory?.Trim() ?? string.Empty );

		foreach ( var group in subcategoryGroups )
		{
			var key = string.IsNullOrEmpty( group.Key ) ? category.ToString() : group.Key;
			ItemGroups.Add( new ItemGroup( key, group.ToList() ) );
		}

		var categoryInfo = Categories.FirstOrDefault( x => x.category == category );
		var position = categoryInfo.position ?? new Vector3( 0, 0, 30 );

		targetDistance = position.z;

		ClothingCanvas.ScrollOffset = 0;

		selectedClothing = null;
		displayingVariations = false;
		StateHasChanged();
	}

	private Clothing selectedClothing;

	public void Select( Clothing clothing )
	{
		selectedClothing = clothing;

		var hasVariations = HasVariations( clothing );
		var isVariation = IsVariation( clothing );

		if ( !hasVariations || displayingVariations )
		{
			AddClothing( clothing );
		}

		if ( isVariation || !hasVariations ) return;

		var variations = ResourceLibrary.GetAll<Clothing>()
			.Where( x => x == clothing || x.Parent == clothing )
			.ToList();

		displayingVariations = true;

		ItemGroups.Clear();
		ItemGroups.Add( new()
		{
			clothing = new( variations ),
			subcategory = clothing.Title
		} );
		ClothingCanvas.ScrollOffset = 0;
		StateHasChanged();
	}

	bool IsVariation( Clothing clothing )
	{
		return clothing.Parent != null;
	}

	bool HasVariations( Clothing clothing )
	{
		return ResourceLibrary.GetAll<Clothing>().Any( x => x.Parent == clothing );
	}

	void AddClothing( Clothing clothing )
	{
		Container.Toggle( clothing );

		SetClass( "is-dirty", true );

		DressModel();
	}

	void DressModel()
	{
		foreach ( var model in ClothingModels )
		{
			ModelViewer.RemoveModel( model );
		}
		ClothingModels.Clear();

		ClothingModels = Clothing.DressSceneObject( ModelViewer.Model, Container.Clothing );
		ModelViewer.AddModels( ClothingModels );

		timeSinceSave = 0;
	}

	public override void OnMouseWheel( Vector2 value )
	{
		base.OnMouseWheel( value );

		targetDistance += value.y * 2.0f;

		targetDistance = targetDistance.Clamp( 4, 30 );
	}

	public override void Tick()
	{
		base.Tick();

		if ( ModelViewer.Model == null )
			return;

		Angles angles = new( 30, 200, 0 );
		Vector3 pos = Vector3.Up * 45 + angles.Forward * -50;

		float closeToFace = fieldOfView.LerpInverse( 30, 2 );

		var eyes = ModelViewer.Model.GetAttachment( "eyes" ) ?? default;

		orbitCamera.Distance = 500;
		orbitCamera.Center = Vector3.Lerp( Vector3.Up * 40.0f, eyes.Position, closeToFace );
		orbitCamera.PitchLimit = new Vector2( -2, 85 );
		orbitCamera.Offset = orbitCamera.Angles.ToRotation().Left * 45.0f.LerpTo( 5, closeToFace );

		//AvatarScene.World = AvatarWorld;
		ModelViewer.Camera.FieldOfView = fieldOfView;
		ModelViewer.Camera.ZNear = 5;
		ModelViewer.Camera.ZFar = 15000;
		ModelViewer.Camera.AmbientLightColor = Color.Gray * 0.01f;
		ModelViewer.Camera.BackgroundColor = Color.Black;

		ModelViewer.Model.SetAnimParameter( "b_grounded", true );
		ModelViewer.Model.SetAnimParameter( "aim_eyes", pos - eyes.Position );
		ModelViewer.Model.SetAnimParameter( "aim_head", pos - eyes.Position );
		ModelViewer.Model.SetAnimParameter( "aim_body", pos - eyes.Position );
		ModelViewer.Model.SetAnimParameter( "aim_body_weight", 1.0f );
		ModelViewer.Model.SetAnimParameter( "aim_head_weight", 1.0f );
		ModelViewer.Model.SetAnimParameter( "idle_states", 0 );

		//We should be using the map fog but use this for now. - louie
		ModelViewer.World.GradientFog.Enabled = true;
		ModelViewer.World.GradientFog.Color = new Color( 0.03f, 0.11f, 0.19f );
		ModelViewer.World.GradientFog.MaximumOpacity = 0.28f;
		ModelViewer.World.GradientFog.StartHeight = 0;
		ModelViewer.World.GradientFog.EndHeight = 2000;
		ModelViewer.World.GradientFog.DistanceFalloffExponent = 2;
		ModelViewer.World.GradientFog.VerticalFalloffExponent = 0;
		ModelViewer.World.GradientFog.StartDistance = 500;
		ModelViewer.World.GradientFog.EndDistance = 1000;

		fieldOfView = fieldOfView.LerpTo( targetDistance, Time.Delta * 10f, true );
	}

	void Load()
	{
		originalValue = ConsoleSystem.GetValue( "avatar" );

		Container.Deserialize( originalValue );
		DressModel();
	}

	public void CancelChanges()
	{
		SetClass( "is-dirty", false );
		ConsoleSystem.SetValue( "avatar", originalValue );
		Load();
	}

	public void SaveChanges()
	{
		var str = Container.Serialize();
		ConsoleSystem.SetValue( "avatar", str );

		//Event.Run( "avatar.changed" );

		SetClass( "is-dirty", false );
		timeSinceSave = 0;

		originalValue = str;
	}


}

