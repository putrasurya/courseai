using System.ComponentModel;

namespace PathWeaver.Services;

public class ProcessingStatusService : INotifyPropertyChanged
{
    private bool _isProcessing;
    private string _statusMessage = string.Empty;

    public bool IsProcessing
    {
        get => _isProcessing;
        private set
        {
            if (_isProcessing != value)
            {
                _isProcessing = value;
                OnPropertyChanged(nameof(IsProcessing));
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
    }

    public void SetProcessing(string message)
    {
        StatusMessage = message;
        IsProcessing = true;
    }

    public void ClearProcessing()
    {
        IsProcessing = false;
        StatusMessage = string.Empty;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}