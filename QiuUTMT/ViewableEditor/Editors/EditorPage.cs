using UndertaleModLib;

namespace QiuUTMT.ViewableEditor.Editors;

public abstract class EditorPage : ContentPage
{
    public TaskCompletionSource<bool> TaskCompletion { get; set; }
    protected object DataContext { get; set; }

    public EditorPage(object dataContext)
    {
        this.DataContext = dataContext;
        TaskCompletion = new TaskCompletionSource<bool>();
    }

    protected async void OnConfirmClicked(object sender, EventArgs e)
    {
        TaskCompletion.TrySetResult(true);
        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        TaskCompletion.TrySetResult(true);
        return base.OnBackButtonPressed();
    }
}