using Sandbox;
using Sandbox.UI;
using Sandbox.Network;

public partial class LobbyList : Panel
{
    private class ResolvedLobby
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string Game { get; set; }
        public string Image { get; set; }
        public string Map { get; set; }
        public int Members { get; set; }
        public int MaxMembers { get; set; }
        public bool Dev { get; set; }
        public bool OwnedByFriend { get; set; }
    }

    private float refreshDelta = 10.0f;

    List<ResolvedLobby> lobbies = new List<ResolvedLobby>();
    private float lastRefresh = 0;
    private bool isRefreshing = false;

    private ResolvedLobby selectedLobby = null;

    private async void LoadLobbies()
    {
        List<LobbyInformation> lobbyInfos = await Networking.QueryLobbies();
        Log.Info($"Found {lobbies.Count} lobbies.");

        List<ResolvedLobby> resolvedLobbies = new List<ResolvedLobby>();
        
        foreach ( LobbyInformation lobbyInfo in lobbyInfos.Where(lobbyInfo => MenuUtility.Friends.Any(friend => friend.Id == lobbyInfo.OwnerId)) )
        {
            resolvedLobbies.Add(new ResolvedLobby
            {
                Id = lobbyInfo.LobbyId,
                Name = lobbyInfo.Name,
                Game = lobbyInfo.Game,
                Image = "https://i.pinimg.com/originals/ae/24/87/ae24874dd301843548c034a3d2973658.png",
                Map = lobbyInfo.Map,
                Members = lobbyInfo.Members,
                MaxMembers = lobbyInfo.MaxMembers,
                Dev = lobbyInfo.Data["dev"] == "1",
                OwnedByFriend = true
            });

            // TODO update selectedLobby
        }

        foreach ( LobbyInformation lobbyInfo in lobbyInfos.Where(lobbyInfo => !MenuUtility.Friends.Any(friend => friend.Id == lobbyInfo.OwnerId)) )
        {
            resolvedLobbies.Add(new ResolvedLobby
            {
                Id = lobbyInfo.LobbyId,
                Name = lobbyInfo.Name,
                Game = lobbyInfo.Game,
                Image = "https://i.pinimg.com/originals/ae/24/87/ae24874dd301843548c034a3d2973658.png",
                Map = lobbyInfo.Map,
                Members = lobbyInfo.Members,
                MaxMembers = lobbyInfo.MaxMembers,
                Dev = lobbyInfo.Data["dev"] == "1",
                OwnedByFriend = false
            });

            // TODO update selectedLobby
        }

        // TODO remove/show selected as offline if not listed anymore

        lobbies = resolvedLobbies;

        lastRefresh = Time.Now;
        isRefreshing = false;

        StateHasChanged();
    }

    public override void Tick() // Kinda an ugly way to do this ngl, maybe there is a better way?
    {
        base.Tick();

        if (Time.Now > lastRefresh + refreshDelta && !isRefreshing)
        {
            isRefreshing = true;
            LoadLobbies();
        }
    }

    private void ShowLobbyInfo(ResolvedLobby lobbyInfo)
    {
        Log.Info($"Showing lobby info for {lobbyInfo.Name}");

        selectedLobby = lobbyInfo;
        StateHasChanged();
    }

    private void JoinLobby(ResolvedLobby lobbyInfo)
    {
        Log.Info($"Joining lobby {lobbyInfo.Name}");

        GameNetworkSystem.Connect(lobbyInfo.Id);
    }
}