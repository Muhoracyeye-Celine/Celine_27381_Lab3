using SecureForms.Controllers;
using SecureForms.Views;

namespace SecureForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var startup=new StartupController();
        using(var setup=new ConnectionForm(startup))
            if(setup.ShowDialog()!=DialogResult.OK)return;
        while(true)
        {
            using var login=new LoginForm(startup);
            if(login.ShowDialog()!=DialogResult.OK||login.Controller==null)return;
            using var main=new MainForm(login.Controller);
            main.ShowDialog();
            if(!main.SignOutRequested)return;
        }
    }
}
