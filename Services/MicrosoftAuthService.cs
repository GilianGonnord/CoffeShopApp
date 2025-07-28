namespace CoffeeShopApp.Services;

public class MicrosoftAuthService : IMicrosoftAuthService
{
    public bool IsEnabled { get; }

    public MicrosoftAuthService(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}