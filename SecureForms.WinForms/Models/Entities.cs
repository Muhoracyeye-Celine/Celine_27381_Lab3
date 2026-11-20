namespace SecureForms.Models;

public static class Roles
{
    public const string SuperAdmin = "Super Admin";
    public const string DepartmentAdmin = "Department Admin";
    public const string Supervisor = "Supervisor";
    public const string Staff = "Staff";
    public const string Viewer = "Management Viewer";
    public static readonly string[] All = { SuperAdmin, DepartmentAdmin, Supervisor, Staff, Viewer };
}

public record Session(int Id, string Name, string Role, int DepartmentId);
public record Department(int Id, string Name);
public record Account(int Id, string Name, string Email, string Role, int DepartmentId, bool Active);
public record FormRecord(int Id, string Title, string Description, int DepartmentId, string Status);
public record Question(int Id, string Label, string FieldType, string Options, bool Required, int Position);
public record ResponseRecord(int Id, string Reference, int FormId, string FormTitle, int OwnerId,
    string Owner, int DepartmentId, string Status, int Revision, int? FirstReviewer, DateTime UpdatedAt);
public record ReviewRecord(int Revision, string Reviewer, string Stage, string Decision, string Comment, DateTime Date);
public record AuditRecord(DateTime Date, string Actor, string Action, string Details);

public static class FieldTypes
{
    public static readonly string[] All = { "Short text", "Long text", "Number", "Date", "Dropdown", "Checkbox" };
}
