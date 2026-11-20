using MySqlConnector;
using SecureForms.Models;

namespace SecureForms.Data;

public sealed class Store
{
    public Database Db { get; }
    public Store(Database db) => Db = db;

    public List<T> Query<T>(string sql, Func<MySqlDataReader, T> map, params (string, object?)[] args)
    {
        using var c = Db.Open(); using var cmd = Database.Command(c, null, sql, args);
        using var r = cmd.ExecuteReader(); var items = new List<T>();
        while (r.Read()) items.Add(map(r)); return items;
    }
    public bool HasAccounts() => Query("SELECT COUNT(*) FROM users", r => r.GetInt32(0))[0] > 0;
    public List<Department> Departments() => Query("SELECT id,name FROM departments ORDER BY name", r => new Department(r.GetInt32(0), r.GetString(1)));
    public List<Account> Accounts() => Query("SELECT id,full_name,email,role,department_id,active FROM users ORDER BY full_name", r => new Account(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetInt32(4), r.GetBoolean(5)));
    public (Session User, string Hash)? Credentials(string email)
    {
        var list = Query("SELECT id,full_name,role,department_id,password_hash FROM users WHERE email=@e AND active=TRUE", r =>
            (new Session(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetInt32(3)), r.GetString(4)), ("@e", email));
        return list.Count == 0 ? null : list[0];
    }
    public bool IsCurrent(Session user) => Query("SELECT COUNT(*) FROM users WHERE id=@i AND active=TRUE AND role=@r AND department_id=@d", r => r.GetInt32(0), ("@i", user.Id), ("@r", user.Role), ("@d", user.DepartmentId))[0] == 1;
    public List<FormRecord> Forms(Session u) => Query("SELECT id,title,description,department_id,status FROM forms WHERE (@all=TRUE OR department_id=@d) ORDER BY id DESC", MapForm, ("@all", u.Role == Roles.SuperAdmin), ("@d", u.DepartmentId));
    public static FormRecord MapForm(MySqlDataReader r) => new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetInt32(3), r.GetString(4));
    public FormRecord GetForm(MySqlConnection c, MySqlTransaction? tx, int id, bool locked = false)
    {
        using var cmd = Database.Command(c, tx, "SELECT id,title,description,department_id,status FROM forms WHERE id=@i" + (locked ? " FOR UPDATE" : ""), ("@i", id));
        using var r = cmd.ExecuteReader();
        if (!r.Read()) throw new InvalidOperationException("Form not found.");
        return MapForm(r);
    }
    public List<Question> Questions(int formId) => Query("SELECT id,label,field_type,options_text,required,position FROM questions WHERE form_id=@f ORDER BY position,id", r => new Question(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetBoolean(4),r.GetInt32(5)), ("@f",formId));
    public static ResponseRecord MapResponse(MySqlDataReader r) => new(r.GetInt32(0),r.GetString(1),r.GetInt32(2),r.GetString(3),r.GetInt32(4),r.GetString(5),r.GetInt32(6),r.GetString(7),r.GetInt32(8),r.IsDBNull(9)?null:r.GetInt32(9),r.GetDateTime(10));
    private const string ResponseSql = "SELECT s.id,s.reference_no,s.form_id,f.title,s.submitted_by,u.full_name,f.department_id,s.status,s.revision,s.first_reviewer_id,s.updated_at FROM submissions s JOIN forms f ON f.id=s.form_id JOIN users u ON u.id=s.submitted_by ";
    public List<ResponseRecord> Responses(Session u) => Query(ResponseSql +
        "WHERE ((@role='Super Admin' AND s.status<>'Draft') OR (f.department_id=@d AND ((@role='Staff' AND s.submitted_by=@u) OR (@role IN ('Supervisor','Department Admin') AND (s.status<>'Draft' OR s.submitted_by=@u)) OR (@role='Management Viewer' AND s.status='Approved')))) ORDER BY s.updated_at DESC,s.id DESC", MapResponse, ("@role",u.Role),("@d",u.DepartmentId),("@u",u.Id));
    public ResponseRecord GetResponse(MySqlConnection c, MySqlTransaction? tx, int id, bool locked = false)
    {
        using var cmd = Database.Command(c,tx,ResponseSql + "WHERE s.id=@i"+(locked?" FOR UPDATE":""),("@i",id));
        using var r=cmd.ExecuteReader();
        if(!r.Read())throw new InvalidOperationException("Response not found.");
        return MapResponse(r);
    }
    public Dictionary<int,string> Answers(int id,int revision) => Query("SELECT question_id,answer_text FROM answers WHERE submission_id=@i AND revision=@v",r=>(r.GetInt32(0),r.GetString(1)),("@i",id),("@v",revision)).ToDictionary(x=>x.Item1,x=>x.Item2);
    public List<int> Revisions(int id) => Query("SELECT DISTINCT revision FROM answers WHERE submission_id=@i ORDER BY revision DESC",r=>r.GetInt32(0),("@i",id));
    public List<ReviewRecord> Reviews(int id) => Query("SELECT r.revision,u.full_name,r.stage,r.decision,r.comment,r.created_at FROM reviews r JOIN users u ON u.id=r.reviewer_id WHERE submission_id=@i ORDER BY r.id",r=>new ReviewRecord(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetDateTime(5)),("@i",id));
    public List<AuditRecord> AuditLog() => Query("SELECT a.created_at,COALESCE(u.full_name,'Setup'),a.action,a.details FROM audit_logs a LEFT JOIN users u ON u.id=a.user_id ORDER BY a.id DESC LIMIT 1000",r=>new AuditRecord(r.GetDateTime(0),r.GetString(1),r.GetString(2),r.GetString(3)));
}
