using Microsoft.UI.Xaml;

namespace KnewFate.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        try
        {
            System.IO.File.AppendAllText(System.IO.Path.Combine(System.AppContext.BaseDirectory, "winui_startup.log"), System.DateTime.Now.ToString("O") + " WinUI App ctor before InitializeComponent" + System.Environment.NewLine);
            this.InitializeComponent();
            System.IO.File.AppendAllText(System.IO.Path.Combine(System.AppContext.BaseDirectory, "winui_startup.log"), System.DateTime.Now.ToString("O") + " WinUI App ctor InitializeComponent OK" + System.Environment.NewLine);
        }
        catch (System.Exception ex)
        {
            System.IO.File.AppendAllText(System.IO.Path.Combine(System.AppContext.BaseDirectory, "winui_startup.log"), System.DateTime.Now.ToString("O") + " WinUI App ctor InitializeComponent FAILED: " + ex + System.Environment.NewLine);
        }
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
