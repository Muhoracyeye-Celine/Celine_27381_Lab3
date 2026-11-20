using System.Drawing.Drawing2D;

namespace SecureForms.Views;

public sealed class Surface : Panel
{
    public int Radius { get; set; } = 14;
    public Color Fill { get; set; } = Color.White;
    public Color Stroke { get; set; } = Color.FromArgb(224,232,240);
    public Surface()
    {
        DoubleBuffered=true;
        SetStyle(ControlStyles.ResizeRedraw|ControlStyles.SupportsTransparentBackColor,true);
        BackColor=Color.Transparent;
        Padding=new Padding(20);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        using var path=Shape(new RectangleF(1,1,Width-3,Height-3),Radius);
        using var brush=new SolidBrush(Fill);
        using var pen=new Pen(Stroke);
        e.Graphics.FillPath(brush,path);
        e.Graphics.DrawPath(pen,path);
        base.OnPaint(e);
    }
    public static GraphicsPath Shape(RectangleF r,float radius)
    {
        float d=Math.Min(radius*2,Math.Min(r.Width,r.Height));
        var path=new GraphicsPath();
        if(d<1){path.AddRectangle(r);return path;}
        path.AddArc(r.X,r.Y,d,d,180,90);path.AddArc(r.Right-d,r.Y,d,d,270,90);
        path.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);path.AddArc(r.X,r.Bottom-d,d,d,90,90);path.CloseFigure();return path;
    }
}

public sealed class ActionButton : Control, IButtonControl
{
    public bool Primary { get; set; }
    public bool Destructive { get; set; }
    public bool Navigation { get; set; }
    public bool Selected { get; set; }
    public string Glyph { get; set; }="";
    public DialogResult DialogResult { get; set; }
    private bool hovering;
    private bool pressed;
    public ActionButton()
    {
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw|ControlStyles.Selectable|ControlStyles.StandardClick|ControlStyles.SupportsTransparentBackColor,true);
        BackColor=Color.Transparent;Cursor=Cursors.Hand;Font=new Font("Segoe UI",10,FontStyle.Bold);TabStop=true;AccessibleRole=AccessibleRole.PushButton;
    }
    public void NotifyDefault(bool value){Invalidate();}
    public void PerformClick(){if(Enabled&&Visible)OnClick(EventArgs.Empty);}
    protected override bool IsInputKey(Keys keyData)=>keyData is Keys.Space or Keys.Enter||base.IsInputKey(keyData);
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if(e.KeyCode is Keys.Space or Keys.Enter){pressed=true;Invalidate();e.Handled=true;}
        base.OnKeyDown(e);
    }
    protected override void OnKeyUp(KeyEventArgs e)
    {
        if(e.KeyCode is Keys.Space or Keys.Enter){pressed=false;Invalidate();PerformClick();e.Handled=true;}
        base.OnKeyUp(e);
    }
    protected override void OnClick(EventArgs e)
    {
        if(DialogResult!=DialogResult.None&&FindForm() is Form form)form.DialogResult=DialogResult;
        base.OnClick(e);
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        Control? parent=Parent;Color color=Ui.Paper;
        while(parent!=null)
        {
            if(parent is Surface surface){color=surface.Fill;break;}
            if(parent.BackColor.A==255){color=parent.BackColor;break;}
            parent=parent.Parent;
        }
        e.Graphics.Clear(color);
    }
    protected override void OnMouseEnter(EventArgs e){hovering=true;Invalidate();base.OnMouseEnter(e);}
    protected override void OnMouseLeave(EventArgs e){hovering=false;pressed=false;Invalidate();base.OnMouseLeave(e);}
    protected override void OnMouseDown(MouseEventArgs e){if(e.Button==MouseButtons.Left){Focus();pressed=true;Invalidate();}base.OnMouseDown(e);}
    protected override void OnMouseUp(MouseEventArgs e){pressed=false;Invalidate();base.OnMouseUp(e);}
    protected override void OnEnabledChanged(EventArgs e){Invalidate();base.OnEnabledChanged(e);}
    protected override void OnGotFocus(EventArgs e){Invalidate();base.OnGotFocus(e);}
    protected override void OnLostFocus(EventArgs e){pressed=false;Invalidate();base.OnLostFocus(e);}
    protected override void OnTextChanged(EventArgs e){Invalidate();base.OnTextChanged(e);}
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
        Color fill=Navigation?(Selected?Ui.Teal:hovering?Color.FromArgb(33,64,91):Ui.Navy):Primary?(hovering||pressed?Color.FromArgb(3,99,94):Ui.Teal):Destructive?(hovering?Color.FromArgb(252,221,222):Color.FromArgb(255,240,240)):hovering?Ui.BlueTint:Color.White;
        Color ink=Navigation?(Selected?Color.White:Color.FromArgb(214,227,239)):Primary?Color.White:Destructive?Ui.Red:Ui.Navy;
        if(!Enabled){fill=Color.FromArgb(236,240,244);ink=Color.FromArgb(137,150,163);}
        using var path=Surface.Shape(new RectangleF(1,1,Width-3,Height-3),9);using var brush=new SolidBrush(fill);g.FillPath(brush,path);
        if(!Primary&&!Navigation){using var border=new Pen(Destructive?Color.FromArgb(245,201,203):Ui.Line);g.DrawPath(border,path);}
        var textBounds=new Rectangle(12,0,Width-24,Height);
        if(Glyph.Length>0)
        {
            using var font=new Font("Segoe MDL2 Assets",13);
            TextRenderer.DrawText(g,Glyph,font,new Rectangle(14,0,25,Height),ink,TextFormatFlags.VerticalCenter|TextFormatFlags.HorizontalCenter|TextFormatFlags.NoPrefix);
            textBounds=new Rectangle(48,0,Width-58,Height);
        }
        TextRenderer.DrawText(g,Text,Font,textBounds,ink,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix|(Navigation?TextFormatFlags.Left:TextFormatFlags.HorizontalCenter));
        if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(g,Rectangle.Inflate(ClientRectangle,-6,-6),ink,fill);
        base.OnPaint(e);
    }
}

public sealed class BrandPanel : Panel
{
    public BrandPanel(){DoubleBuffered=true;BackColor=Ui.Navy;}
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
        using var circle=new SolidBrush(Color.FromArgb(28,56,81));
        g.FillEllipse(circle,Width-210,-150,390,390);
        g.FillEllipse(circle,-190,Height-255,430,430);
        int left=34;int width=Math.Max(180,Width-68);
        using var monogramFont=new Font("Segoe UI",12,FontStyle.Bold);
        using var titleFont=new Font("Segoe UI",29,FontStyle.Bold);
        using var subtitleFont=new Font("Segoe UI",11);
        using var smallFont=new Font("Segoe UI",9,FontStyle.Bold);
        using var white=new SolidBrush(Color.White);using var muted=new SolidBrush(Color.FromArgb(185,205,221));
        using var mint=new SolidBrush(Ui.Mint);using var gold=new SolidBrush(Ui.Gold);
        using var badge=Surface.Shape(new RectangleF(left,34,52,48),12);g.FillPath(mint,badge);
        TextRenderer.DrawText(g,"COK",monogramFont,new Rectangle(left,34,52,48),Ui.Navy,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
        g.DrawString("CITY OF KIGALI",smallFont,white,new RectangleF(left+66,40,width-66,23));
        g.DrawString("DIGITAL WORKSPACE",smallFont,muted,new RectangleF(left+66,61,width-66,22));
        g.FillRectangle(gold,left,132,38,4);
        g.DrawString("Better forms.\nConnected work.",titleFont,white,new RectangleF(left,158,width,170));
        g.DrawString("Create, collect and review departmental information in one workspace.",subtitleFont,muted,new RectangleF(left,334,width,90));
        float baseline=Height-95;
        float skylineScale=Math.Min(1f,Math.Max(.35f,(Height-500)/170f));
        using var building=new SolidBrush(Color.FromArgb(40,73,98));
        int[] heights={40,71,53,100,62,130,86,55,95};
        for(int i=0;i<heights.Length;i++)
        {
            float x=left+i*(width/9f);float w=width/9f-5;
            float height=heights[i]*skylineScale;
            g.FillRectangle(building,x,baseline-height,w,height);
            for(int y=0;y<height-15;y+=18)g.FillRectangle(mint,x+7,baseline-height+10+y,3,4);
        }
        using var line=new Pen(Color.FromArgb(68,94,116));g.DrawLine(line,left,baseline+12,left+width,baseline+12);
        g.DrawString("SECURE DIGITAL FORM &\nDATA MANAGEMENT SYSTEM",smallFont,muted,new RectangleF(left,baseline+29,width,50));
        base.OnPaint(e);
    }
}

public sealed class MetricCard : Control
{
    public string Caption { get; set; }="";
    public string Value { get; set; }="0";
    public string Note { get; set; }="";
    public string Glyph { get; set; }="\uE8A5";
    public Color Accent { get; set; }=Ui.Teal;
    public Color Ink { get; set; }=Color.White;
    public MetricCard(){DoubleBuffered=true;BackColor=Ui.Paper;SetStyle(ControlStyles.ResizeRedraw,true);}
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;float scale=DeviceDpi/96f;
        int P(float value)=>(int)Math.Round(value*scale);
        using var path=Surface.Shape(new RectangleF(1,1,Width-3,Height-3),P(12));using var fill=new SolidBrush(Accent);g.FillPath(fill,path);
        var state=g.Save();g.SetClip(path);
        using var decoration=new SolidBrush(Color.FromArgb(22,Ink));g.FillEllipse(decoration,Width-P(75),-P(26),P(125),P(125));g.Restore(state);
        using var cap=new Font("Segoe UI",10,FontStyle.Bold);using var valueFont=new Font("Segoe UI",29,FontStyle.Bold);using var note=new Font("Segoe UI",8.5f);using var iconFont=new Font("Segoe MDL2 Assets",15);
        var flags=TextFormatFlags.Left|TextFormatFlags.NoPrefix|TextFormatFlags.EndEllipsis;
        TextRenderer.DrawText(g,Caption,cap,new Rectangle(P(19),P(16),Width-P(73),P(25)),Ink,flags);
        TextRenderer.DrawText(g,Glyph,iconFont,new Rectangle(Width-P(46),P(17),P(27),P(26)),Ink,TextFormatFlags.HorizontalCenter|TextFormatFlags.NoPrefix);
        TextRenderer.DrawText(g,Value,valueFont,new Rectangle(P(15),P(46),Width-P(30),P(56)),Ink,flags);
        TextRenderer.DrawText(g,Note,note,new Rectangle(P(19),Height-P(30),Width-P(30),P(22)),Ink,flags);
        base.OnPaint(e);
    }
}

public sealed class StatusChart : Control
{
    public List<(string Label,int Count,Color Color)> Items { get; set; }=new();
    public StatusChart(){DoubleBuffered=true;BackColor=Color.White;SetStyle(ControlStyles.ResizeRedraw,true);}
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;float scale=DeviceDpi/96f;int P(float value)=>(int)Math.Round(value*scale);using var font=new Font("Segoe UI",9);
        if(Items.All(i=>i.Count==0))
        {
            using var heading=new Font("Segoe UI",12,FontStyle.Bold);using var glyph=new Font("Segoe MDL2 Assets",24);
            int top=Math.Max(P(4),Height/2-P(55));
            bool compact=Height<P(125);
            if(!compact)TextRenderer.DrawText(g,"\uE9D9",glyph,new Rectangle(0,top,Width,P(42)),Ui.Blue,TextFormatFlags.HorizontalCenter|TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g,"No reviews yet",heading,new Rectangle(0,compact?Math.Max(4,Height/2-P(25)):top+P(46),Width,P(27)),Ui.Navy,TextFormatFlags.HorizontalCenter|TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g,"Review activity will appear here.",font,new Rectangle(0,compact?Height/2+P(8):top+P(78),Width,P(25)),Ui.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.NoPrefix|TextFormatFlags.EndEllipsis);
            return;
        }
        int maximum=Math.Max(1,Items.Max(i=>i.Count));int row=Math.Max(P(27),Math.Min(P(48),Height/Math.Max(1,Items.Count)));int y=0;
        foreach(var item in Items)
        {
            TextRenderer.DrawText(g,item.Label,font,new Rectangle(0,y,Width-P(30),Math.Max(P(18),row-P(10))),Ui.Muted,TextFormatFlags.Left|TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g,item.Count.ToString(),font,new Rectangle(Width-P(30),y,P(30),P(24)),Ui.Navy,TextFormatFlags.Right|TextFormatFlags.NoPrefix);
            using var track=new SolidBrush(Ui.Paper);using var bar=new SolidBrush(item.Color);
            g.FillRectangle(track,0,y+row-P(8),Width-3,P(6));if(item.Count>0)g.FillRectangle(bar,0,y+row-P(8),Math.Max(P(5),(Width-3)*item.Count/maximum),P(6));y+=row;
        }
        base.OnPaint(e);
    }
}
