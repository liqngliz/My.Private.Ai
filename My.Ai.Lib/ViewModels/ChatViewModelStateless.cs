using GlobalExtensions;
using LLama.Common;
using My.Ai.App.Lib.Models;
using My.Ai.Lib;


namespace My.Ai.App.Lib.ViewModels;
public class ChatViewModelStateless : IChatViewModel
{   
    readonly Settings _settings;
    Func<string, uint, int, ModelParams> _toModelParams;
    public ChatViewModelStateless(string settingsJson, Func<string, uint, int, ModelParams> toModelParams)
    {
        _settings = settingsJson;
        _toModelParams = toModelParams;
    }

    public async Task<History> ChatAsync(History history)
    {
        ModelParams modelParams = _toModelParams(_settings.ModelPath, _settings.ContextSize, _settings.GpuLayerCount);

        if(history.Messages.Count < 2) return history;

        var input = history.Messages[history.Messages.Count - 1];
        var messages = new History(new List<Message>());

        for(int i = 0; i < history.Messages.Count - 1; i++)
        {
            messages.Messages.Add(history.Messages[i]);
        }

        using var chat = new Chat(modelParams, (ChatHistory)messages);
        var inferenceParams = GlobalExt.DefaultAntiPrompt.ToInferenceParams(_settings.ResponseSize);
        await foreach(var text in chat.Prompt((ChatHistory.Message)input, inferenceParams))
            _ = text;

        return (History)chat.History();
    }
}