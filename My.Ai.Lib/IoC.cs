using System.Text.Json;
using System.Text.Json.Nodes;
using Autofac;
using Autofac.Core;
using GlobalExtensions;
using LLama.Common;
using My.Ai.App.Lib.Models;
using My.Ai.App.Lib.ChatModels;

namespace My.Ai.Lib.Container;

public interface IContainer
{
    public Autofac.IContainer Container();
}

public class AIContainer : IContainer
{
    readonly Autofac.IContainer _container;

    public AIContainer(string settingsJson, string baseChatJson, string modelPath)
    {
        var builder = new ContainerBuilder();

        Settings settings = settingsJson;
        settings.ModelPath = modelPath;
        settingsJson = (string)settings;

        History baseChat = baseChatJson.ToChatHistory();

        builder.RegisterInstance(baseChat).Named<History>("baseChat");
        builder.Register<Func<ChatMode, IChatModel>>(c => {
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
    public static Func<ChatMode, IChatModel> ChatViewModelFactory(string settingsJson, Func<string, uint, int, ModelParams> modelParamsFunc, History baseChat) =>
        delegate(ChatMode mode)
        {
            return mode switch
            {
                ChatMode.Persistent => new ChatModelPersistent(settingsJson, modelParamsFunc, baseChat),
                ChatMode.Stateless => new ChatModelStateless(settingsJson, modelParamsFunc),
                _ => throw new ArgumentException("Chat Mode does not exist")
            };
        };
    
    public static Func<ChatMode, IChatModel> ChatViewModelFactory(Settings settingsJson, Func<string, uint, int, ModelParams> modelParamsFunc, History baseChat) =>
        delegate(ChatMode mode)
        {
            return mode switch
            {
                ChatMode.Persistent => new ChatModelPersistent(settingsJson, modelParamsFunc, baseChat),
                ChatMode.Stateless => new ChatModelStateless(settingsJson, modelParamsFunc),
                _ => throw new ArgumentException("Chat Mode does not exist")
            };
        };
}