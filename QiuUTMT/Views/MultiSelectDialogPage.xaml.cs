using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QiuUTMT;
public partial class MultiSelectDialogPage : ContentPage
{
    public ObservableCollection<Item> Items { get; set; }
    public TaskCompletionSource<bool> TaskCompletion { get; set; }
    public MultiSelectDialogPage(ObservableCollection<Item> items)
    {
        InitializeComponent();
        TaskCompletion=new TaskCompletionSource<bool>();
        Items = items;
        ItemsCollectionView.ItemsSource = Items;
        BindingContext = this;
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        TaskCompletion.TrySetResult(true);
        await Navigation.PopModalAsync();
    }

    public partial class Item
    {
        public string Name { get; set; }
        public bool Checked { get; set; }
    }

    protected override bool OnBackButtonPressed()
    {
        TaskCompletion.TrySetResult(true);
        return base.OnBackButtonPressed();
    }
}