using SecureForms.Controllers;
using SecureForms.Models;

namespace SecureForms.Views;

public sealed class MainForm:Form
{
    private readonly AppController controller;
    private readonly Panel content=new(){Dock=DockStyle.Fill};
    public bool SignOutRequested{get;private set;}
    private readonly List<ActionButton> navigation=new();
    private readonly Label breadcrumb=Ui.Label("Overview",11,true);
    public MainForm(AppController controller)
    {
        this.controller=controller;
        Ui.Style(this,"Kigali City | Secure Digital Form and Data Management",new Size(1320,760));MinimumSize=new Size(1100,680);WindowState=FormWindowState.Maximized;
        var frame=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=new Padding(0)};
        frame.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,218));frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));frame.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var sidebar=new TableLayoutPanel{Dock=DockStyle.Fill,BackColor=Ui.Navy,ColumnCount=1,RowCount=5,Padding=new Padding(12,20,12,14),Margin=new Padding(0)};
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute,80));sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute,32));sidebar.RowStyles.Add(new RowStyle(SizeType.AutoSize));sidebar.RowStyles.Add(new RowStyle(SizeType.Percent,100));sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute,88));
        var brand=new Panel{Dock=DockStyle.Fill,BackColor=Ui.Navy};var badge=new Surface{Size=new Size(46,44),Padding=new Padding(0),Fill=Ui.Mint,Stroke=Ui.Mint,Radius=11};
        badge.Controls.Add(new Label{Text="COK",Font=new Font("Segoe UI",11,FontStyle.Bold),Dock=DockStyle.Fill,ForeColor=Ui.Navy,TextAlign=ContentAlignment.MiddleCenter,BackColor=Color.Transparent});brand.Controls.Add(badge);
        var city=Ui.Label("CITY OF KIGALI",10,true,Color.White);city.Location=new Point(57,1);brand.Controls.Add(city);
        var product=Ui.Label("Secure Forms",12,true,Color.White);product.Location=new Point(57,24);brand.Controls.Add(product);sidebar.Controls.Add(brand,0,0);
        var menuLabel=Ui.Label("WORKSPACE",8,true,Color.FromArgb(132,159,181));menuLabel.Padding=new Padding(13,0,0,0);sidebar.Controls.Add(menuLabel,0,1);
        var nav=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoSize=true,Margin=new Padding(0),BackColor=Ui.Navy};
        void Link(string title,string glyph,Action action)
        {
            var b=Ui.Button(title,()=>{});b.Navigation=true;b.Glyph=glyph;b.Size=new Size(192,46);b.MinimumSize=b.Size;b.Margin=new Padding(0,0,0,6);
            b.Click+=(_,_)=>Ui.Run(()=>Navigate(b,title,action));navigation.Add(b);nav.Controls.Add(b);
        }
        Link("Overview","\uE80F",Dashboard);Link("Digital forms","\uE8A5",Forms);Link("Submissions","\uE8F1",Responses);
        if(controller.User.Role==Roles.SuperAdmin){Link("Departments","\uE902",Departments);Link("People & Access","\uE77B",Users);Link("Activity Log","\uE81C",Audit);}
        sidebar.Controls.Add(nav,0,2);
        var foot=new Panel{Dock=DockStyle.Fill,BackColor=Ui.Navy};var small=Ui.Label("YOUR ACCESS",8,true,Color.FromArgb(132,159,181));small.Location=new Point(12,10);foot.Controls.Add(small);
        var role=Ui.Label(controller.User.Role,10,true,Color.White);role.Location=new Point(12,33);foot.Controls.Add(role);
        var department=controller.Departments().FirstOrDefault(d=>d.Id==controller.User.DepartmentId)?.Name??"Department workspace";
        var departmentLabel=Ui.Label(department,9,false,Color.FromArgb(178,200,218));departmentLabel.AutoSize=false;departmentLabel.SetBounds(12,59,184,24);departmentLabel.AutoEllipsis=true;foot.Controls.Add(departmentLabel);sidebar.Controls.Add(foot,0,4);frame.Controls.Add(sidebar,0,0);
        var right=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,Margin=new Padding(0)};right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));right.RowStyles.Add(new RowStyle(SizeType.Absolute,64));right.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var header=new Panel{Dock=DockStyle.Fill,BackColor=Ui.BlueTint,Margin=new Padding(0)};
        var organization=Ui.Label("DIGITAL FORM & DATA MANAGEMENT",8,true,Ui.Muted);organization.Location=new Point(24,11);header.Controls.Add(organization);breadcrumb.Location=new Point(24,31);header.Controls.Add(breadcrumb);
        var profile=Ui.Button(controller.User.Name+"  ▾",()=>{});profile.Primary=false;profile.Size=new Size(217,38);profile.MinimumSize=profile.Size;profile.Anchor=AnchorStyles.Right|AnchorStyles.Top;profile.Top=12;
        var profileMenu=new ContextMenuStrip{Font=new Font("Segoe UI",10)};profileMenu.Items.Add(new ToolStripMenuItem(controller.User.Role){Enabled=false});profileMenu.Items.Add(new ToolStripSeparator());profileMenu.Items.Add("Sign out",null,(_,_)=>{SignOutRequested=true;Close();});
        profile.Click+=(_,_)=>profileMenu.Show(profile,new Point(0,profile.Height));header.Controls.Add(profile);header.Resize+=(_,_)=>profile.Left=header.Width-profile.Width-26;
        right.Controls.Add(header,0,0);right.Controls.Add(content,0,1);content.Margin=new Padding(0);frame.Controls.Add(right,1,0);Controls.Add(frame);
        Shown+=(_,_)=>Ui.Run(()=>Navigate(navigation[0],"Overview",Dashboard));
    }
    private void Navigate(ActionButton selected,string title,Action build)
    {
        foreach(var item in navigation){item.Selected=item==selected;item.Invalidate();}
        breadcrumb.Text=title;ShowPage(build);
    }
    private void ShowPage(Action build)
    {
        content.SuspendLayout();
        try{foreach(Control control in content.Controls.Cast<Control>().ToArray())control.Dispose();build();}
        finally{content.ResumeLayout(true);}
    }
    private void Dashboard()
    {
        var records=controller.Responses();var forms=controller.Forms();
        int pending=records.Count(r=>r.Status=="Pending supervisor"||r.Status=="Pending final approval");
        var page=Ui.Page("Workspace overview","Your forms, submissions and review activity at a glance.");
        var scroll=new Panel{Dock=DockStyle.Fill,AutoScroll=true,Margin=new Padding(0)};
        var layout=new TableLayoutPanel{ColumnCount=1,RowCount=4,Margin=new Padding(0)};
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute,84));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,142));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,34));
        var welcome=new Surface{Dock=DockStyle.Fill,Fill=Ui.Blue,Stroke=Ui.Blue,Padding=new Padding(18),Margin=new Padding(0,0,0,14)};
        var greeting=Ui.Label("Welcome, "+controller.User.Name.Split(' ')[0],19,true,Color.White);greeting.Location=new Point(20,10);welcome.Controls.Add(greeting);
        var welcomeNote=Ui.Label(controller.User.Role==Roles.SuperAdmin?"Manage your people, departments and organization-wide activity.":"Your departmental work, organized in one place.",10,false,Color.FromArgb(227,238,255));welcomeNote.Location=new Point(21,43);welcome.Controls.Add(welcomeNote);
        bool super=controller.User.Role==Roles.SuperAdmin;
        var action=Ui.Button(super?"Manage team":"Open digital forms",()=>Navigate(super?navigation[4]:navigation[1],super?"People & Access":"Digital forms",super?Users:Forms),true);action.Anchor=AnchorStyles.Right|AnchorStyles.Top;action.Top=15;welcome.Controls.Add(action);
        welcome.Resize+=(_,_)=>{action.Left=welcome.Width-action.Width-20;welcomeNote.MaximumSize=new Size(Math.Max(150,action.Left-38),0);};layout.Controls.Add(welcome,0,0);
        var cards=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=4,RowCount=1,Margin=new Padding(0)};
        for(int i=0;i<4;i++)cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));
        var metrics=new[]{new MetricCard{Caption="Digital forms",Value=forms.Count.ToString(),Note="In your workspace",Accent=Ui.Blue,Glyph="\uE8A5"},new MetricCard{Caption="Submissions",Value=records.Count.ToString(),Note="Visible to your account",Accent=Color.FromArgb(0,124,131),Glyph="\uE8F1"},new MetricCard{Caption="Awaiting review",Value=pending.ToString(),Note="Across both review stages",Accent=Ui.Gold,Ink=Color.FromArgb(80,48,5),Glyph="\uE823"},new MetricCard{Caption="Approved",Value=records.Count(r=>r.Status=="Approved").ToString(),Note="Final decisions completed",Accent=Ui.Green,Glyph="\uE73E"}};
        for(int i=0;i<4;i++){metrics[i].Dock=DockStyle.Fill;metrics[i].Margin=new Padding(0,0,i==3?0:12,14);cards.Controls.Add(metrics[i],i,0);}layout.Controls.Add(cards,0,1);
        var middle=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=new Padding(0)};middle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,64));middle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,36));
        var recent=Ui.Grid();recent.RowTemplate.Height=42;recent.ColumnHeadersHeight=38;Ui.Column(recent,"FormTitle","Form",150);Ui.Column(recent,"Owner","Submitted by",90);Ui.Column(recent,"Status","Status",140);recent.DataSource=records.Take(5).ToList();
        var list=Ui.Section("Recent submissions","Latest activity within your access",Ui.EmptyGrid(recent,"No submissions yet","New responses will appear here."));list.Margin=new Padding(0,0,14,12);middle.Controls.Add(list,0,0);
        var chart=new StatusChart{Dock=DockStyle.Fill,Items=new(){("Supervisor review",records.Count(r=>r.Status=="Pending supervisor"),Ui.Gold),("Final approval",records.Count(r=>r.Status=="Pending final approval"),Ui.Blue),("Approved",records.Count(r=>r.Status=="Approved"),Ui.Green),("Returned",records.Count(r=>r.Status=="Returned for correction"),Ui.Teal)}};
        var statusCard=Ui.Section("Review progress","Current submission stages",chart);statusCard.Margin=new Padding(0,0,0,12);middle.Controls.Add(statusCard,1,0);layout.Controls.Add(middle,0,2);
        var footer=Ui.Label("REVIEW WORKFLOW     Staff submission  →  Supervisor review  →  Departmental approval",9,true,Ui.Muted);footer.Dock=DockStyle.Fill;footer.TextAlign=ContentAlignment.MiddleLeft;footer.AutoSize=false;layout.Controls.Add(footer,0,3);
        void Fit()
        {
            float scale=DeviceDpi/96f;int minimum=(int)(500*scale);
            layout.SetBounds(0,0,Math.Max(1,scroll.ClientSize.Width-(scroll.ClientSize.Height<minimum?SystemInformation.VerticalScrollBarWidth:0)),Math.Max(scroll.ClientSize.Height,minimum));
            scroll.AutoScrollMinSize=new Size(0,minimum);
        }
        scroll.Controls.Add(layout);scroll.Resize+=(_,_)=>Fit();page.Controls.Add(scroll,0,1);content.Controls.Add(page);Fit();
    }
    private void Forms()
    {
        var grid=Ui.Grid();Ui.Column(grid,"Id","ID",35);Ui.Column(grid,"Title","Form",180);Ui.Column(grid,"Status","Status",75);Ui.Column(grid,"Description","Instructions",200);
        var search=Ui.Text();search.PlaceholderText="Search forms...";var searchBox=Ui.Input(search);searchBox.Width=240;searchBox.Margin=new Padding(0,0,12,8);
        void Load(){grid.DataSource=controller.Forms().Where(f=>(controller.User.Role!=Roles.Staff||f.Status=="Published")&&f.Title.Contains(search.Text,StringComparison.OrdinalIgnoreCase)).ToList();}
        var bar=Ui.Bar(searchBox,Ui.Button("Refresh",Load));
        if(controller.User.Role==Roles.DepartmentAdmin)
        {
            bar.Controls.Add(Ui.Button("New form",()=>{using var edit=new FormEditor(controller,null);edit.ShowDialog(this);Load();},true));
            bar.Controls.Add(Ui.Button("Edit / questions",()=>{using var edit=new FormEditor(controller,Ui.Selected<FormRecord>(grid));edit.ShowDialog(this);Load();}));
            var menu=new ContextMenuStrip{Font=new Font("Segoe UI",10),ShowImageMargin=false};
            void MenuAction(string title,Action action)=>menu.Items.Add(title,null,(_,_)=>Ui.Run(action));
            MenuAction("Publish form",()=>{controller.ChangeFormState(Ui.Selected<FormRecord>(grid).Id,"Published");Load();});
            MenuAction("Duplicate as draft",()=>{controller.DuplicateForm(Ui.Selected<FormRecord>(grid).Id);Load();});
            MenuAction("Archive form",()=>{if(Ui.Confirm("Archive this form? New and resumed responses will be blocked.")){controller.ChangeFormState(Ui.Selected<FormRecord>(grid).Id,"Archived");Load();}});
            menu.Items.Add(new ToolStripSeparator());
            MenuAction("Delete draft",()=>{if(Ui.Confirm("Delete this draft form and its questions?")){controller.DeleteForm(Ui.Selected<FormRecord>(grid).Id);Load();}});
            var more=Ui.Button("More actions ▾",()=>{});more.Click+=(_,_)=>menu.Show(more,new Point(0,more.Height));bar.Controls.Add(more);bar.Disposed+=(_,_)=>menu.Dispose();
        }
        if(controller.User.Role==Roles.Staff)bar.Controls.Add(Ui.Button("Fill form",()=>{using var fill=new ResponseEditor(controller,Ui.Selected<FormRecord>(grid),null);fill.ShowDialog(this);Load();},true));
        bar.Controls.Add(Ui.Button("Preview",()=>{using var f=new ResponseEditor(controller,Ui.Selected<FormRecord>(grid),null,true);f.ShowDialog(this);}));
        search.TextChanged+=(_,_)=>Ui.Run(Load);content.Controls.Add(Ui.Screen("Digital forms",bar,Ui.EmptyGrid(grid,"No forms to display",controller.User.Role==Roles.SuperAdmin?"Department Admins create forms. Add their accounts in People & Access.":controller.User.Role==Roles.DepartmentAdmin?"Choose New form to create your first departmental form.":"Published departmental forms will appear here."),controller.User.Role==Roles.DepartmentAdmin?"Create, publish and manage your departmental forms.":controller.User.Role==Roles.SuperAdmin?"Oversee forms across departments. Form creation is managed by Department Admins.":"Browse the forms available in your department."));Load();
    }
    private void Responses()
    {
        var grid=Ui.Grid();Ui.Column(grid,"Reference","Reference",120);Ui.Column(grid,"FormTitle","Form",150);Ui.Column(grid,"Owner","Submitted by",90);Ui.Column(grid,"Status","Status",120);Ui.Column(grid,"Revision","Rev.",35);Ui.Column(grid,"UpdatedAt","Updated",100);
        var search=Ui.Text();search.PlaceholderText="Reference, name or status...";var searchBox=Ui.Input(search);searchBox.Width=260;searchBox.Margin=new Padding(0,0,12,8);
        void Load(){grid.DataSource=controller.Responses().Where(r=>$"{r.Reference} {r.FormTitle} {r.Owner} {r.Status}".Contains(search.Text,StringComparison.OrdinalIgnoreCase)).ToList();}
        void Open(){using var viewer=new ResponseDetails(controller,Ui.Selected<ResponseRecord>(grid));viewer.ShowDialog(this);Load();}
        var bar=Ui.Bar(searchBox,Ui.Button("Refresh",Load),Ui.Button("Open / review",Open,true));
        if(controller.User.Role==Roles.Staff)bar.Controls.Add(Ui.Button("Edit draft / correction",()=>{
            var r=controller.Response(Ui.Selected<ResponseRecord>(grid).Id);
            if(r.Status is not "Draft" and not "Returned for correction")throw new InvalidOperationException("Only drafts and returned responses can be edited.");
            var f=controller.Forms().Single(x=>x.Id==r.FormId);using var edit=new ResponseEditor(controller,f,r);edit.ShowDialog(this);Load();
        }));
        if(controller.User.Role is Roles.SuperAdmin or Roles.DepartmentAdmin or Roles.Viewer)bar.Controls.Add(Ui.Button("Export CSV",()=>{using var save=new SaveFileDialog{Filter="CSV file|*.csv",FileName="SecureForms_Submissions.csv"};if(save.ShowDialog(this)==DialogResult.OK){controller.ExportResponses(save.FileName,search.Text);MessageBox.Show("CSV exported.");}}));
        grid.CellDoubleClick+=(_,e)=>{if(e.RowIndex>=0)Ui.Run(Open);};search.TextChanged+=(_,_)=>Ui.Run(Load);content.Controls.Add(Ui.Screen("Submissions and approval",bar,Ui.EmptyGrid(grid,"No submissions found","Submitted responses will appear here.")));Load();
    }
    private void Departments()
    {
        var grid=Ui.Grid();Ui.Column(grid,"Id","ID",30);Ui.Column(grid,"Name","Department",220);
        void Load()=>grid.DataSource=controller.Departments();
        void Edit(bool existing)
        {
            var row=existing?Ui.Selected<Department>(grid):null;using var f=Ui.Dialog("Department",740,420);var fields=Ui.Fields();var name=Ui.Text();name.Text=row?.Name??"";Ui.Field(fields,"Department name",name);
            var card=Ui.Section("Department details","Choose a clear name for this department.",fields);f.Controls.Add(Ui.Editor("Department",row==null?"Add a department to your organization.":"Update the selected department.",card,Ui.Bar(Ui.Button("Cancel",()=>f.Close()),Ui.Button("Save department",()=>{controller.SaveDepartment(row?.Id??0,name.Text);f.DialogResult=DialogResult.OK;},true))));f.ShowDialog(this);Load();
        }
        var bar=Ui.Bar(Ui.Button("Add",()=>Edit(false),true),Ui.Button("Edit",()=>Edit(true)),Ui.Danger("Delete unused",()=>{if(Ui.Confirm("Delete this unused department?")){controller.DeleteDepartment(Ui.Selected<Department>(grid).Id);Load();}}),Ui.Button("Refresh",Load));
        content.Controls.Add(Ui.Screen("Departments",bar,grid));Load();
    }
    private void Users()
    {
        var grid=Ui.Grid();Ui.Column(grid,"Name","Name",100);Ui.Column(grid,"Email","Email",150);Ui.Column(grid,"Role","Role",100);Ui.Column(grid,"DepartmentId","Department",100);Ui.Column(grid,"Active","Status",65);
        var departmentNames=controller.Departments().ToDictionary(d=>d.Id,d=>d.Name);
        grid.CellFormatting+=(_,e)=>{if(e.ColumnIndex>=0&&grid.Columns[e.ColumnIndex].DataPropertyName=="DepartmentId"&&e.Value is int id){e.Value=departmentNames.GetValueOrDefault(id,id.ToString());e.FormattingApplied=true;}};
        void Load()=>grid.DataSource=controller.Accounts();
        void Edit(bool existing){using var f=new AccountEditor(controller,existing?Ui.Selected<Account>(grid):null);f.ShowDialog(this);Load();}
        content.Controls.Add(Ui.Screen("User accounts",Ui.Bar(Ui.Button("Add account",()=>Edit(false),true),Ui.Button("Edit / deactivate",()=>Edit(true)),Ui.Button("Refresh",Load)),grid));Load();
    }
    private void Audit()
    {
        var grid=Ui.Grid();Ui.Column(grid,"Date","Time",100);Ui.Column(grid,"Actor","Actor",100);Ui.Column(grid,"Action","Action",100);Ui.Column(grid,"Details","Details",240);
        void Load()=>grid.DataSource=controller.AuditLog();
        content.Controls.Add(Ui.Screen("Audit history",Ui.Bar(Ui.Button("Refresh",Load),new Label{Text="Latest 1,000 events",AutoSize=true,Padding=new Padding(8,12,0,0)}),grid));Load();
    }
}
