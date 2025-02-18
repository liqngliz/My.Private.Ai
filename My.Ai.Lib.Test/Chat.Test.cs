using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GlobalExtensions;
using LLama;
using LLama.Abstractions;
using LLama.Common;
using LLama.Sampling;
namespace My.Ai.Lib.Test;

public class ChatTest
{   
    readonly ChatHistory _chatHistory;
    readonly InferenceParams _inferenceParams;
    readonly ModelParams _modelParams;

    public ChatTest()
    {
        var llmFolder = "llm".LlmFolder();
        var modelPath = Path.Combine(llmFolder, "DeepSeek-R1-Distill-Qwen-1.5B-Q8_0.gguf");
        _modelParams = modelPath.ToModelParams(80000);
        _chatHistory = File.ReadAllText("Chat.History.Template.Json").ToChatHistory();
        _inferenceParams = GlobalExt.DefaultAntiPrompt.ToInferenceParams(2048);
    }

    [Fact]
    public async Task Should_Respond_To_Prompt()
    {
     
        var actual = new StringBuilder();
        using var sut = new Chat(_modelParams, _chatHistory);
        var input = """can you repeat exact words: 'I am an AI model made to deliver value to people'""";
        await foreach(var text in sut.Prompt(AuthorRole.User.ToMessage(input), _inferenceParams))
        {
            actual.Append(text);
        }
        var res = actual.ToString();
        Assert.True(!string.IsNullOrEmpty(res));
    }

    [Fact]
    public async Task Should_Return_Chat_History()
    {
        var chat = new StringBuilder();
        using var sut = new Chat(_modelParams, _chatHistory);
        var input = """can you repeat exact words: 'I am an AI model made to deliver value to people'""";
        await foreach(var text in sut.Prompt(AuthorRole.User.ToMessage(input), _inferenceParams))
        {
            chat.Append(text);
        }
        var res = chat.ToString();
        var actual = sut.History();
        var last = actual.Messages.Last();
        Assert.True(!string.IsNullOrEmpty(last.Content));
        
        await foreach(var text in sut.Prompt(AuthorRole.User.ToMessage("tell me a funnny joke that contains the word 'cat'"), _inferenceParams))
        {
            chat.Append(text);
        }

        actual = sut.History();
        last = actual.Messages.Last();
        Assert.Contains("cat", last.Content.ToLower());
    }

}