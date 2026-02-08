using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ORIGINAL_DAO.Manager_REFERENCE_ONLY.Migrations
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
                    ScanDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GitCommitHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShortCommitHash = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RepositoryPath = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scans", x => x.Id);
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
                    GuidDeterminationMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetFramework = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProjectStyle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
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
                name: "Solutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanId = table.Column<int>(type: "int", nullable: false),
                    UniqueIdentifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VisualStudioGuid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    GuidDeterminationMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsSingleProjectSolution = table.Column<bool>(type: "bit", nullable: false)
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
                name: "Assemblies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    UniqueIdentifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AssemblyFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OutputType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProjectStyle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetFramework = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProjectFilePath = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assemblies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assemblies_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssemblyReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    AssemblyName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HintPath = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssemblyReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssemblyReferences_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    PackageName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDevelopmentDependency = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageReferences_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectDependencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanId = table.Column<int>(type: "int", nullable: false),
                    SourceProjectId = table.Column<int>(type: "int", nullable: false),
                    TargetProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDependencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDependencies_Projects_SourceProjectId",
                        column: x => x.SourceProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectDependencies_Projects_TargetProjectId",
                        column: x => x.TargetProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectDependencies_Scans_ScanId",
                        column: x => x.ScanId,
                        principalTable: "Scans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolutionProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScanId = table.Column<int>(type: "int", nullable: false),
                    SolutionId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolutionProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolutionProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolutionProjects_Scans_ScanId",
                        column: x => x.ScanId,
                        principalTable: "Scans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolutionProjects_Solutions_SolutionId",
                        column: x => x.SolutionId,
                        principalTable: "Solutions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssemblyDependencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceAssemblyId = table.Column<int>(type: "int", nullable: false),
                    TargetAssemblyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssemblyDependencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssemblyDependencies_Assemblies_SourceAssemblyId",
                        column: x => x.SourceAssemblyId,
                        principalTable: "Assemblies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssemblyDependencies_Assemblies_TargetAssemblyId",
                        column: x => x.TargetAssemblyId,
                        principalTable: "Assemblies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assemblies_Name",
                table: "Assemblies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Assemblies_ProjectId",
                table: "Assemblies",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Assemblies_UniqueIdentifier",
                table: "Assemblies",
                column: "UniqueIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyDependencies_SourceAssemblyId_TargetAssemblyId",
                table: "AssemblyDependencies",
                columns: new[] { "SourceAssemblyId", "TargetAssemblyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyDependencies_TargetAssemblyId",
                table: "AssemblyDependencies",
                column: "TargetAssemblyId");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyReferences_ProjectId",
                table: "AssemblyReferences",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageReferences_PackageName",
                table: "PackageReferences",
                column: "PackageName");

            migrationBuilder.CreateIndex(
                name: "IX_PackageReferences_ProjectId",
                table: "PackageReferences",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageReferences_ProjectId_PackageName_Version",
                table: "PackageReferences",
                columns: new[] { "ProjectId", "PackageName", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDependencies_ScanId",
                table: "ProjectDependencies",
                column: "ScanId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDependencies_SourceProjectId_TargetProjectId",
                table: "ProjectDependencies",
                columns: new[] { "SourceProjectId", "TargetProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDependencies_TargetProjectId",
                table: "ProjectDependencies",
                column: "TargetProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_FilePath",
                table: "Projects",
                column: "FilePath");

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
                name: "IX_Scans_CreatedAt",
                table: "Scans",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Scans_ScanDate",
                table: "Scans",
                column: "ScanDate");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionProjects_ProjectId",
                table: "SolutionProjects",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionProjects_ScanId",
                table: "SolutionProjects",
                column: "ScanId");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionProjects_SolutionId_ProjectId",
                table: "SolutionProjects",
                columns: new[] { "SolutionId", "ProjectId" },
                unique: true);

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
                name: "AssemblyReferences");

            migrationBuilder.DropTable(
                name: "PackageReferences");

            migrationBuilder.DropTable(
                name: "ProjectDependencies");

            migrationBuilder.DropTable(
                name: "SolutionProjects");

            migrationBuilder.DropTable(
                name: "Assemblies");

            migrationBuilder.DropTable(
                name: "Solutions");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Scans");
        }
    }
}
