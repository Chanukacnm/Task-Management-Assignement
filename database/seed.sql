/* =============================================================================
   Task Management - Seed Data
   -----------------------------------------------------------------------------
   Inserts the default user and a few sample tasks. Idempotent: existing rows are
   left untouched. Run after schema.sql.

   Default login:  admin  /  Passw0rd!
   The PasswordHash below is a PBKDF2-SHA256 hash in the form
   "iterations;saltBase64;hashBase64" and matches the password "Passw0rd!".

   Priority column values: 0 = Low, 1 = Medium, 2 = High

   Run with: sqlcmd -S localhost -E -i seed.sql
   ============================================================================= */

USE [TaskManagementDb];
GO

-- Default user -----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Username] = N'admin')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [DisplayName], [PasswordHash], [CreatedAtUtc])
    VALUES (
        N'admin',
        N'Administrator',
        N'600000;zto2bUNxtF1ZuE5Aa97WfQ==;k4tu7PxZSu6hYAc1txcR+K85dwHrYEB0om23N9ck+B4=',
        SYSUTCDATETIME()
    );
END;
GO

-- Sample tasks (only when the table is empty) ----------------------------------
IF NOT EXISTS (SELECT 1 FROM [dbo].[Tasks])
BEGIN
    INSERT INTO [dbo].[Tasks]
        ([Title], [Description], [IsCompleted], [Priority], [DueDateUtc], [CreatedAtUtc], [UpdatedAtUtc])
    VALUES
        (N'Set up project repository',
         N'Initialise the Git repository and push the initial commit.',
         1, 2, DATEADD(DAY, -3, SYSUTCDATETIME()), DATEADD(DAY, -5, SYSUTCDATETIME()), DATEADD(DAY, -3, SYSUTCDATETIME())),

        (N'Design the database schema',
         N'Model tasks and users, then create the EF Core migrations.',
         1, 1, DATEADD(DAY, -1, SYSUTCDATETIME()), DATEADD(DAY, -4, SYSUTCDATETIME()), DATEADD(DAY, -1, SYSUTCDATETIME())),

        (N'Implement the Tasks CRUD API',
         N'Expose create, read, update, delete and complete endpoints.',
         0, 2, DATEADD(DAY, 2, SYSUTCDATETIME()), DATEADD(DAY, -2, SYSUTCDATETIME()), DATEADD(DAY, -2, SYSUTCDATETIME())),

        (N'Build the Angular UI',
         N'List and edit tasks side by side with sorting and filtering.',
         0, 1, DATEADD(DAY, 5, SYSUTCDATETIME()), DATEADD(DAY, -1, SYSUTCDATETIME()), DATEADD(DAY, -1, SYSUTCDATETIME())),

        (N'Write the project documentation',
         N'Document setup, configuration and how to run the application.',
         0, 0, DATEADD(DAY, 7, SYSUTCDATETIME()), SYSUTCDATETIME(), SYSUTCDATETIME());
END;
GO
