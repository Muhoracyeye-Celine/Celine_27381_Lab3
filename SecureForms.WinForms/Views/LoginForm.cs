using SecureForms.Controllers;

namespace SecureForms.Views;

public sealed class LoginForm:Form
{
    public AppController? Controller{get;private set;}
    public LoginForm(StartupController startup)
    {
        Ui.Style(this,"Kigali City | Secure Digital Forms",new Size(1120,790));
        bool first=startup.NeedsAdmin;var fields=AccessLayout.Stack();
        var name=Ui.Text();name.PlaceholderText="Your full name";
        var email=Ui.Text();email.PlaceholderText="you@example.com";
        var password=Ui.Text();password.UseSystemPasswordChar=true;password.MaxLength=128;password.PlaceholderText=first?"At least 8 characters":"Enter your password";
        if(first)AccessLayout.Add(fields,Ui.Stacked("Full name",name));
        AccessLayout.Add(fields,Ui.Stacked("Email address",email));AccessLayout.Add(fields,Ui.Stacked("Password",password));
        var show=new CheckBox{Text="Show password",AutoSize=true,ForeColor=Ui.Muted,Font=new Font("Segoe UI",9),Margin=new Padding(0)};
        show.CheckedChanged+=(_,_)=>password.UseSystemPasswordChar=!show.Checked;AccessLayout.Add(fields,show,32);
        var submit=Ui.Button(first?"Create administrator account":"Sign in to workspace",()=>{
            if(first)startup.CreateAdmin(name.Text,email.Text,password.Text);
            Controller=startup.Login(email.Text,password.Text);DialogResult=DialogResult.OK;
        },true);
        Controls.Add(AccessLayout.Build(first?"02 / ADMINISTRATOR SETUP":"YOUR DEPARTMENT WORKSPACE",first?"Make it your workspace":"Welcome back",first?"Create the first administrator account to manage your people, departments and forms.":"Sign in to continue to your forms, submissions and departmental reviews.",fields,submit,first?"You can add Department Admin, Supervisor, Staff and Viewer accounts after setup.":"Access is managed by your system administrator.",first?680:590));AcceptButton=submit;
    }
}
