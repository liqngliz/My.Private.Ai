using System.Text.Json;

namespace My.Ai.App.Lib.Models;
public struct Settings
{
    public required string ModelPath { get; set; }
    public uint ContextSize { get; set; }
    public int ResponseSize {get; set;}
    public int GpuLayerCount {get; set;}
    public static implicit operator Settings(string json) => JsonSerializer.Deserialize<Settings>(json);
    public static explicit operator string(Settings settings) => JsonSerializer.Serialize(settings);
}

public static class SettingsExt
{
    public static Settings ToSettings(this string FilePath) => JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath));
}