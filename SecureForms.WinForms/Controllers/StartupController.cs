using SecureForms.Data;

namespace SecureForms.Controllers;

public sealed class StartupController
{
    public Store Store { get; private set; } = null!;
    public void Connect(DatabaseSettings settings)
    {
        var database=new Database(settings);database.Initialize();
        settings.Save();Store=new Store(database);
    }
    public bool NeedsAdmin => !Store.HasAccounts();
    public void CreateAdmin(string name,string email,string password)=>AppController.FirstAdmin(Store,name,email,password);
    public AppController Login(string email,string password)=>new(Store,AppController.Login(Store,email,password));
}
