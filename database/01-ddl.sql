/* =====================================================================
   01-ddl.sql  —  DDL (Data Definition Language): define a ESTRUTURA.
   CREATE TABLE, constraints (PK, FK, UNIQUE, CHECK, DEFAULT), índices e procedure.

   Script IDEMPOTENTE: pode rodar várias vezes sem erro (IF OBJECT_ID ... IS NULL).
   Executado automaticamente pela API ao subir (DatabaseInitializer.cs).

   Modelo:
     Usuarios 1───N Projetos (responsável)
     Clientes 1───N Projetos
     Clientes 1───N Chamados
     Projetos 1───N Chamados (opcional)
     Usuarios 1───N Chamados (responsável, opcional)
     Auditoria  -> registra ações em qualquer entidade (Entidade + EntidadeId)
   ===================================================================== */

/* ---------------------------- USUARIOS ---------------------------- */
IF OBJECT_ID('dbo.Usuarios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios
    (
        Id         INT IDENTITY(1,1) NOT NULL,          -- IDENTITY = auto incremento (AUTO_INCREMENT do MySQL)
        Nome       NVARCHAR(100)     NOT NULL,          -- NVARCHAR = texto Unicode (acentos)
        Email      NVARCHAR(150)     NOT NULL,
        SenhaHash  NVARCHAR(200)     NOT NULL,
        Perfil     NVARCHAR(20)      NOT NULL,
        Ativo      BIT               NOT NULL CONSTRAINT DF_Usuarios_Ativo    DEFAULT (1),
        CriadoEm   DATETIME2         NOT NULL CONSTRAINT DF_Usuarios_CriadoEm DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Usuarios        PRIMARY KEY (Id),
        CONSTRAINT UQ_Usuarios_Email  UNIQUE (Email),
        CONSTRAINT CK_Usuarios_Perfil CHECK (Perfil IN ('Admin', 'Analista'))
    );
END
GO

/* ---------------------------- CLIENTES ---------------------------- */
IF OBJECT_ID('dbo.Clientes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes
    (
        Id           INT IDENTITY(1,1) NOT NULL,
        Nome         NVARCHAR(150)     NOT NULL,
        Documento    NVARCHAR(18)      NOT NULL,        -- CNPJ formatado: 00.000.000/0000-00
        Email        NVARCHAR(150)     NOT NULL,
        Telefone     NVARCHAR(20)      NULL,
        Setor        NVARCHAR(50)      NOT NULL,
        CriadoEm     DATETIME2         NOT NULL CONSTRAINT DF_Clientes_CriadoEm DEFAULT (SYSUTCDATETIME()),
        AtualizadoEm DATETIME2         NULL,

        CONSTRAINT PK_Clientes           PRIMARY KEY (Id),
        CONSTRAINT UQ_Clientes_Documento UNIQUE (Documento)
    );
END
GO

/* ---------------------------- PROJETOS ---------------------------- */
IF OBJECT_ID('dbo.Projetos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Projetos
    (
        Id            INT IDENTITY(1,1) NOT NULL,
        ClienteId     INT               NOT NULL,
        Nome          NVARCHAR(150)     NOT NULL,
        Descricao     NVARCHAR(1000)    NULL,
        Status        NVARCHAR(20)      NOT NULL CONSTRAINT DF_Projetos_Status   DEFAULT ('Planejado'),
        ResponsavelId INT               NOT NULL,
        CriadoEm      DATETIME2         NOT NULL CONSTRAINT DF_Projetos_CriadoEm DEFAULT (SYSUTCDATETIME()),
        AtualizadoEm  DATETIME2         NULL,

        CONSTRAINT PK_Projetos PRIMARY KEY (Id),
        -- FOREIGN KEY: garante que ClienteId existe em Clientes (integridade referencial)
        CONSTRAINT FK_Projetos_Clientes    FOREIGN KEY (ClienteId)     REFERENCES dbo.Clientes (Id),
        CONSTRAINT FK_Projetos_Responsavel FOREIGN KEY (ResponsavelId) REFERENCES dbo.Usuarios (Id),
        CONSTRAINT CK_Projetos_Status CHECK (Status IN ('Planejado', 'EmAndamento', 'Pausado', 'Concluido'))
    );
END
GO

/* ---------------------------- CHAMADOS ---------------------------- */
IF OBJECT_ID('dbo.Chamados', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Chamados
    (
        Id            INT IDENTITY(1,1) NOT NULL,
        Titulo        NVARCHAR(150)     NOT NULL,
        Descricao     NVARCHAR(2000)    NOT NULL,
        ClienteId     INT               NOT NULL,
        ProjetoId     INT               NULL,
        ResponsavelId INT               NULL,
        Prioridade    NVARCHAR(10)      NOT NULL CONSTRAINT DF_Chamados_Prioridade DEFAULT ('Media'),
        Status        NVARCHAR(20)      NOT NULL CONSTRAINT DF_Chamados_Status     DEFAULT ('Aberto'),
        CriadoEm      DATETIME2         NOT NULL CONSTRAINT DF_Chamados_CriadoEm   DEFAULT (SYSUTCDATETIME()),
        AtualizadoEm  DATETIME2         NULL,
        ConcluidoEm   DATETIME2         NULL,

        CONSTRAINT PK_Chamados PRIMARY KEY (Id),
        CONSTRAINT FK_Chamados_Clientes    FOREIGN KEY (ClienteId)     REFERENCES dbo.Clientes (Id),
        CONSTRAINT FK_Chamados_Projetos    FOREIGN KEY (ProjetoId)     REFERENCES dbo.Projetos (Id),
        CONSTRAINT FK_Chamados_Responsavel FOREIGN KEY (ResponsavelId) REFERENCES dbo.Usuarios (Id),
        CONSTRAINT CK_Chamados_Prioridade CHECK (Prioridade IN ('Baixa', 'Media', 'Alta', 'Critica')),
        CONSTRAINT CK_Chamados_Status     CHECK (Status IN ('Aberto', 'EmAndamento', 'Concluido', 'Cancelado'))
    );
END
GO

/* ---------------------------- AUDITORIA ---------------------------- */
IF OBJECT_ID('dbo.Auditoria', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Auditoria
    (
        Id         BIGINT IDENTITY(1,1) NOT NULL,       -- BIGINT: tabela de log cresce muito
        Entidade   NVARCHAR(50)         NOT NULL,       -- 'Cliente', 'Projeto', 'Chamado'
        EntidadeId INT                  NOT NULL,
        Acao       NVARCHAR(30)         NOT NULL,       -- 'Criacao', 'Atualizacao', 'AlteracaoStatus', 'Exclusao'
        Detalhes   NVARCHAR(MAX)        NULL,
        UsuarioId  INT                  NULL,
        DataHora   DATETIME2            NOT NULL CONSTRAINT DF_Auditoria_DataHora DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Auditoria PRIMARY KEY (Id),
        CONSTRAINT FK_Auditoria_Usuarios FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios (Id)
        -- Sem FK para EntidadeId: é uma referência "polimórfica" (aponta para tabelas diferentes)
        -- e o registro de auditoria deve sobreviver mesmo se a entidade for excluída.
    );
END
GO

/* ---------------------------- ÍNDICES ----------------------------
   A PK já cria um índice CLUSTERED (a tabela fica fisicamente ordenada pelo Id).
   O SQL Server NÃO cria índice automático para FK: criamos os NONCLUSTERED abaixo
   para acelerar JOINs e filtros usados pela aplicação.                              */

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Projetos_ClienteId')
    CREATE NONCLUSTERED INDEX IX_Projetos_ClienteId ON dbo.Projetos (ClienteId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Chamados_ClienteId')
    CREATE NONCLUSTERED INDEX IX_Chamados_ClienteId ON dbo.Chamados (ClienteId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Chamados_ProjetoId')
    CREATE NONCLUSTERED INDEX IX_Chamados_ProjetoId ON dbo.Chamados (ProjetoId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Chamados_ResponsavelId')
    CREATE NONCLUSTERED INDEX IX_Chamados_ResponsavelId ON dbo.Chamados (ResponsavelId);
GO
-- Índice composto: atende filtros "WHERE Status = ? AND Prioridade = ?" (a ordem das colunas importa)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Chamados_Status_Prioridade')
    CREATE NONCLUSTERED INDEX IX_Chamados_Status_Prioridade ON dbo.Chamados (Status, Prioridade) INCLUDE (CriadoEm);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Auditoria_Entidade')
    CREATE NONCLUSTERED INDEX IX_Auditoria_Entidade ON dbo.Auditoria (Entidade, EntidadeId) INCLUDE (DataHora);
GO

/* ---------------------------- STORED PROCEDURE ----------------------------
   T-SQL: lógica guardada no banco. Chamada pelo DashboardRepository via Dapper.
   Devolve DOIS result sets: (1) totais, (2) chamados por prioridade.            */
CREATE OR ALTER PROCEDURE dbo.sp_ObterDashboard
AS
BEGIN
    SET NOCOUNT ON;   -- não envia a mensagem "(n rows affected)"; boa prática em procedures

    -- Result set 1: totais (subconsultas escalares)
    SELECT
        (SELECT COUNT(*) FROM dbo.Clientes)                                AS TotalClientes,
        (SELECT COUNT(*) FROM dbo.Projetos)                                AS TotalProjetos,
        (SELECT COUNT(*) FROM dbo.Chamados WHERE Status = 'Aberto')       AS ChamadosAbertos,
        (SELECT COUNT(*) FROM dbo.Chamados WHERE Status = 'EmAndamento')  AS ChamadosEmAndamento,
        (SELECT COUNT(*) FROM dbo.Chamados WHERE Status = 'Concluido')    AS ChamadosConcluidos;

    -- Result set 2: GROUP BY + agregação condicional (SUM com CASE)
    SELECT
        Prioridade,
        COUNT(*)                                                               AS Total,
        SUM(CASE WHEN Status IN ('Aberto', 'EmAndamento') THEN 1 ELSE 0 END)   AS EmAberto
    FROM dbo.Chamados
    GROUP BY Prioridade
    ORDER BY CASE Prioridade WHEN 'Critica' THEN 1 WHEN 'Alta' THEN 2 WHEN 'Media' THEN 3 ELSE 4 END;
END
GO
