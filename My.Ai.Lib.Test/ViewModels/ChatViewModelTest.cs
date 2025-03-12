using GlobalExtensions;
using LLama.Common;
using My.Ai.App.Lib.Models;
using My.Ai.App.Lib.ChatModels;

namespace My.Ai.Lib.Test.ViewModels;

[Collection("Sequential")]
public class ChatViewModelTest
{
    readonly string _settings;
    public ChatViewModelTest()
    {
        var settings = @"{
  ""ModelPath"": ""DeepSeek-R1-Distill-Qwen-7B-Q5_K_M.gguf"",
  ""ContextSize"": 2048,
  ""ResponseSize"": 256,
  ""GpuLayerCount"": -1
}"; 

        var path = Path.Combine("llm".LlmFolder(), "DeepSeek-R1-Distill-Qwen-7B-Q5_K_M.gguf");
        settings = settings.Replace("DeepSeek-R1-Distill-Qwen-7B-Q5_K_M.gguf", path);
        _settings = settings;
    }
    [Fact]
    public void Should_Chat_Stateless()
    {
        var func = GlobalExt.ModelParamsDelegate();
        IChatModel sut = new ChatModelStateless(_settings, func);
        History chatHistory = File.ReadAllText("Chat.History.Template.Json").ToChatHistory();
        chatHistory.Messages.Add(new (AuthorRole.User.ConvertToString(), "Are you DeepSeek?"));
        var actual = sut.ChatAsync(chatHistory).Result;
        Assert.True(!string.IsNullOrEmpty(actual.Messages.Last().Content));
        Assert.Equal(5, actual.Messages.Count);
    }

    [Fact]
    public void Should_Chat_Persistent()
    {
        var func = GlobalExt.ModelParamsDelegate();
        History chatHistory = File.ReadAllText("Chat.History.Template.Json").ToChatHistory();
        IChatModel sut = new ChatModelPersistent(_settings, func, chatHistory);
        var messages = new List<Message>(){new ("User", "Are you from china?")};
        var actual = sut.ChatAsync(messages).Result;
        Assert.True(!string.IsNullOrEmpty(actual.Messages.Last().Content));
        Assert.Equal(5, actual.Messages.Count);
    }
    
    
}