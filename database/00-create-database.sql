/* =====================================================================
   00-create-database.sql
   Só para execução MANUAL (SSMS, Azure Data Studio, sqlcmd).
   A API já cria o banco sozinha ao subir (DatabaseInitializer.cs).
   ===================================================================== */

IF DB_ID('TimewareServiceHub') IS NULL
    CREATE DATABASE TimewareServiceHub;
GO

USE TimewareServiceHub;
GO

-- Depois rode, nesta ordem: 01-ddl.sql, e (após a API criar os usuários) 02-seed.sql
