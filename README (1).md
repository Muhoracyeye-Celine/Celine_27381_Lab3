# Kigali City Secure Digital Form and Data Management System

**Name:** Celine Muhoracyeye  
**Student ID:** 27381  
**Assignment:** Lab 3

## Project overview

This desktop application supports digital form creation, departmental data collection, submission review, and record management. It uses C# Windows Forms for the interface and MySQL for persistent storage. Users interact with the database through the graphical interface.

The application follows the Model-View-Controller (MVC) architecture, with a separate data access layer for database operations.

## Main features

- Department and user account management.
- Role-based and department-based access.
- Dynamic forms with short text, long text, number, date, dropdown, and checkbox fields.
- Draft form editing, publishing, duplication, and archiving.
- Response drafts and required-field validation.
- Supervisor review and final departmental approval.
- Correction requests with previous answer revisions and review history.
- Dashboards, submission search, CSV summary exports, and audit logs.
- Salted password hashing and parameterized SQL queries.

## User roles

| Role | Responsibilities |
| --- | --- |
| Super Admin | Manage departments and user accounts, assign roles, and view audit history. |
| Department Admin | Create and publish departmental forms, manage submissions, and give final approval. |
| Supervisor | Perform the first review, approve for the next stage, reject, or request corrections. |
| Staff | Complete published forms in their department, save drafts, submit responses, and make requested corrections. |
| Management Viewer | View approved departmental submissions and export permitted summaries. |

Staff can access published forms in their own department. The Department Admin, Supervisor, and Staff accounts involved in a review workflow must belong to the same department.

## Technologies

- C# and .NET 8.
- Windows Forms.
- MySQL-compatible database.
- MySqlConnector 2.6.2.
- Git and GitHub.

## MVC structure

| Component | Location | Purpose |
| --- | --- | --- |
| Models | `SecureForms.WinForms/Models` | Define the data structures for accounts, departments, forms, questions, submissions, and reviews. |
| Views | `SecureForms.WinForms/Views` | Display screens, collect input, and show results. |
| Controllers | `SecureForms.WinForms/Controllers` | Validate actions, enforce permissions, and manage the submission workflow. |
| Data access | `SecureForms.WinForms/Data` | Manage connections, execute SQL, and retrieve or save records. |
| Security | `SecureForms.WinForms/Security` | Hash and verify passwords. |
| Entry point | `SecureForms.WinForms/Program.cs` | Start the application and open the connection, login, and workspace screens. |

For example, submitting a response calls the controller from the response-entry view. The controller validates the answers and permissions, then uses the data access layer to save the response and its audit record.

## Database structure

The database contains eight related tables:

| Table | Purpose |
| --- | --- |
| `departments` | Department names. |
| `users` | Account details, password hashes, roles, department membership, and active status. |
| `forms` | Form titles, instructions, departments, creators, and publication status. |
| `questions` | Field definitions, choices, required flags, and display order. |
| `submissions` | Response references, owners, current revisions, and workflow status. |
| `answers` | Answers stored by submission, revision, and question. |
| `reviews` | Reviewers, stages, decisions, comments, and decision times. |
| `audit_logs` | Recorded application actions. |

The schema is included in `Database/schema.sql`. The application creates the database and tables during initialization if they do not already exist.

## Requirements

- Windows 10 or 11.
- Visual Studio with the **.NET desktop development** workload and the **.NET 8 SDK**.
- A running MySQL or MariaDB server.
- Internet access for the first NuGet package restore.

## Installation and running

1. Clone or download this repository.
2. Start the database server. If an existing service such as **MySQL80** is already running, use that service.
3. Open `Celine_27381_Lab3.sln` in Visual Studio.
4. Set `SecureForms.WinForms` as the startup project if necessary.
5. Restore NuGet packages and build the solution.
6. Press **F5** to run the application.
7. Enter the database connection details:

| Setting | Value |
| --- | --- |
| Server | `127.0.0.1` for a local server |
| Port | `3306`, or the configured database port |
| Database | `secure_forms_lab3` |
| Username | A local database account with permission to create the database and tables |
| Password | The password for that database account |

8. Click **Connect to workspace**.
9. For a new database, create the first Super Admin account. For an existing database, sign in with an existing application account.
10. Use **People & Access** to add the other role accounts and assign their departments.

Database credentials and application login credentials are separate. Local connection settings are stored in `%LOCALAPPDATA%\CelineSecureFormsLab3\database.json`; this file should remain outside the repository.

The application can also be built and started from the solution folder:

```powershell
dotnet restore Celine_27381_Lab3.sln
dotnet build Celine_27381_Lab3.sln -c Release
dotnet run --project SecureForms.WinForms
```

## Form and approval workflow

1. The Department Admin creates a form and adds its questions.
2. The Department Admin selects **More actions > Publish form**.
3. Staff in the same department open the form and submit their answers.
4. The Supervisor opens **Submissions**, selects the response, and chooses **Open / review**.
5. Supervisor approval moves the response to **Pending final approval**.
6. The Department Admin reviews it and gives the final decision.
7. Approved responses become visible to the Management Viewer.

A correction request returns the response to Staff. Resubmitting creates a new answer revision and restarts Supervisor review. Earlier answers and decisions remain available in the history. Reviewers cannot approve their own submissions.

Published form questions cannot be edited directly. A form can be duplicated into a new draft when its definition needs to change.

## Checks

The console checks can be run with:

```powershell
dotnet run --project Tests
```

They cover password handling, answer validation, approval stages, department restrictions, and review rules.

## Desktop scope

This lab implements the internal desktop workflow. Public browser forms, email OTP, file attachments, GPS capture, and PDF reports are not included. CSV exports contain submission summaries rather than all dynamic answers.
