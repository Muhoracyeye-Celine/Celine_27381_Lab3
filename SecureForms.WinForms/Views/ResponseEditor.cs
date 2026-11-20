using SecureForms.Controllers;
using SecureForms.Models;

namespace SecureForms.Views;

public sealed class ResponseEditor:Form
{
    public ResponseEditor(AppController controller,FormRecord form,ResponseRecord? response,bool preview=false)
    {
        Ui.Style(this,preview?"Kigali City | Form preview":"Kigali City | Complete form",new Size(930,800));
        var questions=controller.Questions(form.Id);var answers=response==null?new Dictionary<int,string>():controller.Answers(response.Id,response.Revision);
        var inputs=new Dictionary<int,Func<string>>();var fields=AccessLayout.Stack();
        if(!string.IsNullOrWhiteSpace(form.Description))
        {
            var instructions=Ui.Text(true);instructions.ReadOnly=true;instructions.Text=form.Description;instructions.ScrollBars=ScrollBars.Vertical;instructions.Height=90;
            AccessLayout.Add(fields,Ui.Stacked("Form instructions",instructions),158);
        }
        int count=0;
        foreach(var q in questions)
        {
            count++;string value=answers.GetValueOrDefault(q.Id,"");Control input;
            if(q.FieldType=="Dropdown")
            {
                var combo=Ui.Choices(new[]{""}.Concat(q.Options.Split('|')));combo.SelectedItem=value;input=combo;inputs[q.Id]=()=>combo.Text;
            }
            else if(q.FieldType=="Checkbox")
            {
                var check=new CheckBox{Text="Yes",Checked=value=="Yes",AutoSize=true,ForeColor=Ui.Teal};input=check;inputs[q.Id]=()=>check.Checked?"Yes":"No";
            }
            else if(q.FieldType=="Date")
            {
                var date=new DateTimePicker{Format=DateTimePickerFormat.Custom,CustomFormat="yyyy-MM-dd",ShowCheckBox=true,Checked=false};if(DateTime.TryParse(value,out var parsed)){date.Value=parsed;date.Checked=true;}input=date;inputs[q.Id]=()=>date.Checked?date.Value.ToString("yyyy-MM-dd"):"";
            }
            else
            {
                var text=Ui.Text(q.FieldType=="Long text");text.Text=value;text.MaxLength=5000;text.PlaceholderText=q.FieldType=="Number"?"Enter a number":"Type your answer...";input=text;inputs[q.Id]=()=>text.Text;
            }
            input.Enabled=!preview;
            if(q.FieldType=="Checkbox")
            {
                var group=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=2,ColumnCount=1,BackColor=Color.White};group.RowStyles.Add(new RowStyle(SizeType.Absolute,32));group.RowStyles.Add(new RowStyle(SizeType.Absolute,36));group.Controls.Add(Ui.Label($"{count:00}  {q.Label}"+(q.Required?" *":""),10,true),0,0);group.Controls.Add(input,0,1);AccessLayout.Add(fields,group,84);
            }
            else AccessLayout.Add(fields,Ui.Stacked($"{count:00}  {q.Label}"+(q.Required?" *":""),input,q.FieldType=="Number"?"Use a dot for decimal values.":""),q.FieldType=="Long text"?153:q.FieldType=="Number"?113:96);
        }
        var scroll=new Panel{Dock=DockStyle.Fill,AutoScroll=true,Padding=new Padding(7,0,14,0),BackColor=Color.White};scroll.Controls.Add(fields);
        var card=Ui.Section(form.Title.Length>65?form.Title[..62]+"...":form.Title,"Fields marked with * are required.",scroll);
        var actions=Ui.Bar(Ui.Button(preview?"Close preview":"Cancel",Close));
        if(!preview)
        {
            void Save(bool draft)
            {
                int id=controller.SaveResponse(form.Id,response?.Id??0,response?.Revision??0,inputs.ToDictionary(p=>p.Key,p=>p.Value()),draft);string reference=controller.Response(id).Reference;
                MessageBox.Show(draft?$"Your draft has been saved.\n\nReference: {reference}":$"Your response has been sent for supervisor review.\n\nReference: {reference}","Response saved");DialogResult=DialogResult.OK;
            }
            actions.Controls.Add(Ui.Button("Save draft",()=>Save(true)));actions.Controls.Add(Ui.Button("Submit response",()=>Save(false),true));
        }
        Controls.Add(Ui.Editor(preview?"Form preview":response==null?"Complete a form":"Update your response",preview?"Check the questions and field layout before collecting responses.":"Provide the requested information. You can save a draft and finish later.",card,actions));
    }
}
