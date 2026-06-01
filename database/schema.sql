/* =============================================================================
   Task Management - Database Schema
   -----------------------------------------------------------------------------
   Target  : Microsoft SQL Server (2017+ / SQL Server Express)
   Purpose : Creates the TaskManagementDb database and its tables, indexes and
             EF Core migration-history bookkeeping. This script is idempotent and
             can be run repeatedly.

   NOTE: The application also creates and migrates this schema automatically on
         startup (see ApplicationDbContextInitialiser). This script is provided
         as the standalone database deliverable. Run seed.sql afterwards to add
         the default user and sample tasks.

   Run with: sqlcmd -S localhost -E -i schema.sql
   ============================================================================= */

IF DB_ID(N'TaskManagementDb') IS NULL
BEGIN
    CREATE DATABASE [TaskManagementDb];
END;
GO

USE [TaskManagementDb];
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531102314_InitialCreate'
)
BEGIN
    CREATE TABLE [Tasks] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [IsCompleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [Priority] int NOT NULL,
        [DueDateUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Tasks] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531102314_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Username] nvarchar(100) NOT NULL,
        [DisplayName] nvarchar(150) NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531102314_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tasks_DueDateUtc] ON [Tasks] ([DueDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531102314_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tasks_IsCompleted] ON [Tasks] ([IsCompleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531102314_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tasks_Priority] ON [Tasks] ([Priority]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531102314_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260531102314_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260531102314_InitialCreate', N'9.0.8');
END;

COMMIT;
GO
