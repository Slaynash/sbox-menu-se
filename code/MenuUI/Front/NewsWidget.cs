using Sandbox;

public partial class NewsWidget
{
	class Blog
	{
		public int BlogId { get; set; }
		public int Created { get; set; }
		public string Title { get; set; }
		public string Url { get; set; }
		public string Header { get; set; }
		public string Summary { get; set; }
		public string Tags { get; set; }
	}

	static System.Uri BlogsUrl = new( "https://api.facepunch.com/api/public/blogs/20" );

	async Task<Blog[]> GetBlogsAsync()
	{
		try
		{
			return await Http.RequestJsonAsync<Blog[]>( BlogsUrl.ToString() );
		}
		catch ( System.Exception e )
		{
			Log.Warning( $"Couldn't load data for NewsWidget: {e}" );
			return null;
		}
	}
}

