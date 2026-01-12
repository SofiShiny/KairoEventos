using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;
using FluentAssertions;

namespace Entradas.Pruebas;

public class MigrationSnapshotTests
{
    [Fact]
    public void ModelSnapshot_BuildModel_Coverage()
    {
        var infraAssembly = typeof(Entradas.Infraestructura.Persistencia.EntradasDbContext).Assembly;
        var snapshotType = infraAssembly.GetType("Entradas.Infraestructura.Migrations.EntradasDbContextModelSnapshot");
        
        snapshotType.Should().NotBeNull();
        
        var snapshot = Activator.CreateInstance(snapshotType!) as ModelSnapshot;
        snapshot.Should().NotBeNull();

        var modelBuilder = new ModelBuilder(new ConventionSet());
        var method = snapshotType!.GetMethod("BuildModel", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(snapshot, new object[] { modelBuilder });

        modelBuilder.Model.Should().NotBeNull();
    }

    [Fact]
    public void InitialCreate_Coverage()
    {
        var infraAssembly = typeof(Entradas.Infraestructura.Persistencia.EntradasDbContext).Assembly;
        var migrationType = infraAssembly.GetType("Entradas.Infraestructura.Migrations.InitialCreate");
        
        migrationType.Should().NotBeNull();
        
        var migration = Activator.CreateInstance(migrationType!) as Migration;
        migration.Should().NotBeNull();

        // 1. Test Up method
        var migrationBuilder = new MigrationBuilder("Npgsql.EntityFrameworkCore.PostgreSQL");
        var upMethod = migrationType!.GetMethod("Up", BindingFlags.Instance | BindingFlags.NonPublic);
        upMethod!.Invoke(migration, new object[] { migrationBuilder });
        migrationBuilder.Operations.Should().NotBeEmpty();

        // 2. Test Down method
        var downMigrationBuilder = new MigrationBuilder("Npgsql.EntityFrameworkCore.PostgreSQL");
        var downMethod = migrationType!.GetMethod("Down", BindingFlags.Instance | BindingFlags.NonPublic);
        downMethod!.Invoke(migration, new object[] { downMigrationBuilder });
        downMigrationBuilder.Operations.Should().NotBeEmpty();

        // 3. Test BuildTargetModel (from Designer.cs)
        var modelBuilder = new ModelBuilder(new ConventionSet());
        var buildTargetMethod = migrationType!.GetMethod("BuildTargetModel", BindingFlags.Instance | BindingFlags.NonPublic);
        buildTargetMethod!.Invoke(migration, new object[] { modelBuilder });
        modelBuilder.Model.Should().NotBeNull();
    }
}
