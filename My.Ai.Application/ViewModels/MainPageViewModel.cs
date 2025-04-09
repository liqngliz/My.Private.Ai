using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LLama.Native;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
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
        set {SetProperty(ref history, value, nameof(History));}
    }
    
    private string input;
    public string Input
    {
        set { SetProperty(ref input, value, nameof(Input)); }
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
        set{ SetProperty(ref savedHistories, value, nameof(SavedHistories)); }
        get => savedHistories;
    }
    
    //Commands
    public ICommand Submit {get;}
    public ICommand ClearChat {get;}
    public ICommand NewChat {get;}
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
            
            chatGuid = saveChat(chatGuid, chatModel.GetHistory().Result, baseChat,_historyRepository);
            refreshSave().Invoke();
            isEditing = false;
            OnPropertyChanged(nameof(Editable));
            (Submit as Command).ChangeCanExecute();
        },
        canExecute: ()=> !isEditing);

        ClearChat = new Command(
            execute: () => 
            {
                isEditing = true;
                makeChat(out chatModel, _chatFactory, ChatMode.Persistent, baseChat);
                History.Clear();
                saveChat(chatGuid, chatModel.GetHistory().Result, baseChat,_historyRepository);
                refreshSave().Invoke();
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
                makeChat(out chatModel, _chatFactory, ChatMode.Persistent, baseChat);
                refreshSave().Invoke();
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
        refreshSave().Invoke();
    }


    private async Task submit(string message, ObservableCollection<MessageViewModel> history, IChatModel chatModel)
    {
        var msg = new Message("User", message);
        history.Add(msg);
        var latest = new History(new List<Message>(){msg});
        var response = await chatModel.ChatAsync(latest);
        history.Add(response.Messages.Last());
    }

    private void makeChat(out IChatModel model, Func<ChatMode, History, IChatModel> factory, ChatMode chatMode, History history)
    {
        model = factory(chatMode, history);
    }

    private Action<string> deleteChat(Action refreshSaves) => delegate(string guid)
    {
        _historyRepository.Delete(guid);
        refreshSaves();
        if(chatGuid == guid)
        {
            makeChat(out chatModel, _chatFactory, ChatMode.Persistent, baseChat);
            History.Clear();
            chatGuid = string.Empty;
        }
    };
    
    private Action refreshSave() => delegate
    {
        var histories = _historyRepository.GetHistories();
        var saves = histories.Select(x => x.ToSavedHistoryViewModel(
                            20,
                            x.ToSavedHistoryDelete(deleteChat(refreshSave()), isEditable(), setEditable()),
                            x.ToSavedHistorySelect(selectChat(), isEditable(), setEditable())
                            ));
        SavedHistories = saves.Count() > 0 ? 
            new ObservableCollection<SavedHistoryViewModel>(saves) : 
            new ObservableCollection<SavedHistoryViewModel>();
    };

    private Action<bool> setEditable() => delegate(bool editable)
    {
        isEditing = editable;
        OnPropertyChanged(nameof(Editable));
    };

    private Func<bool> isEditable() => delegate 
    {
        return isEditing;
    };

    private Action <string> selectChat() 
    {
        return delegate(string guid)
        {   
            if(chatGuid == guid || string.IsNullOrEmpty(guid)) return;
            
            var chatHistory = _historyRepository.GetHistory(guid);
            if(chatHistory == null) return;

            if(!string.IsNullOrEmpty(chatGuid))
                saveChat(chatGuid, chatModel.GetHistory().Result, baseChat,_historyRepository);
            
            chatGuid = guid;
            baseChat = chatHistory.baseChat;
            
            makeChat(out chatModel, _chatFactory, ChatMode.Persistent, chatHistory.history);

            var basechatSize = baseChat.Messages.Count;
            var diplayedHistory = chatHistory.history.Messages.Select(x => (MessageViewModel)x);

            History = new ObservableCollection<MessageViewModel>();

            var count = 0;
            foreach(var his in diplayedHistory)
            {   
                count++;
                if(count <= basechatSize) continue;
                History.Add(his);
            }
        };
    }

    private string saveChat(string id, History history, History baseChat,IHistoryReposity historyReposity)
    {
        var guid = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
        var hist = historyReposity.GetHistory(guid);
        var saveItem = hist == null? 
            new SavedHistory(guid, history, baseChat, DateTime.Now, DateTime.Now) :
            hist with { history = history, LastUpdate = DateTime.Now };
        
        historyReposity.Save(saveItem);
        return guid;
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

public record SavedHistoryViewModel(string preview, History history, DateTime LastUpdate, ICommand Delete, ICommand Select);
