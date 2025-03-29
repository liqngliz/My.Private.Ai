using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using My.Ai.App.Utils;

namespace My.Ai.App.ViewModels;

public static class MainPageViewModelExt
{
    public static SavedHistoryViewModel ToSavedHistoryViewModel(this SavedHistory savedHistory, int previewSize, ICommand delete, ICommand select) 
    {   
        var starter = savedHistory.baseChat == null ? 3 : savedHistory.baseChat.Messages.Count;
        bool isNew = savedHistory.history.Messages.Count <= starter;
        var preview = isNew? "" : savedHistory.history.Messages[starter].Content;
        int contentSize = preview.Length;
        if(contentSize > previewSize) preview = preview.Length > previewSize ? preview.Substring(0, previewSize) + "..." : preview;
        return new (preview, savedHistory.history, savedHistory.LastUpdate, delete, select);
    }

    public static ICommand ToSavedHistoryDelete(this SavedHistory savedHistory, Action<string> delete, Func<bool> isEditable, Action<bool> isEditing) =>
        new Command(
            execute: () => {
                isEditing(true);
                delete(savedHistory.Guid);
                isEditing(false);
            },
            canExecute: () => !isEditable()
        );
    
    public static ICommand ToSavedHistorySelect(this SavedHistory savedHistory, Action<string> select, Func<bool> isEditable, Action<bool> isEditing) => 
        new Command(
                execute: () => {
                isEditing(true);
                select(savedHistory.Guid);
                isEditing(false);
            },
            canExecute: () => !isEditable()
        );
}
