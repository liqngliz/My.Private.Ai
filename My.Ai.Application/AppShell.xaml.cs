namespace My.Ai.App;

public partial class AppShell : Shell
{
	public AppShell(IServiceProvider service)
	{
		InitializeComponent();
		//Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
		//Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
	}
}
