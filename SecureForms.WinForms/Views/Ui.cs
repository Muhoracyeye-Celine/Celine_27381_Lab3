using MySqlConnector;

namespace SecureForms.Views;

public static class Ui
{
    public static readonly Color Navy=Color.FromArgb(20,44,67);
    public static readonly Color Blue=Color.FromArgb(37,99,235);
    public static readonly Color Teal=Color.FromArgb(0,132,125);
    public static readonly Color Mint=Color.FromArgb(123,213,196);
    public static readonly Color Gold=Color.FromArgb(244,180,65);
    public static readonly Color Green=Color.FromArgb(23,132,76);
    public static readonly Color Red=Color.FromArgb(181,67,71);
    public static readonly Color Paper=Color.FromArgb(244,247,251);
    public static readonly Color BlueTint=Color.FromArgb(224,237,251);
    public static readonly Color Muted=Color.FromArgb(78,100,123);
    public static readonly Color Line=Color.FromArgb(224,232,240);

    public static bool Run(Action action)
    {
        try{Cursor.Current=Cursors.WaitCursor;action();return true;}
        catch(MySqlException e)
        {
            string message=e.Number switch{1062=>"That name or email already exists.",1451=>"This record is in use. Keep its history; archive or deactivate it instead.",1452=>"The related record no longer exists. Refresh and try again.",_=>"The database operation failed. Check that MySQL is running and the connection settings are correct.\n\n"+e.Message};
            MessageBox.Show(message,"Database",MessageBoxButtons.OK,MessageBoxIcon.Warning);return false;
        }
        catch(Exception e){MessageBox.Show(e.Message,"Please check",MessageBoxButtons.OK,MessageBoxIcon.Warning);return false;}
        finally{Cursor.Current=Cursors.Default;}
    }
    public static void Style(Form form,string title,Size size)
    {
        var area=System.Windows.Forms.Screen.FromPoint(Cursor.Position).WorkingArea;
        form.Text=title;form.ClientSize=new Size(Math.Min(size.Width,Math.Max(800,area.Width-48)),Math.Min(size.Height,Math.Max(560,area.Height-80)));
        form.MinimumSize=new Size(Math.Min(size.Width,940),Math.Min(Math.Min(size.Height,620),area.Height-20));
        form.StartPosition=FormStartPosition.CenterScreen;form.Font=new Font("Segoe UI",10);form.ForeColor=Navy;form.BackColor=Paper;form.AutoScaleDimensions=new SizeF(96,96);form.AutoScaleMode=AutoScaleMode.Dpi;
    }
    public static ActionButton Button(string text,Action action,bool primary=false)
    {
        int width=Math.Max(100,TextRenderer.MeasureText(text,new Font("Segoe UI",10,FontStyle.Bold)).Width+34);
        var b=new ActionButton{Text=text,Primary=primary,Size=new Size(width,42),MinimumSize=new Size(width,42),Margin=new Padding(0,0,8,8),BackColor=Color.Transparent};
        b.Click+=(_,_)=>Run(action);return b;
    }
    public static ActionButton Danger(string text,Action action)
    {
        var b=Button(text,action);b.Destructive=true;return b;
    }
    public static Label Label(string text,float size=10,bool bold=false,Color? color=null)
        =>new(){Text=text,AutoSize=true,Font=new Font("Segoe UI",size,bold?FontStyle.Bold:FontStyle.Regular),ForeColor=color??Navy,BackColor=Color.Transparent,UseMnemonic=false};
    public static FlowLayoutPanel Bar(params Control[] controls)
    {
        var p=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,WrapContents=true,Padding=new Padding(0,0,0,4),Margin=new Padding(0)};p.Controls.AddRange(controls);return p;
    }
    public static DataGridView Grid()
    {
        var g=new DataGridView{Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,AllowUserToResizeRows=false,MultiSelect=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=Color.White,BorderStyle=BorderStyle.None,RowHeadersVisible=false,AutoGenerateColumns=false,CellBorderStyle=DataGridViewCellBorderStyle.SingleHorizontal,GridColor=Color.FromArgb(236,241,246)};
        g.EnableHeadersVisualStyles=false;g.ColumnHeadersBorderStyle=DataGridViewHeaderBorderStyle.None;g.ColumnHeadersDefaultCellStyle.BackColor=Paper;g.ColumnHeadersDefaultCellStyle.ForeColor=Muted;g.ColumnHeadersDefaultCellStyle.Font=new Font("Segoe UI",9,FontStyle.Bold);g.ColumnHeadersDefaultCellStyle.Padding=new Padding(10,0,6,0);
        g.ColumnHeadersHeight=46;g.ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.DisableResizing;g.RowTemplate.Height=52;
        g.DefaultCellStyle.Font=new Font("Segoe UI",10);g.DefaultCellStyle.ForeColor=Navy;g.DefaultCellStyle.BackColor=Color.White;g.DefaultCellStyle.Padding=new Padding(10,3,6,3);g.DefaultCellStyle.SelectionBackColor=Color.FromArgb(236,245,251);g.DefaultCellStyle.SelectionForeColor=Navy;
        g.CellPainting+=(_,e)=>{
            if(e.RowIndex<0||e.ColumnIndex<0||g.Columns[e.ColumnIndex].DataPropertyName is not "Status" and not "Decision" and not "Active")return;
            e.PaintBackground(e.ClipBounds,true);
            string text=e.Value is bool b?(b?"Active":"Inactive"):Convert.ToString(e.Value)??"";
            var (fill,ink)=StatusColors(text);var font=g.DefaultCellStyle.Font!;
            int width=Math.Min(e.CellBounds.Width-16,TextRenderer.MeasureText(text,font).Width+20);
            var rect=new Rectangle(e.CellBounds.Left+8,e.CellBounds.Top+(e.CellBounds.Height-28)/2,Math.Max(15,width),28);
            e.Graphics!.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var shape=Surface.Shape(rect,7);using var brush=new SolidBrush(fill);e.Graphics.FillPath(brush,shape);
            TextRenderer.DrawText(e.Graphics,text,font,Rectangle.Inflate(rect,-7,0),ink,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);e.Handled=true;
        };
        return g;
    }
    public static (Color,Color) StatusColors(string status)=>status switch
    {
        "Approved" or "Published" or "Active" or "Approve" =>(Color.FromArgb(225,244,237),Color.FromArgb(20,122,88)),
        "Pending supervisor" or "Pending final approval" =>(Color.FromArgb(255,244,218),Color.FromArgb(155,106,26)),
        "Returned for correction" or "Return" =>(Color.FromArgb(228,239,254),Blue),
        "Rejected" or "Reject" or "Inactive" =>(Color.FromArgb(253,233,233),Red),
        _=>(Color.FromArgb(237,241,246),Muted)
    };
    public static void Column(DataGridView grid,string property,string title,int weight=100)
    {
        grid.Columns.Add(new DataGridViewTextBoxColumn{DataPropertyName=property,HeaderText=title,FillWeight=weight,MinimumWidth=property=="Id"||property=="Revision"?52:property=="Status"?175:95});
    }
    public static T Selected<T>(DataGridView grid) where T:class
        =>grid.CurrentRow?.DataBoundItem as T??throw new InvalidOperationException("Select a row first.");
    public static TableLayoutPanel Screen(string title,Control toolbar,Control body,string? subtitle=null)
    {
        var p=Page(title,subtitle??Subtitle(title));
        var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,Margin=new Padding(0)};
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        toolbar.Margin=new Padding(0,4,0,12);layout.Controls.Add(toolbar,0,0);
        var surface=new Surface{Dock=DockStyle.Fill,Padding=new Padding(14),Margin=new Padding(0)};body.Dock=DockStyle.Fill;surface.Controls.Add(body);layout.Controls.Add(surface,0,1);
        p.Controls.Add(layout,0,1);return p;
    }
    public static TableLayoutPanel Page(string title,string subtitle)
    {
        var p=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,Padding=new Padding(22,16,22,18),Margin=new Padding(0),BackColor=Paper};
        p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));p.RowStyles.Add(new RowStyle(SizeType.Absolute,68));p.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var head=new Panel{Dock=DockStyle.Fill};var heading=Label(title,21,true);heading.Location=new Point(0,0);var note=Label(subtitle,10,false,Muted);note.Location=new Point(2,38);note.AutoEllipsis=true;note.AutoSize=false;note.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;note.Width=900;note.Height=27;head.Controls.AddRange(new Control[]{heading,note});head.Resize+=(_,_)=>note.Width=head.Width;
        p.Controls.Add(head,0,0);return p;
    }
    private static string Subtitle(string title)=>title switch
    {
        "Digital forms"=>"Design, publish and manage the forms in your department.",
        "Submissions and approval"=>"Track each response from submission to the final decision.",
        "Departments"=>"Keep your organizational structure clear and up to date.",
        "User accounts"=>"Manage people, departments and access to your workspace.",
        "Audit history"=>"A traceable record of activity across the application.",
        _=>"Kigali City Secure Digital Form and Data Management System"
    };
    public static TableLayoutPanel Fields()
    {
        var p=new TableLayoutPanel{Dock=DockStyle.Top,AutoSize=true,ColumnCount=2,RowCount=0,Padding=new Padding(4),BackColor=Color.White};
        p.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,150));p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));return p;
    }
    public static void Field(TableLayoutPanel p,string label,Control control)
    {
        int row=p.RowCount++;p.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var caption=Label(label,10,true);caption.Padding=new Padding(0,12,0,0);caption.Margin=new Padding(0,2,10,12);p.Controls.Add(caption,0,row);
        Control input=control is TextBox or ComboBox or NumericUpDown or DateTimePicker?Input(control):control;
        input.Dock=DockStyle.Fill;input.Margin=new Padding(0,2,0,12);p.Controls.Add(input,1,row);
    }
    public static Surface Input(Control control)
    {
        var frame=new Surface{Height=control is TextBox{Multiline:true}?control.Height+22:46,Padding=new Padding(12,10,12,8),Radius=8,Stroke=Color.FromArgb(206,219,231)};
        control.Font=new Font("Segoe UI",11);control.ForeColor=Navy;control.BackColor=Color.White;
        if(control is TextBox t){t.BorderStyle=BorderStyle.None;t.Dock=DockStyle.Fill;}
        else{control.Dock=DockStyle.Fill;if(control is ComboBox combo)combo.FlatStyle=FlatStyle.Flat;if(control is NumericUpDown number)number.BorderStyle=BorderStyle.None;}
        control.Enter+=(_,_)=>{frame.Stroke=Teal;frame.Invalidate();};control.Leave+=(_,_)=>{frame.Stroke=Color.FromArgb(206,219,231);frame.Invalidate();};frame.Controls.Add(control);return frame;
    }
    public static Panel Stacked(string label,Control control,string hint="")
    {
        var p=new Panel{Height=hint.Length>0?111:91,BackColor=Color.Transparent,Margin=new Padding(0,0,0,0)};
        var caption=Label(label,10,true);caption.Location=new Point(0,0);p.Controls.Add(caption);
        var input=Input(control);input.SetBounds(0,27,200,input.Height);input.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;p.Controls.Add(input);
        p.Resize+=(_,_)=>input.Width=p.Width;
        if(hint.Length>0){var note=Label(hint,9,false,Muted);note.Location=new Point(0,80);note.MaximumSize=new Size(600,0);p.Controls.Add(note);}
        return p;
    }
    public static TextBox Text(bool multiline=false)=>new(){Multiline=multiline,Height=multiline?76:28,MaxLength=multiline?5000:200,BorderStyle=BorderStyle.None,Font=new Font("Segoe UI",11)};
    public static ComboBox Choices(IEnumerable<string> items)
    {
        var c=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Width=200,FlatStyle=FlatStyle.Flat};c.Items.AddRange(items.Cast<object>().ToArray());if(c.Items.Count>0)c.SelectedIndex=0;return c;
    }
    public static Form Dialog(string title,int width=700,int height=450)
    {
        var f=new Form();Style(f,title,new Size(width,height));f.MinimizeBox=false;f.MaximizeBox=false;return f;
    }
    public static TableLayoutPanel Editor(string title,string subtitle,Control body,Control footer)
    {
        var p=Page(title,subtitle);var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,Margin=new Padding(0)};
        layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        body.Dock=DockStyle.Fill;layout.Controls.Add(body,0,0);footer.Margin=new Padding(0,16,0,0);layout.Controls.Add(footer,0,1);p.Controls.Add(layout,0,1);return p;
    }
    public static Surface Section(string title,string subtitle,Control body)
    {
        var card=new Surface{Dock=DockStyle.Fill,Padding=new Padding(18)};
        var rows=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=2,BackColor=Color.White};rows.RowStyles.Add(new RowStyle(SizeType.Absolute,54));rows.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var head=new Panel{Dock=DockStyle.Fill,BackColor=Color.White};var name=Label(title,13,true);var note=Label(subtitle,9,false,Muted);note.Location=new Point(0,28);note.AutoSize=false;note.Height=32;note.AutoEllipsis=true;note.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;head.Resize+=(_,_)=>note.Width=head.Width;head.Controls.AddRange(new Control[]{name,note});rows.Controls.Add(head,0,0);body.Dock=DockStyle.Fill;rows.Controls.Add(body,0,1);card.Controls.Add(rows);return card;
    }
    public static Control EmptyGrid(DataGridView grid,string emptyTitle,string emptyNote)
    {
        var panel=new Panel{Dock=DockStyle.Fill,BackColor=Color.White};var empty=new Panel{Dock=DockStyle.Fill,BackColor=Color.White};
        var icon=Label("\uE8A5",25,false,Blue);icon.Font=new Font("Segoe MDL2 Assets",25);
        var title=Label(emptyTitle,12,true);var note=Label(emptyNote,9,false,Muted);note.AutoSize=false;note.TextAlign=ContentAlignment.MiddleCenter;note.Height=45;
        empty.Controls.AddRange(new Control[]{icon,title,note});
        void Position()
        {
            int center=Math.Max(8,(empty.Height-126)/2);icon.Location=new Point(Math.Max(0,(empty.Width-icon.Width)/2),center);title.Location=new Point(Math.Max(8,(empty.Width-title.Width)/2),center+50);note.SetBounds(10,center+78,Math.Max(50,empty.Width-20),45);
            icon.Visible=empty.Height>=150;
            if(!icon.Visible){title.Top=Math.Max(5,empty.Height/2-28);note.Top=title.Bottom+5;}
        }
        empty.Resize+=(_,_)=>Position();grid.DataBindingComplete+=(_,_)=>{empty.Visible=grid.Rows.Count==0;empty.BringToFront();Position();};panel.Controls.Add(grid);panel.Controls.Add(empty);empty.Visible=grid.Rows.Count==0;Position();return panel;
    }
    public static bool Confirm(string text)=>MessageBox.Show(text,"Confirm",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes;
}
