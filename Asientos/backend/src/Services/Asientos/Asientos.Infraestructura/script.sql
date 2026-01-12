CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "Mapas" (
    "Id" uuid NOT NULL,
    "EventoId" uuid NOT NULL,
    CONSTRAINT "PK_Mapas" PRIMARY KEY ("Id")
);

CREATE TABLE "Asientos" (
    "Id" uuid NOT NULL,
    "MapaId" uuid NOT NULL,
    "EventoId" uuid NOT NULL,
    "Fila" integer NOT NULL,
    "Numero" integer NOT NULL,
    "CategoriaNombre" text NOT NULL,
    "CategoriaPrecioBase" numeric,
    "CategoriaTienePrioridad" boolean NOT NULL,
    "Reservado" boolean NOT NULL,
    "MapaAsientosId" uuid,
    CONSTRAINT "PK_Asientos" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Asientos_Mapas_MapaAsientosId" FOREIGN KEY ("MapaAsientosId") REFERENCES "Mapas" ("Id"),
    CONSTRAINT "FK_Asientos_Mapas_MapaId" FOREIGN KEY ("MapaId") REFERENCES "Mapas" ("Id") ON DELETE CASCADE
);

CREATE TABLE "MapasCategorias" (
    "Id" uuid NOT NULL,
    "Nombre" text NOT NULL,
    "PrecioBase" numeric,
    "TienePrioridad" boolean NOT NULL,
    "MapaId" uuid NOT NULL,
    CONSTRAINT "PK_MapasCategorias" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_MapasCategorias_Mapas_MapaId" FOREIGN KEY ("MapaId") REFERENCES "Mapas" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Asientos_MapaAsientosId" ON "Asientos" ("MapaAsientosId");

CREATE UNIQUE INDEX "IX_Asientos_MapaId_Fila_Numero" ON "Asientos" ("MapaId", "Fila", "Numero");

CREATE UNIQUE INDEX "IX_MapasCategorias_MapaId_Nombre" ON "MapasCategorias" ("MapaId", "Nombre");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251128004515_InitialCreate', '8.0.0');

COMMIT;

