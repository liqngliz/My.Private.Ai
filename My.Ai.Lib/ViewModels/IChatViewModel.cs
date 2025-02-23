using My.Ai.App.Lib.Models;

namespace My.Ai.App.Lib.ViewModels;
public interface IChatViewModel
{
    public Task<History> ChatAsync(History history);   
}
