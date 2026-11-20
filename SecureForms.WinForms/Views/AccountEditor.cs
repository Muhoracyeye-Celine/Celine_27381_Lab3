using SecureForms.Controllers;
using SecureForms.Models;

namespace SecureForms.Views;

public sealed class AccountEditor:Form
{
    public AccountEditor(AppController controller,Account? account)
    {
        Ui.Style(this,"Kigali City | User account",new Size(840,750));
        var fields=Ui.Fields();var name=Ui.Text();var email=Ui.Text();var role=Ui.Choices(Roles.All);var department=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,DisplayMember="Name",ValueMember="Id"};department.DataSource=controller.Departments();
        var active=new CheckBox{Text="Allow this person to sign in",Checked=true,AutoSize=true,ForeColor=Ui.Teal};var password=Ui.Text();password.UseSystemPasswordChar=true;password.MaxLength=128;
        name.PlaceholderText="Full name";email.PlaceholderText="name@example.com";password.PlaceholderText=account==null?"At least 8 characters":"Leave empty to keep current password";
        if(account!=null){name.Text=account.Name;email.Text=account.Email;role.SelectedItem=account.Role;department.SelectedValue=account.DepartmentId;active.Checked=account.Active;}
        Ui.Field(fields,"Full name",name);Ui.Field(fields,"Email address",email);Ui.Field(fields,"User role",role);Ui.Field(fields,"Department",department);Ui.Field(fields,account==null?"Password":"New password",password);Ui.Field(fields,"Account status",active);
        var scroll=new Panel{Dock=DockStyle.Fill,AutoScroll=true,BackColor=Color.White};scroll.Controls.Add(fields);
        var card=Ui.Section("Profile and permissions","Give each person the access they need for their work.",scroll);
        var footer=Ui.Bar(Ui.Button("Cancel",Close),Ui.Button("Save account",()=>{controller.SaveAccount(account?.Id??0,name.Text,email.Text,role.Text,(department.SelectedItem as Department)?.Id??0,active.Checked,password.Text);DialogResult=DialogResult.OK;},true));
        Controls.Add(Ui.Editor(account==null?"Add a team member":"Edit team member","Manage identity, department membership and account access.",card,footer));
    }
}
