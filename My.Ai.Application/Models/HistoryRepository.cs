using System.Text.Json;
using My.Ai.App.Lib.Models;


namespace My.Ai.App.Utils;

public record SavedHistory(string Guid, History history, History baseChat,DateTime CreatedDate, DateTime LastUpdate);

public interface IHistoryReposity
{
    public IEnumerable<SavedHistory> GetHistories();
    public void Save(SavedHistory history);
    public void Delete(string Guid);

    public SavedHistory? GetHistory(string Guid);
}

public class HistoryRepository : IHistoryReposity
{   
    readonly string _HistoryFolderPath;
    public HistoryRepository()
    {
        var appDirectoryPath = FileSystem.AppDataDirectory;
        var folderName = "ChatHistories";
        _HistoryFolderPath = Path.Combine(appDirectoryPath, folderName);
        var dir = Directory.GetDirectories(appDirectoryPath);

        if(!dir.Contains(folderName))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(appDirectoryPath);
            directoryInfo.CreateSubdirectory(folderName);
        }
        //dir = Directory.GetDirectories(appDirectoryPath);
        //var files = Directory.GetFiles(appDirectoryPath);

    }

    public IEnumerable<SavedHistory> GetHistories() => getHistories().OrderByDescending(x => x.CreatedDate);
    

    public void Delete(string Guid)
    {
        var chats = getHistories();
        bool exists = chats.Where(x => x.Guid == Guid).Count() > 0;
        if(exists)
            File.Delete(Path.Combine(_HistoryFolderPath, Guid + ".json"));
    }

    public void Save(SavedHistory history)
    {   
        var path = Path.Combine(_HistoryFolderPath, history.Guid + ".json");
        var json = JsonSerializer.Serialize(history);
        File.WriteAllText(path, json);
    }

    private IEnumerable<SavedHistory> getHistories()
    {
        var files = Directory.GetFiles(_HistoryFolderPath);
        var fileContent = files.Select(x => File.ReadAllText(x));
        var res = fileContent.Select(x => JsonSerializer.Deserialize<SavedHistory>(x));
        return res;
    }

    public SavedHistory? GetHistory(string Guid)
    {
        var files = Directory.GetFiles(_HistoryFolderPath);
        var fileContent = files.Select(x => File.ReadAllText(x));
        var res = fileContent.Select(x => JsonSerializer.Deserialize<SavedHistory>(x));
        if(res == null)
            return null;
        
        var history = res.Where(x => x!=null && !string.IsNullOrEmpty(x.Guid) && x.Guid == Guid).FirstOrDefault();

        return history;
    }
}
