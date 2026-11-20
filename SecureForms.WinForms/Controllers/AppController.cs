using System.Globalization;
using System.Net.Mail;
using System.Text;
using MySqlConnector;
using SecureForms.Data;
using SecureForms.Models;
using SecureForms.Security;

namespace SecureForms.Controllers;

public sealed class AppController
{
    private readonly Store store;
    public Session User { get; }
    public AppController(Store store, Session user) { this.store = store; User = user; }
    private void Authorize(params string[] roles)
    {
        if (!store.IsCurrent(User)) throw new InvalidOperationException("Your account changed or was deactivated. Sign out and sign in again.");
        if (roles.Length > 0 && !roles.Contains(User.Role)) throw new InvalidOperationException("Your role cannot perform this action.");
    }
    private void OwnDepartment(int id)
    {
        if (id != User.DepartmentId) throw new InvalidOperationException("This record belongs to another department.");
    }
    private void CanRead(ResponseRecord r)
    {
        if (User.Role == Roles.SuperAdmin && r.Status != "Draft") return;
        OwnDepartment(r.DepartmentId);
        if ((User.Role == Roles.Staff || r.Status == "Draft") && r.OwnerId != User.Id)
            throw new InvalidOperationException("You cannot open this response.");
        if (User.Role == Roles.Viewer && r.Status != "Approved") throw new InvalidOperationException("Management can view approved responses only.");
    }
    public static Session Login(Store store, string email, string password)
    {
        var found = store.Credentials(email.Trim());
        if (found == null || !Passwords.Verify(password, found.Value.Hash)) throw new InvalidOperationException("Incorrect email or password, or the account is inactive.");
        using var c=store.Db.Open(); using var tx=c.BeginTransaction();
        Database.Audit(c,tx,found.Value.User.Id,"Login","Successful desktop sign-in");tx.Commit();
        return found.Value.User;
    }
    public static void FirstAdmin(Store store,string name,string email,string password)
    {
        name=Rules.Text(name,"Name",120);email=ValidEmail(email);string hash=Passwords.Hash(password);
        using var c=store.Db.Open();
        using var guard=Database.Command(c,null,"SELECT GET_LOCK('secure_forms_lab3_first_admin',8)");
        if(Convert.ToInt32(guard.ExecuteScalar())!=1)throw new InvalidOperationException("Setup is already running. Please retry.");
        try
        {
            using var tx=c.BeginTransaction();
            using(var count=Database.Command(c,tx,"SELECT COUNT(*) FROM users"))
                if(Convert.ToInt32(count.ExecuteScalar())>0)throw new InvalidOperationException("An administrator already exists. Sign in instead.");
            Database.Execute(c,tx,"INSERT IGNORE INTO departments(name) VALUES('Digitalization Division')");
            using var dep=Database.Command(c,tx,"SELECT id FROM departments WHERE name='Digitalization Division'");
            int department=Convert.ToInt32(dep.ExecuteScalar());
            int id=(int)Database.Insert(c,tx,"INSERT INTO users(full_name,email,password_hash,role,department_id) VALUES(@n,@e,@p,@r,@d)",("@n",name),("@e",email),("@p",hash),("@r",Roles.SuperAdmin),("@d",department));
            Database.Audit(c,tx,id,"Initial setup","First administrator created");tx.Commit();
        }
        finally { using var release=Database.Command(c,null,"SELECT RELEASE_LOCK('secure_forms_lab3_first_admin')");release.ExecuteScalar(); }
    }
    private static string ValidEmail(string email)
    {
        email=Rules.Text(email,"Email",180);
        if(!MailAddress.TryCreate(email,out var parsed)||parsed.Address!=email)throw new InvalidOperationException("Enter a valid email address.");
        return email;
    }
    public List<Department> Departments(){Authorize();return store.Departments();}
    public List<Account> Accounts(){Authorize(Roles.SuperAdmin);return store.Accounts();}
    public void SaveDepartment(int id,string name)
    {
        Authorize(Roles.SuperAdmin);name=Rules.Text(name,"Department name",100);
        using var c=store.Db.Open();using var tx=c.BeginTransaction();
        if(id==0)Database.Execute(c,tx,"INSERT INTO departments(name) VALUES(@n)",("@n",name));
        else if(Database.Execute(c,tx,"UPDATE departments SET name=@n WHERE id=@i",("@n",name),("@i",id))==0)throw new InvalidOperationException("Department not found or unchanged.");
        Database.Audit(c,tx,User.Id,"Save department",name);tx.Commit();
    }
    public void DeleteDepartment(int id)
    {
        Authorize(Roles.SuperAdmin);
        using var c=store.Db.Open();using var tx=c.BeginTransaction();
        Database.Execute(c,tx,"DELETE FROM departments WHERE id=@i",("@i",id));
        Database.Audit(c,tx,User.Id,"Delete department",$"Department {id}");tx.Commit();
    }
    public void SaveAccount(int id,string name,string email,string role,int department,bool active,string password)
    {
        Authorize(Roles.SuperAdmin);name=Rules.Text(name,"Name",120);email=ValidEmail(email);
        if(!Roles.All.Contains(role))throw new InvalidOperationException("Select a role.");
        if(department<=0)throw new InvalidOperationException("Select a department.");
        if(id==User.Id && (!active||role!=User.Role||department!=User.DepartmentId))throw new InvalidOperationException("You cannot deactivate, move, or change the role of your signed-in account.");
        string? hash=string.IsNullOrEmpty(password)&&id!=0?null:Passwords.Hash(password);
        using var c=store.Db.Open();using var tx=c.BeginTransaction();
        if(id==0)Database.Execute(c,tx,"INSERT INTO users(full_name,email,password_hash,role,department_id,active) VALUES(@n,@e,@p,@r,@d,@a)",("@n",name),("@e",email),("@p",hash),("@r",role),("@d",department),("@a",active));
        else Database.Execute(c,tx,"UPDATE users SET full_name=@n,email=@e,role=@r,department_id=@d,active=@a,password_hash=COALESCE(@p,password_hash) WHERE id=@i",("@n",name),("@e",email),("@p",hash),("@r",role),("@d",department),("@a",active),("@i",id));
        Database.Audit(c,tx,User.Id,"Save user",$"{email}; {role}; active={active}");tx.Commit();
    }
    public List<FormRecord> Forms(){Authorize();return store.Forms(User);}
    public List<Question> Questions(int id)
    {
        Authorize();using var c=store.Db.Open();var f=store.GetForm(c,null,id);
        if(User.Role!=Roles.SuperAdmin)OwnDepartment(f.DepartmentId);
        return store.Questions(id);
    }
    private FormRecord EditableForm(MySqlConnection c,MySqlTransaction tx,int id)
    {
        Authorize(Roles.DepartmentAdmin);var f=store.GetForm(c,tx,id,true);OwnDepartment(f.DepartmentId);
        if(f.Status!="Draft")throw new InvalidOperationException("Only draft forms can be changed. Duplicate a published form to create a new version.");
        return f;
    }
    public int SaveForm(int id,string title,string description)
    {
        Authorize(Roles.DepartmentAdmin);title=Rules.Text(title,"Title",150);
        if(description.Length>4000)throw new InvalidOperationException("Instructions must not exceed 4000 characters.");
        using var c=store.Db.Open();using var tx=c.BeginTransaction();
        if(id==0)id=(int)Database.Insert(c,tx,"INSERT INTO forms(title,description,department_id,created_by) VALUES(@t,@x,@d,@u)",("@t",title),("@x",description.Trim()),("@d",User.DepartmentId),("@u",User.Id));
        else{EditableForm(c,tx,id);Database.Execute(c,tx,"UPDATE forms SET title=@t,description=@x WHERE id=@i",("@t",title),("@x",description.Trim()),("@i",id));}
        Database.Audit(c,tx,User.Id,"Save form",$"Form {id}: {title}");tx.Commit();return id;
    }
    public void SaveQuestion(int form,int id,string label,string type,string options,bool required,int position)
    {
        label=Rules.Text(label,"Question label",200);
        if(!FieldTypes.All.Contains(type))throw new InvalidOperationException("Select a field type.");
        options=string.Join('|',options.Split('|',StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries).Distinct());
        if(type=="Dropdown" && options.Length==0)throw new InvalidOperationException("Enter dropdown choices separated by |.");
        if(options.Length>2000)throw new InvalidOperationException("The choice list is too long.");
        if(position<1||position>1000)throw new InvalidOperationException("Position must be between 1 and 1000.");
        using var c=store.Db.Open();using var tx=c.BeginTransaction();EditableForm(c,tx,form);
        if(id==0)Database.Execute(c,tx,"INSERT INTO questions(form_id,label,field_type,options_text,required,position) VALUES(@f,@l,@t,@o,@r,@p)",("@f",form),("@l",label),("@t",type),("@o",options),("@r",required),("@p",position));
        else Database.Execute(c,tx,"UPDATE questions SET label=@l,field_type=@t,options_text=@o,required=@r,position=@p WHERE id=@i AND form_id=@f",("@f",form),("@i",id),("@l",label),("@t",type),("@o",options),("@r",required),("@p",position));
        Database.Audit(c,tx,User.Id,"Save question",$"Form {form}; question {label}");tx.Commit();
    }
    public void DeleteQuestion(int form,int question)
    {
        using var c=store.Db.Open();using var tx=c.BeginTransaction();EditableForm(c,tx,form);
        Database.Execute(c,tx,"DELETE FROM questions WHERE id=@i AND form_id=@f",("@i",question),("@f",form));
        Database.Audit(c,tx,User.Id,"Delete question",$"Question {question}");tx.Commit();
    }
    public void ChangeFormState(int id,string status)
    {
        Authorize(Roles.DepartmentAdmin);using var c=store.Db.Open();using var tx=c.BeginTransaction();var form=store.GetForm(c,tx,id,true);OwnDepartment(form.DepartmentId);
        if(status=="Published")
        {
            if(form.Status!="Draft")throw new InvalidOperationException("Only draft forms can be published.");
            if(store.Questions(id).Count==0)throw new InvalidOperationException("Add at least one question before publishing.");
        }
        else if(status!="Archived"||form.Status!="Published")throw new InvalidOperationException("Select a published form to archive.");
        Database.Execute(c,tx,"UPDATE forms SET status=@s WHERE id=@i",("@s",status),("@i",id));
        Database.Audit(c,tx,User.Id,"Form state",$"Form {id}: {status}");tx.Commit();
    }
    public void DeleteForm(int id)
    {
        using var c=store.Db.Open();using var tx=c.BeginTransaction();EditableForm(c,tx,id);
        Database.Execute(c,tx,"DELETE FROM forms WHERE id=@i",("@i",id));Database.Audit(c,tx,User.Id,"Delete form",$"Form {id}");tx.Commit();
    }
    public int DuplicateForm(int id)
    {
        Authorize(Roles.DepartmentAdmin);using var c=store.Db.Open();using var tx=c.BeginTransaction();var f=store.GetForm(c,tx,id,true);OwnDepartment(f.DepartmentId);
        string title=f.Title.Length>140?f.Title[..140]:f.Title;
        int copy=(int)Database.Insert(c,tx,"INSERT INTO forms(title,description,department_id,created_by) VALUES(@t,@x,@d,@u)",("@t",title+" (copy)"),("@x",f.Description),("@d",f.DepartmentId),("@u",User.Id));
        Database.Execute(c,tx,"INSERT INTO questions(form_id,label,field_type,options_text,required,position) SELECT @copy,label,field_type,options_text,required,position FROM questions WHERE form_id=@old",("@copy",copy),("@old",id));
        Database.Audit(c,tx,User.Id,"Duplicate form",$"Form {id} copied to draft {copy}");tx.Commit();return copy;
    }
    public List<ResponseRecord> Responses(){Authorize();return store.Responses(User);}
    public ResponseRecord Response(int id)
    {
        Authorize();using var c=store.Db.Open();var r=store.GetResponse(c,null,id);CanRead(r);return r;
    }
    public List<int> Revisions(int id){Response(id);return store.Revisions(id);}
    public Dictionary<int,string> Answers(int id,int revision){Response(id);return store.Answers(id,revision);}
    public List<ReviewRecord> Reviews(int id){Response(id);return store.Reviews(id);}
    public int SaveResponse(int formId,int id,int expectedRevision,Dictionary<int,string> answers,bool draft)
    {
        Authorize(Roles.Staff);using var c=store.Db.Open();using var tx=c.BeginTransaction();
        var f=store.GetForm(c,tx,formId,true);OwnDepartment(f.DepartmentId);
        if(f.Status!="Published")throw new InvalidOperationException("This form is no longer published.");
        var questions=store.Questions(formId);Rules.ValidateAnswers(questions,answers,draft);
        int revision=1;string status=draft?"Draft":"Pending supervisor";
        if(id==0)
        {
            string reference="COK-"+Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
            id=(int)Database.Insert(c,tx,"INSERT INTO submissions(reference_no,form_id,submitted_by,status) VALUES(@r,@f,@u,@s)",("@r",reference),("@f",formId),("@u",User.Id),("@s",status));
        }
        else
        {
            var old=store.GetResponse(c,tx,id,true);
            if(old.OwnerId!=User.Id||old.FormId!=formId||old.Revision!=expectedRevision||old.Status is not "Draft" and not "Returned for correction")
                throw new InvalidOperationException("The response changed or cannot be edited. Refresh and try again.");
            revision=old.Revision+1;
            Database.Execute(c,tx,"UPDATE submissions SET status=@s,revision=@v,first_reviewer_id=NULL,updated_at=CURRENT_TIMESTAMP WHERE id=@i",("@s",status),("@v",revision),("@i",id));
        }
        foreach(var q in questions)Database.Execute(c,tx,"INSERT INTO answers(submission_id,revision,question_id,answer_text) VALUES(@i,@v,@q,@a)",("@i",id),("@v",revision),("@q",q.Id),("@a",answers.GetValueOrDefault(q.Id,"").Trim()));
        Database.Audit(c,tx,User.Id,draft?"Save response draft":"Submit response",$"Response {id}; revision {revision}");tx.Commit();return id;
    }
    public void Review(int id,int expectedRevision,string expectedStatus,string decision,string comment)
    {
        Authorize(Roles.Supervisor,Roles.DepartmentAdmin);using var c=store.Db.Open();using var tx=c.BeginTransaction();var r=store.GetResponse(c,tx,id,true);
        if(r.Revision!=expectedRevision||r.Status!=expectedStatus)throw new InvalidOperationException("Another action changed this response. Refresh and reopen it.");
        string status=Rules.NextStatus(User,r,decision,comment.Trim());
        Database.Execute(c,tx,"INSERT INTO reviews(submission_id,revision,reviewer_id,stage,decision,comment) VALUES(@i,@v,@u,@s,@d,@c)",("@i",id),("@v",r.Revision),("@u",User.Id),("@s",r.Status),("@d",decision),("@c",comment.Trim()));
        Database.Execute(c,tx,"UPDATE submissions SET status=@s,first_reviewer_id=@first,updated_at=CURRENT_TIMESTAMP WHERE id=@i",("@s",status),("@first",r.Status=="Pending supervisor"&&decision=="Approve"?User.Id:r.FirstReviewer),("@i",id));
        Database.Audit(c,tx,User.Id,"Review response",$"Response {id}; {r.Status} -> {status}");tx.Commit();
    }
    public List<AuditRecord> AuditLog(){Authorize(Roles.SuperAdmin);return store.AuditLog();}
    public void ExportResponses(string path,string search)
    {
        Authorize(Roles.SuperAdmin,Roles.DepartmentAdmin,Roles.Viewer);
        var records=Responses().Where(r=>$"{r.Reference} {r.FormTitle} {r.Owner} {r.Status}".Contains(search,StringComparison.OrdinalIgnoreCase));
        static string Csv(string value){if(value.Length>0&&"=+-@\t\r".Contains(value[0]))value="'"+value;return "\""+value.Replace("\"","\"\"")+"\"";}
        var lines=new List<string>{"Reference,Form,Submitted by,Status,Revision,Updated at"};
        lines.AddRange(records.Select(r=>string.Join(',',new[]{r.Reference,r.FormTitle,r.Owner,r.Status,r.Revision.ToString(),r.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture)}.Select(Csv))));
        File.WriteAllLines(path,lines,new UTF8Encoding(true));
        using var c=store.Db.Open();using var tx=c.BeginTransaction();Database.Audit(c,tx,User.Id,"Export responses",$"Exported {lines.Count-1} visible response summaries");tx.Commit();
    }
}
