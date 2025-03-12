using GlobalExtensions;
using LLama.Common;
using My.Ai.App.Lib.Models;
using My.Ai.Lib;


namespace My.Ai.App.Lib.ChatModels;
public class ChatModelPersistent : IChatModel , IDisposable
{   
    readonly Settings _settings;
    readonly Func<string, uint, int, ModelParams> _toModelParams;
    readonly Chat _chat;

    public ChatModelPersistent(string settingsJson, Func<string, uint, int, ModelParams> toModelParams, History startingInput)
    {
        _settings = settingsJson;
        _toModelParams = toModelParams;
        ModelParams modelParams = _toModelParams(_settings.ModelPath, _settings.ContextSize, _settings.GpuLayerCount);
        _chat = new Chat(modelParams, (ChatHistory)startingInput);
    }

    public ChatModelPersistent(Settings settingsJson, Func<string, uint, int, ModelParams> toModelParams, History startingInput)
    {
        _settings = settingsJson;
        _toModelParams = toModelParams;
        ModelParams modelParams = _toModelParams(_settings.ModelPath, _settings.ContextSize, _settings.GpuLayerCount);
        _chat = new Chat(modelParams, (ChatHistory)startingInput);
    }

    public async Task<History> ChatAsync(History history)
    {
        if(history.Messages.Count < 1) return history;
        var input = history.Messages[history.Messages.Count - 1];
        var messages = new History(new List<Message>());

        for(int i = 0; i < history.Messages.Count - 1; i++)
        {
            messages.Messages.Add(history.Messages[i]);
        }

        var inferenceParams = GlobalExt.DefaultAntiPrompt.ToInferenceParams(_settings.ResponseSize);
        
        await foreach(var text in _chat.Prompt((ChatHistory.Message)input, inferenceParams))
            _ = text;

        return (History)_chat.History();
    }

    public void Dispose()
    {
        _chat.Dispose();
    }
}