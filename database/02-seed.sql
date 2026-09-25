/* =====================================================================
   02-seed.sql  —  DML (Data Manipulation Language): manipula os DADOS.
   INSERT de dados de demonstração. Executado pela API se a tabela Clientes estiver vazia.
   Pré-requisito: usuários já criados (a API cria com senha em BCrypt antes de rodar este script).
   Os nomes de empresas abaixo são FICTÍCIOS.
   ===================================================================== */

SET NOCOUNT ON;

-- Variáveis T-SQL: guardamos os Ids dos usuários pelo e-mail
DECLARE @Admin  INT = (SELECT Id FROM dbo.Usuarios WHERE Email = 'admin@servicehub.local');
DECLARE @Ana    INT = (SELECT Id FROM dbo.Usuarios WHERE Email = 'ana@servicehub.local');
DECLARE @Bruno  INT = (SELECT Id FROM dbo.Usuarios WHERE Email = 'bruno@servicehub.local');

-- Transação: ou tudo é gravado, ou nada (atomicidade)
BEGIN TRY
    BEGIN TRANSACTION;

    /* ---------- INSERT: clientes (um de cada setor atendido) ---------- */
    INSERT INTO dbo.Clientes (Nome, Documento, Email, Telefone, Setor)
    VALUES
        (N'Rede Sol Supermercados', '12.345.678/0001-90', 'ti@redesol.com.br',        '(11) 3000-1000', N'Varejo'),
        (N'Clínica Vida Plena',     '23.456.789/0001-01', 'sistemas@vidaplena.com.br', '(11) 3000-2000', N'Saúde'),
        (N'Banco Horizonte',        '34.567.890/0001-12', 'tecnologia@horizonte.com',  '(11) 3000-3000', N'Finanças'),
        (N'Loja Pixel Games',       '45.678.901/0001-23', 'contato@pixelgames.com.br', NULL,             N'Varejo');

    DECLARE @RedeSol   INT = (SELECT Id FROM dbo.Clientes WHERE Documento = '12.345.678/0001-90');
    DECLARE @Clinica   INT = (SELECT Id FROM dbo.Clientes WHERE Documento = '23.456.789/0001-01');
    DECLARE @Banco     INT = (SELECT Id FROM dbo.Clientes WHERE Documento = '34.567.890/0001-12');

    /* ---------- INSERT: projetos ---------- */
    INSERT INTO dbo.Projetos (ClienteId, Nome, Descricao, Status, ResponsavelId)
    VALUES
        (@RedeSol, N'E-commerce B2C',             N'Loja virtual integrada ao ERP e marketplaces', 'EmAndamento', @Ana),
        (@RedeSol, N'App de Fidelidade',          N'Programa de pontos no aplicativo',             'Planejado',   @Bruno),
        (@Clinica, N'Prontuário Eletrônico',      N'Anamnese e histórico do paciente',             'EmAndamento', @Bruno),
        (@Banco,   N'API Open Finance',           N'APIs de consentimento e consulta de contas',   'EmAndamento', @Admin),
        (@Banco,   N'Observabilidade Core',       N'Logs, métricas e traces do core bancário',     'Pausado',     @Ana);

    DECLARE @Ecommerce  INT = (SELECT Id FROM dbo.Projetos WHERE Nome = N'E-commerce B2C');
    DECLARE @Prontuario INT = (SELECT Id FROM dbo.Projetos WHERE Nome = N'Prontuário Eletrônico');
    DECLARE @OpenFin    INT = (SELECT Id FROM dbo.Projetos WHERE Nome = N'API Open Finance');

    /* ---------- INSERT: chamados ---------- */
    INSERT INTO dbo.Chamados (Titulo, Descricao, ClienteId, ProjetoId, ResponsavelId, Prioridade, Status)
    VALUES
        (N'Checkout lento na Black Friday',   N'Tempo de resposta acima de 5s no fechamento do pedido.', @RedeSol, @Ecommerce,  @Ana,   'Critica', 'EmAndamento'),
        (N'Estoque divergente com o ERP',     N'Produtos aparecem sem estoque no site.',                 @RedeSol, @Ecommerce,  @Ana,   'Alta',    'Aberto'),
        (N'Erro ao salvar anamnese',          N'Campo de alergias não é gravado.',                       @Clinica, @Prontuario, @Bruno, 'Alta',    'Aberto'),
        (N'Relatório mensal de atendimentos', N'Criar exportação em Excel.',                             @Clinica, @Prontuario, NULL,   'Baixa',   'Aberto'),
        (N'Token de consentimento expirando', N'Consentimentos expiram antes do prazo.',                 @Banco,   @OpenFin,    @Admin, 'Critica', 'Aberto'),
        (N'Ajuste de layout no extrato',      N'Colunas desalinhadas no mobile.',                        @Banco,   NULL,        @Bruno, 'Media',   'Concluido'),
        (N'Dúvida sobre limite de API',       N'Cliente pediu documentação de rate limit.',             @Banco,   @OpenFin,    NULL,   'Baixa',   'Cancelado');

    /* ---------- UPDATE: completa a data de conclusão dos chamados concluídos ---------- */
    UPDATE dbo.Chamados
    SET ConcluidoEm = SYSUTCDATETIME(),
        AtualizadoEm = SYSUTCDATETIME()
    WHERE Status = 'Concluido' AND ConcluidoEm IS NULL;

    /* ---------- INSERT ... SELECT: auditoria de criação para todos os chamados ---------- */
    INSERT INTO dbo.Auditoria (Entidade, EntidadeId, Acao, Detalhes, UsuarioId)
    SELECT 'Chamado', Id, 'Criacao', CONCAT(N'Chamado aberto com prioridade ', Prioridade, N' (carga inicial).'), @Admin
    FROM dbo.Chamados;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;   -- relança o erro original (a API vai registrar no log)
END CATCH;
GO
