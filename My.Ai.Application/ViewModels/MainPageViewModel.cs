using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LLama.Native;
using My.Ai.App.Lib.ChatModels;
using My.Ai.App.Lib.Models;
using My.Ai.Lib.Container;


namespace My.Ai.App.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{   
    readonly Func<ChatMode,IChatModel> _chatFactory;
    private IChatModel chatModel;
    private ObservableCollection<ViewMessage> history;

    public ObservableCollection<ViewMessage> History
    {
        get => history;
        set
        {
            SetProperty(ref history, value);
        }
    }
    
    string input;
    public string Input
    {
        set { SetProperty(ref input, value); }
        get { return input; }
    }

    private int _count;

    public int Count
    {
        get => _count;
        set
        {
            if (_count != value)
            {
                _count = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CounterText));
            }
        }
    }

        public string CounterText => Count == 1 ? $"Clicked {Count} time" : $"Clicked {Count} times";
        bool isEditing = false;
        public ICommand IncrementCommand { get; }
        public ICommand Submit {get; }

        public MainPageViewModel(Func<ChatMode,IChatModel> chatFactory, History history)
        {   
            IncrementCommand = new Command(() => Count++);
            Submit = new Command(
                execute:async () => {
                //Save input
                isEditing = true;
                string content = Input;
                Input = string.Empty;
                var message = new Message("User", content);
                History.Add(message);
                var latest = new History(new List<Message>(){message});
                var response = await chatModel.ChatAsync(latest);
                History.Add(response.Messages.Last());
                isEditing = false;
                (Submit as Command).ChangeCanExecute();
            },
            canExecute: ()=> !isEditing);
            _chatFactory = chatFactory;
            //chatModel = _chatFactory(ChatMode.Persistent);
            this.history = new ObservableCollection<ViewMessage>(history.Messages.Select(x=> (ViewMessage)x));
            History = new ObservableCollection<ViewMessage>(history.Messages.Select(x=> (ViewMessage)x));
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

public record ViewMessage(string Role, string Content, LayoutOptions Position)
{
    public static implicit operator ViewMessage(Message message) => 
        new ViewMessage(message.AuthorRole, message.Content, message.AuthorRole.ToLower().Contains("user")? new LayoutOptions() {Alignment = LayoutAlignment.End} : new LayoutOptions() {Alignment = LayoutAlignment.Start} );
};
