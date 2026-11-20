using SecureForms.Controllers;
using SecureForms.Data;

namespace SecureForms.Views;

public sealed class ConnectionForm:Form
{
    public ConnectionForm(StartupController controller)
    {
        Ui.Style(this,"Kigali City | Connect your database",new Size(1120,790));
        var server=Ui.Text();var port=new NumericUpDown{Minimum=1,Maximum=65535};var database=Ui.Text();var user=Ui.Text();var password=Ui.Text();password.UseSystemPasswordChar=true;
        DatabaseSettings settings;try{settings=DatabaseSettings.Load();}catch{settings=DatabaseSettings.Defaults();}
        server.Text=settings.Server;port.Value=settings.Port;database.Text=settings.Database;user.Text=settings.User;password.Text=settings.Password;
        server.PlaceholderText="127.0.0.1";user.PlaceholderText="Database username";password.PlaceholderText="Your MySQL password";
        var fields=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=4,BackColor=Color.White,Margin=new Padding(0)};
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,58));fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,42));
        for(int i=0;i<3;i++)fields.RowStyles.Add(new RowStyle(SizeType.Absolute,91));fields.RowStyles.Add(new RowStyle(SizeType.Absolute,52));
        var serverField=Ui.Stacked("Server address",server);serverField.Dock=DockStyle.Fill;serverField.Margin=new Padding(0,0,14,0);fields.Controls.Add(serverField,0,0);
        var portField=Ui.Stacked("Port",port);portField.Dock=DockStyle.Fill;fields.Controls.Add(portField,1,0);
        var dbField=Ui.Stacked("Database name",database);dbField.Dock=DockStyle.Fill;fields.Controls.Add(dbField,0,1);fields.SetColumnSpan(dbField,2);
        var accountField=Ui.Stacked("Username",user);accountField.Dock=DockStyle.Fill;accountField.Margin=new Padding(0,0,14,0);fields.Controls.Add(accountField,0,2);
        var passwordField=Ui.Stacked("Password",password);passwordField.Dock=DockStyle.Fill;fields.Controls.Add(passwordField,1,2);
        fields.SetColumnSpan(accountField,1);
        var hint=Ui.Label("Use an existing MySQL service or start MySQL in XAMPP. Your saved records will be kept.",9,false,Ui.Muted);hint.Dock=DockStyle.Fill;hint.AutoSize=false;fields.Controls.Add(hint,0,3);fields.SetColumnSpan(hint,2);
        var connect=Ui.Button("Connect to workspace",()=>{controller.Connect(new DatabaseSettings(server.Text.Trim(),(uint)port.Value,database.Text.Trim(),user.Text.Trim(),password.Text));DialogResult=DialogResult.OK;},true);
        Controls.Add(AccessLayout.Build("01 / DATABASE CONNECTION","Your workspace starts here","Connect your database to open the digital forms workspace.",fields,connect,"Connection details are saved on this computer in your Windows user profile.",660));AcceptButton=connect;
    }
}
