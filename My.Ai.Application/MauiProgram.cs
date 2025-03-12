using Autofac;
using Microsoft.Extensions.Logging;
using My.Ai.App.Lib.ChatModels;
using My.Ai.App.Lib.Models;
using My.Ai.App.ViewModels;
using My.Ai.Lib.Container;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace My.Ai.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
		var assembly = Assembly.GetExecutingAssembly();
        var manifest = assembly.GetManifestResourceNames();
        

        var appDirectoryPath = FileSystem.AppDataDirectory;
        var files = Directory.GetFiles(appDirectoryPath);
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register App and Shell
        builder.Services.AddSingleton<App>();
        builder.Services.AddSingleton<AppShell>();

        // Register ViewModels and Pages
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<MainPage>();
        
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<SettingsPage>();

		//Register LLM
		//AIContainer
        string settingsFilename = "settings.json";
        string chatTemplateFilename = "Chat.History.Template.json";
        var settingsJsonBase = assembly.ToJsonResource($"My.Ai.App.Resources.Json.{settingsFilename}").Result;
        var chatHistoryJsonBase = assembly.ToJsonResource($"My.Ai.App.Resources.Json.{chatTemplateFilename}").Result;

        if(!settingsFilename.AppDataFileExists())
        {
            settingsJsonBase.AppDataTextToFile(settingsFilename);
        }

        if(!chatTemplateFilename.AppDataFileExists())
        {
            chatHistoryJsonBase.AppDataTextToFile(chatTemplateFilename);
        }

        Settings settings = settingsJsonBase;
        var modelPath = Path.Combine(appDirectoryPath, settings.ModelPath);

        var settingsJson = settingsFilename.AppDataReadTextFile();
        var chatHistoryJson = chatTemplateFilename.AppDataReadTextFile();

		var container = new AIContainer(settingsJson, chatHistoryJson, modelPath).Container();
		var chatViewModelFactory = container.Resolve<Func<ChatMode, IChatModel>>();

		#if DEBUG
        builder.Logging.AddDebug();
		#endif

        return builder.Build();
    }
}

public static class MauiExtension 
{
    public static async Task<string> ToJsonResource(this Assembly assembly, string path)
    {
        using (var stream = assembly.GetManifestResourceStream(path))
        {
            StreamReader reader = new StreamReader(stream);
            string text = await reader.ReadToEndAsync();
            return text;
        }
    }

       public static void AppDataTextToFile(this string text, string targetFileName)  
        {   
           string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, targetFileName);  
           File.WriteAllText(targetFile, text);
       }

       public static string AppDataReadTextFile(this string targetFileName)  
       {  
           string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, targetFileName);  
           return File.ReadAllText(targetFile);
       }

       public static bool AppDataFileExists(this string targetFileName) 
       {
            string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, targetFileName);
            bool exists = File.Exists(targetFile);
            return exists;
       }
}
