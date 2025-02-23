using System.Text.Json;
using System.Text.Json.Nodes;
using Autofac;
using Autofac.Core;
using GlobalExtensions;
using LLama.Common;
using My.Ai.App.Lib.Models;
using My.Ai.App.Lib.ViewModels;

namespace My.Ai.Lib.Container;

public interface IContainer
{
    public Autofac.IContainer Container();
}

public class AIContainer : IContainer
{
    readonly Autofac.IContainer _container;

    public AIContainer(string settingsPath, string baseChatPath, string fileMetasPath)
    {
        var builder = new ContainerBuilder();

        string settingsJson = File.ReadAllText(settingsPath);
        Settings settings = settingsJson;
        settings.ModelPath = Path.Combine("llm".LlmFolder(), settings.ModelPath);
        settingsJson = (string)settings;
        History baseChat = File.ReadAllText(baseChatPath).ToChatHistory();
        string fileMetasJson = File.ReadAllText(fileMetasPath);
        var fileMetas = new FileMetas(new List<FileMeta>());
        
        try
        {
            FileMetas metas = JsonSerializer.Deserialize<FileMetas>(fileMetasJson);
            if(metas != null)
                fileMetas.Metas.AddRange(metas.Metas);
        } 
        catch(Exception e)
        {
            //create file metas
            File.WriteAllText(fileMetasPath, JsonSerializer.Serialize(fileMetas));
        }

        builder.RegisterInstance(fileMetas).Named<FileMetas>("files");
        builder.RegisterInstance(baseChat).Named<History>("baseChat");
        builder.Register<Func<ChatMode, IChatViewModel>>(c => {
            return ContainerExt.ChatViewModelFactory(settingsJson, GlobalExt.ModelParamsDelegate(), baseChat);
        });

        _container = builder.Build();
    }

    public Autofac.IContainer Container() => _container;

}    

public enum ChatMode
{
    Persistent,
    Stateless
}

public static class ContainerExt
{
    public static Func<ChatMode, IChatViewModel> ChatViewModelFactory(string settingsJson, Func<string, uint, int, ModelParams> modelParamsFunc, History baseChat) =>
        delegate(ChatMode mode)
        {
            return mode switch
            {
                ChatMode.Persistent => new ChatViewModelPersistent(settingsJson, modelParamsFunc, baseChat),
                ChatMode.Stateless => new ChatViewModelStateless(settingsJson, modelParamsFunc),
                _ => throw new ArgumentException("Chat Mode does not exist")
            };
        };
}