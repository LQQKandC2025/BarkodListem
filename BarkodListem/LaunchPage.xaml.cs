namespace BarkodListem;

public partial class LaunchPage : ContentPage
{
    private bool _transitionStarted;

    public LaunchPage()
    {
        InitializeComponent();
        launchImage.Scale = 1.035;
        launchCaption.Opacity = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_transitionStarted)
            return;

        _transitionStarted = true;

        var imageAnimation = launchImage.ScaleTo(1.0, 1900, Easing.CubicOut);
        var captionAnimation = launchCaption.FadeTo(1.0, 550, Easing.CubicOut);

        await Task.WhenAll(imageAnimation, captionAnimation, Task.Delay(1900));

        if (Application.Current?.Windows.Count > 0)
            Application.Current.Windows[0].Page = new LoginPage();
    }
}
