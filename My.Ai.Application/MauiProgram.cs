using Autofac;
using GlobalExtensions;
using Microsoft.Extensions.Logging;
using My.Ai.App.Lib.ChatModels;
using My.Ai.App.Lib.Models;
using My.Ai.App.ViewModels;
using My.Ai.Lib.Container;
using System.Reflection;


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
        
        // Add this code here - right after getting appDirectoryPath
                // Add this code here - right after getting appDirectoryPath
        #if MACCATALYST
        try 
        {
            // Since you mentioned you're using ARM, we'll hardcode it instead of detecting
            var nativePath = Path.Combine(appDirectoryPath, "runtimes", "osx-arm64", "native");
            
            if (!Directory.Exists(nativePath))
                Directory.CreateDirectory(nativePath);
                
            // Set environment variables for library loading
            Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", nativePath, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("DYLD_LIBRARY_PATH", nativePath, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("LLAMACPP_LIB", Path.Combine(nativePath, "libllama.dylib"), EnvironmentVariableTarget.Process);
            
            Console.WriteLine($"Set environment variables for LLama native library at: {nativePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting up native libraries: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
        #endif

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
		Func<ChatMode,IChatModel> chatViewModelFactory = container.Resolve<Func<ChatMode, IChatModel>>();

        History baseHistory = chatHistoryJson.ToChatHistory();
        builder.Services.AddSingleton(chatViewModelFactory);
        builder.Services.AddSingleton(baseHistory);
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
