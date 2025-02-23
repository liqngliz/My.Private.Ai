using LLama.Common;
using My.Ai.App.Lib.Models;

namespace My.Ai.Lib.Test.Models;

[Collection("Sequential")]
public class HistoryTest
{
    [Fact]
    public void Should_Cast_To_Message()
    {
        // Given
        ChatHistory.Message msg = new ChatHistory.Message(AuthorRole.System, "System input");
        // When
        var sut = (Message)msg;
        // Then
        Assert.Equal("System", sut.AuthorRole);
        Assert.Equal("System input", sut.Content);
    }

    [Fact]
    public void Should_Cast_To_ChatMessage()
    {
        // Given
        var msg = new Message("Assistant", "Content");
        // When
        var sut = (ChatHistory.Message)msg;
        // Then
        Assert.Equal(AuthorRole.Assistant, sut.AuthorRole);
        Assert.Equal("Content", sut.Content);
    }

    [Fact]
    public void Should_Cast_To_History()
    {
        // Given
        ChatHistory chatHistory = new ChatHistory();
        chatHistory.AddMessage(AuthorRole.System, "System input");
        chatHistory.AddMessage(AuthorRole.Assistant, "Content");
        // When
        var sut = (History)chatHistory;
        // Then
        Assert.Equivalent(new List<Message>{new("System", "System input"), new("Assistant", "Content")}, sut.Messages);
    }

    [Fact]
    public void Should_Cast_To_ChatHistory()
    {
        // Given
        var history = new History(new List<Message>{new("System", "System input"), new("Assistant", "Content")});
        // When
        var sut = (ChatHistory)history;
        // Then
        var expected = new ChatHistory();
        expected.AddMessage(AuthorRole.System, "System input");
        expected.AddMessage(AuthorRole.Assistant, "Content");
        Assert.Equivalent(expected, sut);
    }

}
