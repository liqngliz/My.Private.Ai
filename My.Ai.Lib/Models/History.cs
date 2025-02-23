using LLama.Common;

namespace My.Ai.App.Lib.Models;

public record Message(string AuthorRole, string Content)
{
    public static explicit operator ChatHistory.Message(Message message) => 
        new ChatHistory.Message(message.AuthorRole.ConverToEnum<AuthorRole>(), message.Content);
    public static implicit operator Message(ChatHistory.Message message) => 
        new Message(message.AuthorRole.ConvertToString(), message.Content);
};

public record History(List<Message> Messages)
{
    public static implicit operator History(ChatHistory history) => 
        new History(history.Messages.Select(x=>(Message)x).ToList());
    public static explicit operator ChatHistory(History history) => 
        new ChatHistory(history.Messages.Select(x=>(ChatHistory.Message)x).ToArray());

    public static implicit operator History(List<Message> messages) => new History(messages);
    public static implicit operator List<Message>(History history) => history.Messages;
};

public static class HistoryExt
{
    public static string ConvertToString(this Enum eff)
    {   
        var res = Enum.GetName(eff.GetType(), eff);
        return res;
    }

    public static EnumType ConverToEnum<EnumType>(this String enumValue)  
    {
        return (EnumType) Enum.Parse(typeof(EnumType), enumValue);
    }
}