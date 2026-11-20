using System.Globalization;
using SecureForms.Models;

namespace SecureForms.Controllers;

public static class Rules
{
    public static string Text(string value, string label, int max)
    {
        value = value.Trim();
        if (value.Length == 0 || value.Length > max)
            throw new InvalidOperationException($"{label} is required and must not exceed {max} characters.");
        return value;
    }

    public static void ValidateAnswers(List<Question> questions, Dictionary<int, string> answers, bool draft)
    {
        if (answers.Keys.Any(id => questions.All(q => q.Id != id)))
            throw new InvalidOperationException("An answer does not belong to this form.");
        foreach (var question in questions)
        {
            string value = answers.GetValueOrDefault(question.Id, "").Trim();
            if (!draft && question.Required && value.Length == 0)
                throw new InvalidOperationException($"Please complete: {question.Label}");
            if (value.Length > 5000) throw new InvalidOperationException("An answer exceeds 5000 characters.");
            if (value.Length == 0) continue;
            if (question.FieldType == "Number" && !decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                throw new InvalidOperationException($"{question.Label} must be a number. Use a dot for decimals.");
            if (question.FieldType == "Date" && !DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                throw new InvalidOperationException($"{question.Label} must be a date in yyyy-MM-dd format.");
            if (question.FieldType == "Dropdown" && !question.Options.Split('|').Contains(value))
                throw new InvalidOperationException($"Select an available option for {question.Label}.");
            if (question.FieldType == "Checkbox" && value is not "Yes" and not "No")
                throw new InvalidOperationException("Invalid checkbox answer.");
            if (!draft && question.Required && question.FieldType == "Checkbox" && value != "Yes")
                throw new InvalidOperationException($"Please check: {question.Label}");
        }
    }

    public static string NextStatus(Session user, ResponseRecord record, string decision, string comment)
    {
        if (user.DepartmentId != record.DepartmentId) throw new InvalidOperationException("This response belongs to another department.");
        if (user.Id == record.OwnerId) throw new InvalidOperationException("You cannot review your own response.");
        bool first = record.Status == "Pending supervisor" && user.Role == Roles.Supervisor;
        bool final = record.Status == "Pending final approval" && user.Role == Roles.DepartmentAdmin;
        if (!first && !final) throw new InvalidOperationException("You cannot review this response at its current stage.");
        if (final && record.FirstReviewer == user.Id) throw new InvalidOperationException("The final approver must differ from the first reviewer.");
        if (decision is not "Approve" and not "Reject" and not "Return") throw new InvalidOperationException("Choose a valid decision.");
        if (decision != "Approve") Text(comment, "A reason", 2000);
        if (comment.Length > 2000) throw new InvalidOperationException("The comment is too long.");
        return decision switch { "Reject" => "Rejected", "Return" => "Returned for correction", _ => first ? "Pending final approval" : "Approved" };
    }
}
