using My.Ai.App.Lib.Models;

namespace My.Ai.App.Lib.ChatModels;
public interface IChatModel
{
    public Task<History> ChatAsync(History history);
    public Task<History> GetHistory();
}
