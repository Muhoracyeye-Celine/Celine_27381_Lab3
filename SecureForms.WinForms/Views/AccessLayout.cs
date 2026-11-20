namespace SecureForms.Views;

public static class AccessLayout
{
    public static Control Build(string step,string title,string subtitle,Control fields,ActionButton submit,string footer,int cardHeight)
    {
        var root=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Margin=new Padding(0)};
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,38));root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,62));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        var brand=new BrandPanel{Dock=DockStyle.Fill,Margin=new Padding(0)};root.Controls.Add(brand,0,0);
        var right=new Panel{Dock=DockStyle.Fill,AutoScroll=true,BackColor=Ui.Paper,Margin=new Padding(0)};
        var card=new Surface{Size=new Size(490,cardHeight),Padding=new Padding(32),Radius=18};
        var rows=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=6,BackColor=Color.White,Margin=new Padding(0)};
        rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        rows.RowStyles.Add(new RowStyle(SizeType.Absolute,35));rows.RowStyles.Add(new RowStyle(SizeType.Absolute,55));rows.RowStyles.Add(new RowStyle(SizeType.Absolute,66));rows.RowStyles.Add(new RowStyle(SizeType.Percent,100));rows.RowStyles.Add(new RowStyle(SizeType.Absolute,60));rows.RowStyles.Add(new RowStyle(SizeType.Absolute,45));
        var stepLabel=Ui.Label(step,9,true,Ui.Teal);stepLabel.Margin=new Padding(0,0,0,0);rows.Controls.Add(stepLabel,0,0);
        var heading=Ui.Label(title,24,true);heading.Dock=DockStyle.Fill;heading.AutoSize=false;heading.Margin=new Padding(0);rows.Controls.Add(heading,0,1);
        var note=Ui.Label(subtitle,10,false,Ui.Muted);note.AutoSize=false;note.Dock=DockStyle.Fill;note.Margin=new Padding(0,3,0,0);rows.Controls.Add(note,0,2);
        fields.Dock=DockStyle.Fill;fields.Margin=new Padding(0);rows.Controls.Add(fields,0,3);
        submit.Dock=DockStyle.Top;submit.Height=46;submit.Margin=new Padding(0,5,0,9);rows.Controls.Add(submit,0,4);
        var fine=Ui.Label(footer,9,false,Ui.Muted);fine.Dock=DockStyle.Fill;fine.AutoSize=false;fine.Margin=new Padding(0,3,0,0);rows.Controls.Add(fine,0,5);card.Controls.Add(rows);right.Controls.Add(card);
        void Position()
        {
            card.Width=Math.Min(520,Math.Max(370,right.ClientSize.Width-60));
            card.Left=Math.Max(20,(right.ClientSize.Width-card.Width)/2);
            card.Top=Math.Max(26,(right.ClientSize.Height-card.Height)/2);
            right.AutoScrollMinSize=new Size(0,card.Height+52);
        }
        right.Resize+=(_,_)=>Position();root.Controls.Add(right,1,0);root.HandleCreated+=(_,_)=>Position();return root;
    }
    public static TableLayoutPanel Stack()
    {
        var p=new TableLayoutPanel{ColumnCount=1,RowCount=0,Dock=DockStyle.Top,AutoSize=true,BackColor=Color.White};p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));return p;
    }
    public static void Add(TableLayoutPanel p,Control field,int height=91)
    {
        int row=p.RowCount++;p.RowStyles.Add(new RowStyle(SizeType.Absolute,height));field.Dock=DockStyle.Fill;field.Margin=new Padding(0);p.Controls.Add(field,0,row);
    }
}
