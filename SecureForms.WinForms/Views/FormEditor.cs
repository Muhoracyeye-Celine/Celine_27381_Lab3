using SecureForms.Controllers;
using SecureForms.Models;

namespace SecureForms.Views;

public sealed class FormEditor:Form
{
    private int formId;
    private int questionId;
    public FormEditor(AppController controller,FormRecord? form)
    {
        formId=form?.Id??0;Ui.Style(this,"Kigali City | Form builder",new Size(1230,830));MinimumSize=new Size(1050,700);
        var title=Ui.Text();title.Text=form?.Title??"";title.PlaceholderText="For example, Department Service Request";
        var instructions=Ui.Text(true);instructions.Text=form?.Description??"";instructions.PlaceholderText="Tell staff what information they should provide.";
        var grid=Ui.Grid();Ui.Column(grid,"Position","Order",45);Ui.Column(grid,"Label","Question",180);Ui.Column(grid,"FieldType","Type",90);Ui.Column(grid,"Required","Required",65);
        var label=Ui.Text();label.PlaceholderText="What would you like to ask?";var type=Ui.Choices(FieldTypes.All);var options=Ui.Text();options.PlaceholderText="Gasabo|Kicukiro|Nyarugenge";
        var required=new CheckBox{Text="This question requires an answer",AutoSize=true,ForeColor=Ui.Teal};var position=new NumericUpDown{Minimum=1,Maximum=1000,Value=1};bool editable=form==null||form.Status=="Draft";
        var editing=Ui.Label("NEW QUESTION",8,true,Ui.Teal);
        void Load(){if(formId>0)grid.DataSource=controller.Questions(formId);}
        void Clear(){questionId=0;label.Clear();options.Clear();required.Checked=false;position.Value=Math.Min(1000,grid.Rows.Count+1);type.SelectedIndex=0;editing.Text="NEW QUESTION";grid.ClearSelection();}
        void SaveForm(){formId=controller.SaveForm(formId,title.Text,instructions.Text);Load();}
        void SelectQuestion()
        {
            if(grid.CurrentRow?.DataBoundItem is Question q){questionId=q.Id;label.Text=q.Label;type.SelectedItem=q.FieldType;options.Text=q.Options;required.Checked=q.Required;position.Value=q.Position;editing.Text="EDIT QUESTION "+q.Position;}
        }
        grid.CellClick+=(_,e)=>{if(e.RowIndex>=0)SelectQuestion();};grid.KeyUp+=(_,_)=>SelectQuestion();
        var left=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,Margin=new Padding(0)};left.RowStyles.Add(new RowStyle(SizeType.Absolute,245));left.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var meta=Ui.Fields();Ui.Field(meta,"Form title",title);Ui.Field(meta,"Instructions",instructions);
        var metaCard=Ui.Section("01  Form details","A clear title and helpful instructions.",meta);metaCard.Margin=new Padding(0,0,0,16);left.Controls.Add(metaCard,0,0);
        var listCard=Ui.Section("02  Your questions","Select a row to edit it in the question panel.",Ui.EmptyGrid(grid,"Build your first question","Use the panel on the right."));listCard.Margin=new Padding(0);left.Controls.Add(listCard,0,1);
        var questionStack=AccessLayout.Stack();AccessLayout.Add(questionStack,editing,31);AccessLayout.Add(questionStack,Ui.Stacked("Question label",label));AccessLayout.Add(questionStack,Ui.Stacked("Field type",type));
        var optionField=Ui.Stacked("Dropdown choices",options,"Separate each choice with a vertical bar |.");AccessLayout.Add(questionStack,optionField,112);AccessLayout.Add(questionStack,Ui.Stacked("Display position",position));AccessLayout.Add(questionStack,required,40);
        type.SelectedIndexChanged+=(_,_)=>{options.Enabled=type.Text=="Dropdown";};options.Enabled=false;
        var saveQuestion=Ui.Button("Save question",()=>{if(formId==0)SaveForm();controller.SaveQuestion(formId,questionId,label.Text,type.Text,options.Text,required.Checked,(int)position.Value);Load();Clear();},true);
        var actions=Ui.Bar(saveQuestion,Ui.Button("New",Clear),Ui.Danger("Delete",()=>{if(questionId==0)throw new InvalidOperationException("Select a question first.");if(Ui.Confirm("Delete this question?")){controller.DeleteQuestion(formId,questionId);Load();Clear();}}));AccessLayout.Add(questionStack,actions,108);
        var questionScroll=new Panel{Dock=DockStyle.Fill,AutoScroll=true,BackColor=Color.White};questionScroll.Controls.Add(questionStack);
        var questionCard=Ui.Section("Question settings","Choose a field and define its rules.",questionScroll);questionCard.Margin=new Padding(16,0,0,0);
        var columns=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=new Padding(0)};columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,61));columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,39));columns.Controls.Add(left,0,0);columns.Controls.Add(questionCard,1,0);
        var save=Ui.Button("Save form",()=>{SaveForm();MessageBox.Show("Form details saved.","Saved");},true);save.Enabled=editable;questionCard.Enabled=editable;title.ReadOnly=!editable;instructions.ReadOnly=!editable;
        Controls.Add(Ui.Editor(editable?"Build a digital form":"Form definition",editable?"Create a structured form, one question at a time.":"This definition is locked. Duplicate the form to make changes.",columns,Ui.Bar(Ui.Button("Close builder",Close),save)));
        Load();Clear();
    }
}
