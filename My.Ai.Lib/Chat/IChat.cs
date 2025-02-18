using LLama.Abstractions;
using LLama.Common;

namespace My.Ai.Lib;

public interface IChat <T>
{   
    public IAsyncEnumerable<string> Prompt(T input, IInferenceParams inferenceParams);
    public ChatHistory History();
}

