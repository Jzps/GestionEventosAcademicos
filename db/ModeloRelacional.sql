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
    WHERE [MigrationId] = N'20261019181438_EsquemaInicial'
)
BEGIN
    CREATE TABLE [Categorias] (
        [Id] bigint NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(500) NOT NULL,
        CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019181438_EsquemaInicial'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] bigint NOT NULL IDENTITY,
        [NombreCompleto] nvarchar(200) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [ContrasenaHash] nvarchar(400) NOT NULL,
        [Rol] nvarchar(20) NOT NULL,
        [Activo] bit NOT NULL,
        [Codigo] nvarchar(50) NULL,
        [Programa] nvarchar(150) NULL,
        [Dependencia] nvarchar(150) NULL,
        [Cargo] nvarchar(150) NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Usuarios_Perfil] CHECK (([Rol] = 'Estudiante' AND [Codigo] IS NOT NULL AND [Programa] IS NOT NULL AND [Dependencia] IS NULL AND [Cargo] IS NULL) OR ([Rol] = 'Responsable' AND [Codigo] IS NULL AND [Programa] IS NULL AND [Dependencia] IS NOT NULL AND [Cargo] IS NOT NULL)),
        CONSTRAINT [CK_Usuarios_Rol] CHECK ([Rol] IN ('Estudiante', 'Responsable'))
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019181438_EsquemaInicial'
)
BEGIN
    CREATE TABLE [Eventos] (
        [Id] bigint NOT NULL IDENTITY,
        [Nombre] nvarchar(200) NOT NULL,
        [Descripcion] nvarchar(2000) NOT NULL,
        [FechaHoraInicio] datetime2 NOT NULL,
        [FechaHoraFin] datetime2 NOT NULL,
        [Lugar] nvarchar(200) NOT NULL,
        [CuposTotales] int NOT NULL,
        [Estado] int NOT NULL,
        [FechaRegistro] datetime2 NOT NULL,
        [ResponsableId] bigint NOT NULL,
        [CategoriaId] bigint NOT NULL,
        [Version] rowversion NULL,
        CONSTRAINT [PK_Eventos] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Eventos_Cupos] CHECK ([CuposTotales] > 0),
        CONSTRAINT [CK_Eventos_Fechas] CHECK ([FechaHoraFin] > [FechaHoraInicio]),
        CONSTRAINT [FK_Eventos_Categorias_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Eventos_Usuarios_ResponsableId] FOREIGN KEY ([ResponsableId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019181438_EsquemaInicial'
)
BEGIN
    CREATE TABLE [Inscripciones] (
        [Id] bigint NOT NULL IDENTITY,
        [EstudianteId] bigint NOT NULL,
        [EventoId] bigint NOT NULL,
        [FechaHoraRegistro] datetime2 NOT NULL,
        [Estado] int NOT NULL,
        [FechaHoraCancelacion] datetime2 NULL,
        CONSTRAINT [PK_Inscripciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Inscripciones_Eventos_EventoId] FOREIGN KEY ([EventoId]) REFERENCES [Eventos] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Inscripciones_Usuarios_EstudianteId] FOREIGN KEY ([EstudianteId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019181438_EsquemaInicial'
)
BEGIN
    CREATE TABLE [Asistencias] (
        [Id] bigint NOT NULL IDENTITY,
        [InscripcionId] bigint NOT NULL,
        [Asistio] bit NOT NULL,
        [FechaHoraRegistro] datetime2 NOT NULL,
        [Observacion] nvarchar(500) NULL,
        CONSTRAINT [PK_Asistencias] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Asistencias_Inscripciones_InscripcionId] FOREIGN KEY ([InscripcionId]) REFERENCES [Inscripciones] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019181438_EsquemaInicial'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Asistencias_Inscripcion] ON [Asistencias] ([InscripcionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019181438_EsquemaInicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categorias_Nombre] ON [Categorias] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019181438_EsquemaInicial'
)
BEGIN
    CREATE INDEX [IX_Eventos_CategoriaId] ON [Eventos] ([CategoriaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019181438_EsquemaInicial'
)
BEGIN
    CREATE INDEX [IX_Eventos_FechaHoraInicio] ON [Eventos] ([FechaHoraInicio]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019181438_EsquemaInicial'
)
BEGIN
    CREATE INDEX [IX_Eventos_ResponsableId] ON [Eventos] ([ResponsableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019181438_EsquemaInicial'
)
BEGIN
    CREATE INDEX [IX_Inscripciones_Evento] ON [Inscripciones] ([EventoId], [Estado]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019181438_EsquemaInicial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Inscripciones_Activas] ON [Inscripciones] ([EstudianteId], [EventoId]) WHERE [Estado] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019181438_EsquemaInicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [Usuarios] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019181438_EsquemaInicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019181438_EsquemaInicial', N'10.0.0');
END;

COMMIT;
GO

