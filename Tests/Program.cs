using SecureForms.Controllers;
using SecureForms.Data;
using SecureForms.Models;
using SecureForms.Security;

int passed=0;
void Check(string name,Action action){action();Console.WriteLine("PASS "+name);passed++;}
void Reject(Action action){try{action();}catch(InvalidOperationException){return;}throw new Exception("Expected validation failure.");}
var staff=new Session(1,"Staff",Roles.Staff,1);
var supervisor=new Session(2,"Supervisor",Roles.Supervisor,1);
var admin=new Session(3,"Admin",Roles.DepartmentAdmin,1);
var response=new ResponseRecord(1,"COK-1",1,"Test",1,"Staff",1,"Pending supervisor",1,null,DateTime.Now);
Check("Password hashing and wrong password",()=>{var hash=Passwords.Hash("ValidPass123");if(!Passwords.Verify("ValidPass123",hash)||Passwords.Verify("wrong",hash))throw new Exception();});
Check("Password salt differs",()=>{if(Passwords.Hash("ValidPass123")==Passwords.Hash("ValidPass123"))throw new Exception();});
Check("Weak password refused",()=>Reject(()=>Passwords.Hash("short")));
Check("Supervisor approval only forwards",()=>{if(Rules.NextStatus(supervisor,response,"Approve","")!="Pending final approval")throw new Exception();});
var final=response with{Status="Pending final approval",FirstReviewer=2};
Check("Final approval",()=>{if(Rules.NextStatus(admin,final,"Approve","")!="Approved")throw new Exception();});
Check("Final approver cannot skip supervisor",()=>Reject(()=>Rules.NextStatus(admin,response,"Approve","")));
Check("Self review forbidden",()=>Reject(()=>Rules.NextStatus(staff with{Role=Roles.Supervisor},response,"Approve","")));
Check("Other department review forbidden",()=>Reject(()=>Rules.NextStatus(supervisor with{DepartmentId=2},response,"Approve","")));
Check("Same reviewer cannot finalize",()=>Reject(()=>Rules.NextStatus(admin with{Id=2},final,"Approve","")));
Check("Correction needs reason",()=>Reject(()=>Rules.NextStatus(supervisor,response,"Return","")));
Check("Rejection needs reason",()=>Reject(()=>Rules.NextStatus(supervisor,response,"Reject","")));
Check("Return transition",()=>{if(Rules.NextStatus(supervisor,response,"Return","Fix date")!="Returned for correction")throw new Exception();});
Check("Completed record cannot be reviewed",()=>Reject(()=>Rules.NextStatus(admin,final with{Status="Approved"},"Approve","")));
var qs=new List<Question>{new(1,"Age","Number","",true,1),new(2,"Date","Date","",true,2),new(3,"District","Dropdown","Gasabo|Kicukiro",true,3)};
var valid=new Dictionary<int,string>{{1,"20"},{2,"2026-11-19"},{3,"Gasabo"}};
Check("Valid dynamic answers",()=>Rules.ValidateAnswers(qs,valid,false));
Check("Incomplete draft allowed",()=>Rules.ValidateAnswers(qs,new(),true));
Check("Required validation",()=>Reject(()=>Rules.ValidateAnswers(qs,new(),false)));
Check("Invalid number refused",()=>Reject(()=>Rules.ValidateAnswers(qs,new(valid){[1]="abc"},false)));
Check("Invalid calendar date refused",()=>Reject(()=>Rules.ValidateAnswers(qs,new(valid){[2]="2026-02-30"},false)));
Check("Invented option refused",()=>Reject(()=>Rules.ValidateAnswers(qs,new(valid){[3]="Other"},false)));
Check("Foreign question refused",()=>Reject(()=>Rules.ValidateAnswers(qs,new(valid){[99]="Injection"},false)));

if(args.Contains("--integration"))
{
    string suffix=Guid.NewGuid().ToString("N")[..10];
    string dbName="lab3_test_"+suffix;
    string server=Environment.GetEnvironmentVariable("LAB3_TEST_SERVER")??"127.0.0.1";
    uint port=uint.Parse(Environment.GetEnvironmentVariable("LAB3_TEST_PORT")??"3306");
    string dbUser=Environment.GetEnvironmentVariable("LAB3_TEST_USER")??"root";
    string dbPass=Environment.GetEnvironmentVariable("LAB3_TEST_PASSWORD")??"";
    var settings=new DatabaseSettings(server,port,dbName,dbUser,dbPass);
    var db=new Database(settings);db.Initialize();var store=new Store(db);
    try
    {
        AppController.FirstAdmin(store,"Root","root@example.test","ValidPass123");
        var root=new AppController(store,AppController.Login(store,"root@example.test","ValidPass123"));
        int department=root.Departments().Single().Id;
        root.SaveAccount(0,"Department","admin@example.test",Roles.DepartmentAdmin,department,true,"ValidPass123");
        root.SaveAccount(0,"Supervisor","reviewer@example.test",Roles.Supervisor,department,true,"ValidPass123");
        root.SaveAccount(0,"Staff","staff@example.test",Roles.Staff,department,true,"ValidPass123");
        root.SaveAccount(0,"Viewer","viewer@example.test",Roles.Viewer,department,true,"ValidPass123");
        AppController SignIn(string email)=>new(store,AppController.Login(store,email,"ValidPass123"));
        var departmentAdmin=SignIn("admin@example.test");var reviewer=SignIn("reviewer@example.test");var employee=SignIn("staff@example.test");var viewer=SignIn("viewer@example.test");
        int formId=departmentAdmin.SaveForm(0,"Citizen request","Enter a request.");
        departmentAdmin.SaveQuestion(formId,0,"Request","Long text","",true,1);int q=departmentAdmin.Questions(formId).Single().Id;
        departmentAdmin.ChangeFormState(formId,"Published");
        Check("Database published form immutable",()=>Reject(()=>departmentAdmin.SaveForm(formId,"Changed","")));
        int id=employee.SaveResponse(formId,0,0,new(){{q,"Please check this request."}},false);
        Check("Database persistence after reconnect",()=>{if(SignIn("staff@example.test").Response(id).Status!="Pending supervisor")throw new Exception();});
        Check("Viewer cannot read pending record",()=>Reject(()=>viewer.Response(id)));
        reviewer.Review(id,1,"Pending supervisor","Return","Please add details");
        employee.SaveResponse(formId,id,1,new(){{q,"Updated with details."}},false);
        Check("Correction preserves revision",()=>{if(employee.Answers(id,1)[q]!="Please check this request."||employee.Answers(id,2)[q]!="Updated with details.")throw new Exception();});
        Check("Stale edit rejected",()=>Reject(()=>employee.SaveResponse(formId,id,1,new(){{q,"Stale"}},false)));
        reviewer.Review(id,2,"Pending supervisor","Approve","");
        Check("Stale review rejected",()=>Reject(()=>reviewer.Review(id,2,"Pending supervisor","Reject","Stale")));
        departmentAdmin.Review(id,2,"Pending final approval","Approve","");
        Check("Approved response visible to viewer",()=>{if(viewer.Response(id).Status!="Approved")throw new Exception();});
        Check("Two review stages stored",()=>{if(employee.Reviews(id).Count!=3)throw new Exception();});
        root.SaveDepartment(0,"Other Department");int other=root.Departments().Single(d=>d.Name=="Other Department").Id;
        root.SaveAccount(0,"Other Staff","other@example.test",Roles.Staff,other,true,"ValidPass123");
        Check("Database department isolation",()=>Reject(()=>SignIn("other@example.test").Response(id)));
        int draft=employee.SaveResponse(formId,0,0,new(),true);
        Check("Private draft hidden from supervisor",()=>{if(reviewer.Responses().Any(r=>r.Id==draft))throw new Exception();});
        int copy=departmentAdmin.DuplicateForm(formId);
        Check("Form duplicate copies questions",()=>{if(departmentAdmin.Questions(copy).Count!=1)throw new Exception();});
        departmentAdmin.DeleteForm(copy);
        Check("Draft delete persists",()=>{if(departmentAdmin.Forms().Any(f=>f.Id==copy))throw new Exception();});
        var account=root.Accounts().Single(a=>a.Email=="staff@example.test");root.SaveAccount(account.Id,account.Name,account.Email,account.Role,account.DepartmentId,false,"");
        Check("Deactivation blocks old session",()=>Reject(()=>employee.Responses()));
        Check("Audit trail persisted",()=>{if(!root.AuditLog().Any(a=>a.Action=="Review response"))throw new Exception();});
    }
    finally
    {
        using var c=new MySqlConnector.MySqlConnection(settings.ConnectionString(false));c.Open();
        using var cmd=new MySqlConnector.MySqlCommand($"DROP DATABASE `{dbName}`",c);cmd.ExecuteNonQuery();
    }
}
Console.WriteLine($"{passed} checks passed.");
