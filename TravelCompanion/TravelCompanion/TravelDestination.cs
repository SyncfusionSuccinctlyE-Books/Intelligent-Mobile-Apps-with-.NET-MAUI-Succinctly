using Azure.Search.Documents.Indexes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class TravelDestination : INotifyPropertyChanged
{
    private string _id;
    private string _name;
    private string _description;
    private double _rating;

    [SearchableField(IsKey = true)]
    public string Id
    {
        get => _id;
        set
        {
            if (_id != value)
            {
                _id = value;
                OnPropertyChanged();
            }
        }
    }

    [SearchableField]
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    [SearchableField]
    public string Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged();
            }
        }
    }

    [SimpleField(IsFilterable = true, IsSortable = true)]
    public double Rating
    {
        get => _rating;
        set
        {
            if (_rating != value)
            {
                _rating = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? 
        PropertyChanged;

    protected virtual void OnPropertyChanged(
        [CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, 
            new PropertyChangedEventArgs(propertyName));
    }
}