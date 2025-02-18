using System.Reflection;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLama.Common;
using LLama.Sampling;

namespace GlobalExtensions;

public static class GlobalExt
{
    public static string LlmFolder(this string folder) 
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var dir = Directory.GetDirectories(path);
        bool hasLLM = dir.Any(x => x.ToLower().Contains(folder));
        while(!hasLLM)
        {
            dir = Directory.GetDirectories(path);
            hasLLM = dir.Any(x => x.ToLower().Contains(folder));

            if(!hasLLM)
            {
                var parent = Directory.GetParent(path);
                if(parent == null) throw new NullReferenceException("Llm Folder could not find a parent directory");
                path = parent.FullName;
            }
        }
        
        path = dir.First(x => x.ToLower().Contains(folder));
        return path;
    }

    public static ChatHistory ToChatHistory(this string ChatHistoryJson) 
    {
        ChatHistory? res = JsonSerializer.Deserialize<ChatHistory>(ChatHistoryJson);
        if(res == null) throw new NullReferenceException("No Chat history found");
        return res;
    }
    public static ModelParams ToModelParams(this string ModelPath, uint ContextSize = 128000, int GpuLayerCount = -1) => new ModelParams(ModelPath)
    {
        ContextSize = ContextSize, 
        GpuLayerCount = GpuLayerCount
    };

    public static List<string> DefaultAntiPrompt = new List<string>(){"User:"};
    public static InferenceParams ToInferenceParams(this List<string> AntiPrompts, int MaxTokens = -1) => new InferenceParams(){AntiPrompts = AntiPrompts, MaxTokens = MaxTokens, SamplingPipeline = new DefaultSamplingPipeline()};
}