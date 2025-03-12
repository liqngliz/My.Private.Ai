namespace My.Ai.App;

public partial class App : Application
{
	public App(IServiceProvider service)
	{
		InitializeComponent();

		MainPage = service.GetService<AppShell>();
	}
}
