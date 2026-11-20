using SecureForms.Controllers;
using SecureForms.Models;

namespace SecureForms.Views;

public sealed class ResponseDetails:Form
{
    public ResponseDetails(AppController controller,ResponseRecord selected)
    {
        var response=controller.Response(selected.Id);Ui.Style(this,"Kigali City | Submission review",new Size(1110,850));
        var versions=Ui.Choices(controller.Revisions(response.Id).Select(x=>x.ToString()));
        var answers=Ui.Grid();Ui.Column(answers,"Question","Question",130);Ui.Column(answers,"Answer","Answer",220);answers.DefaultCellStyle.WrapMode=DataGridViewTriState.True;answers.AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.AllCells;answers.RowTemplate.MinimumHeight=52;
        var history=Ui.Grid();Ui.Column(history,"Revision","Rev.",40);Ui.Column(history,"Reviewer","Reviewer",90);Ui.Column(history,"Stage","Review stage",110);Ui.Column(history,"Decision","Decision",70);Ui.Column(history,"Comment","Comment",160);Ui.Column(history,"Date","Date",100);history.DefaultCellStyle.WrapMode=DataGridViewTriState.True;history.AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.AllCells;history.RowTemplate.MinimumHeight=52;
        void LoadAnswers()
        {
            if(!int.TryParse(versions.Text,out int version))return;
            var values=controller.Answers(response.Id,version);answers.DataSource=controller.Questions(response.FormId).Select(q=>new{Question=q.Label,Answer=values.GetValueOrDefault(q.Id,"")}).ToList();
        }
        versions.SelectedIndexChanged+=(_,_)=>Ui.Run(LoadAnswers);LoadAnswers();history.DataSource=controller.Reviews(response.Id);
        bool canReview=((controller.User.Role==Roles.Supervisor&&response.Status=="Pending supervisor")||(controller.User.Role==Roles.DepartmentAdmin&&response.Status=="Pending final approval"))&&response.OwnerId!=controller.User.Id;
        var body=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,Margin=new Padding(0)};body.RowStyles.Add(new RowStyle(SizeType.Absolute,104));body.RowStyles.Add(new RowStyle(SizeType.Percent,100));body.RowStyles.Add(new RowStyle(SizeType.Absolute,canReview?210:0));
        var summary=new Surface{Dock=DockStyle.Fill,Padding=new Padding(20),Margin=new Padding(0,0,0,15)};
        var name=Ui.Label(response.FormTitle,14,true);name.Location=new Point(20,15);summary.Controls.Add(name);var metadata=Ui.Label($"Submitted by {response.Owner}   ·   Revision {response.Revision}   ·   {response.UpdatedAt:dd MMM yyyy, HH:mm}",10,false,Ui.Muted);metadata.Location=new Point(21,49);summary.Controls.Add(metadata);
        var status=Ui.Label(response.Status,10,true,Ui.StatusColors(response.Status).Item2);status.BackColor=Ui.StatusColors(response.Status).Item1;status.Padding=new Padding(10,6,10,6);status.Anchor=AnchorStyles.Top|AnchorStyles.Right;status.Top=24;summary.Controls.Add(status);summary.Resize+=(_,_)=>{status.Left=summary.Width-status.Width-20;name.MaximumSize=new Size(Math.Max(150,status.Left-40),0);};body.Controls.Add(summary,0,0);
        var tabs=new TabControl{Dock=DockStyle.Fill,Font=new Font("Segoe UI",10),Padding=new Point(20,10)};
        var answerTab=new TabPage("Submitted answers"){BackColor=Color.White,Padding=new Padding(12)};var answerRows=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2};answerRows.RowStyles.Add(new RowStyle(SizeType.AutoSize));answerRows.RowStyles.Add(new RowStyle(SizeType.Percent,100));var versionInput=Ui.Input(versions);versionInput.Width=150;var label=Ui.Label("Answer revision",10,true);label.Padding=new Padding(0,12,10,0);answerRows.Controls.Add(Ui.Bar(label,versionInput),0,0);answerRows.Controls.Add(answers,0,1);answerTab.Controls.Add(answerRows);
        var historyTab=new TabPage("Decision history"){BackColor=Color.White,Padding=new Padding(12)};historyTab.Controls.Add(Ui.EmptyGrid(history,"No decisions recorded","Decisions will appear here after review."));tabs.TabPages.Add(answerTab);tabs.TabPages.Add(historyTab);body.Controls.Add(tabs,0,1);
        var reason=Ui.Text(true);reason.MaxLength=2000;reason.Height=58;reason.PlaceholderText="Explain your decision. A comment is required when returning or rejecting a response.";
        if(canReview)
        {
            var fields=Ui.Fields();Ui.Field(fields,"Review comment",reason);var reviewCard=Ui.Section("Record your decision","Your decision and comment will be added to the history.",fields);reviewCard.Margin=new Padding(0,16,0,0);body.Controls.Add(reviewCard,0,2);
        }
        var actions=Ui.Bar(Ui.Button("Close",Close));
        if(canReview)
        {
            void Decide(string action){if(Ui.Confirm($"{action} this response?")){controller.Review(response.Id,response.Revision,response.Status,action,reason.Text);MessageBox.Show("Your decision has been saved.","Review completed");DialogResult=DialogResult.OK;}}
            actions.Controls.Add(Ui.Button("Return for correction",()=>Decide("Return")));actions.Controls.Add(Ui.Danger("Reject response",()=>Decide("Reject")));actions.Controls.Add(Ui.Button("Approve response",()=>Decide("Approve"),true));
        }
        Controls.Add(Ui.Editor(response.Reference,"Review the submitted information and its decision history.",body,actions));
    }
}
