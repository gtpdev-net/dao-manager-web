using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAO.Manager.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialNormalizedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Scans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RepositoryPath = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    GitCommit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ScanDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assemblies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assemblies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assemblies_Scans_ScanId",
                        column: x => x.ScanId,
                        principalTable: "Scans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Packages_Scans_ScanId",
                        column: x => x.ScanId,
                        principalTable: "Scans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanId = table.Column<int>(type: "int", nullable: false),
                    UniqueIdentifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VisualStudioGuid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TargetFramework = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Scans_ScanId",
                        column: x => x.ScanId,
                        principalTable: "Scans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScanEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanId = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Phase = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScanEvents_Scans_ScanId",
                        column: x => x.ScanId,
                        principalTable: "Scans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Solutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanId = table.Column<int>(type: "int", nullable: false),
                    UniqueIdentifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VisualStudioGuid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solutions_Scans_ScanId",
                        column: x => x.ScanId,
                        principalTable: "Scans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssemblyDependencies",
                columns: table => new
                {
                    ReferencingAssemblyId = table.Column<int>(type: "int", nullable: false),
                    ReferencedAssemblyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssemblyDependencies", x => new { x.ReferencingAssemblyId, x.ReferencedAssemblyId });
                    table.ForeignKey(
                        name: "FK_AssemblyDependencies_Assemblies_ReferencedAssemblyId",
                        column: x => x.ReferencedAssemblyId,
                        principalTable: "Assemblies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssemblyDependencies_Assemblies_ReferencingAssemblyId",
                        column: x => x.ReferencingAssemblyId,
                        principalTable: "Assemblies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectAssemblyReferences",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    AssemblyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAssemblyReferences", x => new { x.ProjectId, x.AssemblyId });
                    table.ForeignKey(
                        name: "FK_ProjectAssemblyReferences_Assemblies_AssemblyId",
                        column: x => x.AssemblyId,
                        principalTable: "Assemblies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectAssemblyReferences_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPackageReferences",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    PackageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPackageReferences", x => new { x.ProjectId, x.PackageId });
                    table.ForeignKey(
                        name: "FK_ProjectPackageReferences_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectPackageReferences_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectReferences",
                columns: table => new
                {
                    ReferencingProjectId = table.Column<int>(type: "int", nullable: false),
                    ReferencedProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectReferences", x => new { x.ReferencingProjectId, x.ReferencedProjectId });
                    table.ForeignKey(
                        name: "FK_ProjectReferences_Projects_ReferencedProjectId",
                        column: x => x.ReferencedProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectReferences_Projects_ReferencingProjectId",
                        column: x => x.ReferencingProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolutionProjects",
                columns: table => new
                {
                    SolutionId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolutionProjects", x => new { x.SolutionId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_SolutionProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolutionProjects_Solutions_SolutionId",
                        column: x => x.SolutionId,
                        principalTable: "Solutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assemblies_Name",
                table: "Assemblies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Assemblies_ScanId",
                table: "Assemblies",
                column: "ScanId");

            migrationBuilder.CreateIndex(
                name: "IX_Assemblies_ScanId_FilePath",
                table: "Assemblies",
                columns: new[] { "ScanId", "FilePath" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyDependencies_ReferencedAssemblyId",
                table: "AssemblyDependencies",
                column: "ReferencedAssemblyId");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_Name",
                table: "Packages",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_ScanId",
                table: "Packages",
                column: "ScanId");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_ScanId_Name_Version",
                table: "Packages",
                columns: new[] { "ScanId", "Name", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssemblyReferences_AssemblyId",
                table: "ProjectAssemblyReferences",
                column: "AssemblyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPackageReferences_PackageId",
                table: "ProjectPackageReferences",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectReferences_ReferencedProjectId",
                table: "ProjectReferences",
                column: "ReferencedProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_FilePath",
                table: "Projects",
                column: "FilePath");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name",
                table: "Projects",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ScanId",
                table: "Projects",
                column: "ScanId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ScanId_UniqueIdentifier",
                table: "Projects",
                columns: new[] { "ScanId", "UniqueIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScanEvents_ScanId",
                table: "ScanEvents",
                column: "ScanId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanEvents_ScanId_OccurredAt",
                table: "ScanEvents",
                columns: new[] { "ScanId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Scans_CreatedAt",
                table: "Scans",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Scans_GitCommit",
                table: "Scans",
                column: "GitCommit");

            migrationBuilder.CreateIndex(
                name: "IX_Scans_ScanDate",
                table: "Scans",
                column: "ScanDate");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionProjects_ProjectId",
                table: "SolutionProjects",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_FilePath",
                table: "Solutions",
                column: "FilePath");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_ScanId",
                table: "Solutions",
                column: "ScanId");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_ScanId_UniqueIdentifier",
                table: "Solutions",
                columns: new[] { "ScanId", "UniqueIdentifier" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssemblyDependencies");

            migrationBuilder.DropTable(
                name: "ProjectAssemblyReferences");

            migrationBuilder.DropTable(
                name: "ProjectPackageReferences");

            migrationBuilder.DropTable(
                name: "ProjectReferences");

            migrationBuilder.DropTable(
                name: "ScanEvents");

            migrationBuilder.DropTable(
                name: "SolutionProjects");

            migrationBuilder.DropTable(
                name: "Assemblies");

            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Solutions");

            migrationBuilder.DropTable(
                name: "Scans");
        }
    }
}
