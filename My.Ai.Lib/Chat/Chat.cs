using System;
using LLama.Common;
using LLama;
using LLama.Abstractions;
using System.Text;
using LLama.Native;

namespace My.Ai.Lib;

public class Chat : IChat<ChatHistory.Message>, IDisposable
{   
    readonly ChatSession _session;
    readonly ChatHistory _chatHistory;
    readonly ModelParams _modelParams;
    readonly LLamaWeights _model;
    readonly LLamaContext _context;
    public Chat(ModelParams modelParams, ChatHistory chatHistory)
    {   
        _modelParams = modelParams;
        _model = modelParams.ToWeights();
        _context = _model.CreateContext(modelParams);
        var executor = new InteractiveExecutor(_context);

        _session = new ChatSession(executor, chatHistory);
        _chatHistory = chatHistory;
    }

    public async IAsyncEnumerable<string> Prompt(ChatHistory.Message message, IInferenceParams inferenceParams)
    {   
        await foreach (var response in _session.ChatAsync(message, inferenceParams))
        {   
            yield return response;
        }
    }

    public ChatHistory History()
    {   
       _chatHistory.Messages = _chatHistory.Messages
        .Select(x => x.AuthorRole != AuthorRole.Assistant ? 
            x : 
            new ChatHistory.Message(x.AuthorRole, x.Content.Replace("\nUser:","").Replace("User:","").Replace("Assistant:","")))
        .ToList();
       return _chatHistory;
    }

    public void Dispose()
    {
        _context.Dispose();
        _model.Dispose();
    }
}

public static class ChatExt
{
    public static ChatHistory ToChatHistory(this List<ChatHistory.Message> messages) => new ChatHistory(messages.ToArray());
    public static ChatHistory.Message ToMessage(this AuthorRole role, string message) => new ChatHistory.Message(role, message);
    public static LLamaWeights ToWeights(this ModelParams modelParams) => LLamaWeights.LoadFromFile(modelParams);
    public static LLamaContext ToContext(this LLamaWeights weights, ModelParams modelParams) =>  weights.CreateContext(modelParams);
    public static Chat ToChat(this ModelParams modelParams, ChatHistory chatHistory) => new Chat(modelParams, chatHistory);
}
