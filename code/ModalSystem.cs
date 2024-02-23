using Menu.Modals;
using Sandbox;
using Sandbox.Modals;
using System;

public class ModalSystem : IModalSystem
{
	List<BaseModal> OpenModals = new List<BaseModal>();

	public void CloseAll()
	{
		foreach ( var modal in OpenModals )
		{
			modal.Delete();
		}

		OpenModals = new();
	}

	protected void Push( BaseModal modal )
	{
		MenuOverlay.Instance.AddChild( modal );
		modal.OnClosed += ( s ) => OnModalClosing( modal, s );
		OpenModals.Add( modal );
	}

	void OnModalClosing( BaseModal modal, bool success )
	{
		modal.Delete();
		OpenModals.Remove( modal );
	}

	public void Package( string packageIdent )
	{
		var modal = new PackageModal();
		modal.PackageIdent = packageIdent;

		Push( modal );
	}

	public void PackageSelect( string query, Action<Package> onPackageSelected, Action<string> onFilterChanged )
	{
		var modal = new PackageSelectionModal();
		modal.PackageQuery = query;
		modal.OnPackageSelected = onPackageSelected;
		modal.OnFilterChanged = onFilterChanged;

		Push( modal );
	}

	public void Organization( Package.Organization org )
	{
		var modal = new OrganizationModal();
		modal.Org = org;

		Push( modal );
	}

	public void FriendsList( FriendsListModalOptions config )
	{
		var modal = new FriendsListModal( config );
		Push( modal );
	}

	public void Launcher( string packageIdent )
	{
		var modal = new CreateGameModal( packageIdent );
		Push( modal );
	}

	public void ServerList( in ServerListConfig config )
	{
		var modal = new ServerListModal( config );
		Push( modal );
	}

	public void Binds()
	{
		var modal = new BindsModal();
		Push( modal );
	}
}
