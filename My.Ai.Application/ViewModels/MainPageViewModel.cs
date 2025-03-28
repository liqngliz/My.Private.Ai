using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LLama.Native;
using My.Ai.App.Lib.ChatModels;
using My.Ai.App.Lib.Models;
using My.Ai.App.Utils;
using My.Ai.Lib.Container;


namespace My.Ai.App.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{   
    readonly Func<ChatMode, History, IChatModel> _chatFactory;
    readonly IHistoryReposity _historyRepository;
    private string chatGuid = string.Empty;
    private History baseChat;
    private IChatModel chatModel;

    //observable items for command pattern
    private ObservableCollection<MessageViewModel> history;
    public ObservableCollection<MessageViewModel> History
    {
        get => history;
        set {SetProperty(ref history, value);}
    }
    
    private string input;
    public string Input
    {
        set { SetProperty(ref input, value); }
        get => input;
    }

    private bool isEditing;
    public bool Editable 
    {
        get => !isEditing; 
    }

    private ObservableCollection<SavedHistoryViewModel> savedHistories = new ObservableCollection<SavedHistoryViewModel>();
    public ObservableCollection<SavedHistoryViewModel> SavedHistories
    {
        set{ SetProperty(ref savedHistories, value); }
        get => savedHistories;
    }
    
    //Commands
    public ICommand Submit {get; }
    public ICommand ClearChat{get; }
    public ICommand NewChat{get; }

    public MainPageViewModel(Func<ChatMode, History, IChatModel> chatFactory, IHistoryReposity historyReposity, History history)
    {   
        _chatFactory = chatFactory;
        _historyRepository = historyReposity;
        Submit = new Command(
            execute:async () => {
            isEditing = true;
            OnPropertyChanged(nameof(Editable));
            string message = Input;
            Input = string.Empty;

            await submit(message, History, chatModel);
            
            isEditing = false;
            OnPropertyChanged(nameof(Editable));
            (Submit as Command).ChangeCanExecute();
        },
        canExecute: ()=> !isEditing);

        ClearChat = new Command(
            execute: () => 
            {
                isEditing = true;
                clearChat(out chatModel, _chatFactory, ChatMode.Persistent, baseChat);
                History.Clear();
                isEditing = false;
                (ClearChat as Command).ChangeCanExecute();
            },
            canExecute: ()=> !isEditing
        );


        NewChat = new Command(
            execute: ()=>
            {   
                isEditing = true;
                saveChat(chatGuid, chatModel.GetHistory().Result, baseChat,_historyRepository);
                clearChat(out chatModel, _chatFactory, ChatMode.Persistent, baseChat);

                SavedHistories = new ObservableCollection<SavedHistoryViewModel>(
                        _historyRepository.GetHistories().Select(x => (SavedHistoryViewModel)x)
                        );

                History.Clear();
                chatGuid = string.Empty;
                isEditing = false;
                (NewChat as Command).ChangeCanExecute();
            },
            canExecute: ()=> !isEditing
        );

        baseChat = history;
        chatModel = _chatFactory(ChatMode.Persistent, baseChat);
        this.history = new ObservableCollection<MessageViewModel>();
        History = new ObservableCollection<MessageViewModel>();

        var saves = _historyRepository.GetHistories().Select(x => (SavedHistoryViewModel)x);
        SavedHistories = saves.Count() > 0 ? 
            new ObservableCollection<SavedHistoryViewModel>(saves) : 
            new ObservableCollection<SavedHistoryViewModel>();
    }


    private async Task submit(string message, ObservableCollection<MessageViewModel> history, IChatModel chatModel)
    {
        var msg = new Message("User", message);
        history.Add(msg);
        var latest = new History(new List<Message>(){msg});
        var response = await chatModel.ChatAsync(latest);
        history.Add(response.Messages.Last());
    }

    private void clearChat(out IChatModel model, Func<ChatMode, History, IChatModel> factory, ChatMode chatMode, History history)
    {
        model = factory(chatMode, history);
    }

    private void saveChat(string id, History history, History baseChat,IHistoryReposity historyReposity)
    {
        var guid = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
        var hist = historyReposity.GetHistory(guid);
        var saveItem = hist == null? 
            new SavedHistory(guid, history, baseChat, DateTime.Now, DateTime.Now) :
            hist with { history = history, LastUpdate = DateTime.Now };
        
        historyReposity.Save(saveItem);
    }

    #region INotifyPropertyChanged Implementation
    public event PropertyChangedEventHandler PropertyChanged;
    bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (Object.Equals(storage, value))
            return false;

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}

public record MessageViewModel(string Role, string Content, int Position)
{
    public static implicit operator MessageViewModel(Message message) => 
        new MessageViewModel(message.AuthorRole, message.Content, message.AuthorRole.ToLower().Contains("user")? 3:0);
};

public record SavedHistoryViewModel(string preview, History history, DateTime LastUpdate)
{
    public static implicit operator SavedHistoryViewModel(SavedHistory savedHistory) 
    {   
        var starter = savedHistory.baseChat == null ? 3 : savedHistory.baseChat.Messages.Count;
        bool isNew = savedHistory.history.Messages.Count <= starter;
        var preview = isNew? "" : savedHistory.history.Messages[starter].Content;
        int contentSize = preview.Length;
        if(contentSize > 12) preview = preview.Length > 12 ? preview.Substring(0, 12) + "..." : preview;
        return new (preview, savedHistory.history, savedHistory.LastUpdate);
    }
}