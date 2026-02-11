using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAO.Manager.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCircularCascadePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectAssemblyReferences_Assemblies_AssemblyId",
                table: "ProjectAssemblyReferences");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectPackageReferences_Packages_PackageId",
                table: "ProjectPackageReferences");

            migrationBuilder.DropForeignKey(
                name: "FK_SolutionProjects_Solutions_SolutionId",
                table: "SolutionProjects");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectAssemblyReferences_Assemblies_AssemblyId",
                table: "ProjectAssemblyReferences",
                column: "AssemblyId",
                principalTable: "Assemblies",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectPackageReferences_Packages_PackageId",
                table: "ProjectPackageReferences",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_SolutionProjects_Solutions_SolutionId",
                table: "SolutionProjects",
                column: "SolutionId",
                principalTable: "Solutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectAssemblyReferences_Assemblies_AssemblyId",
                table: "ProjectAssemblyReferences");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectPackageReferences_Packages_PackageId",
                table: "ProjectPackageReferences");

            migrationBuilder.DropForeignKey(
                name: "FK_SolutionProjects_Solutions_SolutionId",
                table: "SolutionProjects");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectAssemblyReferences_Assemblies_AssemblyId",
                table: "ProjectAssemblyReferences",
                column: "AssemblyId",
                principalTable: "Assemblies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectPackageReferences_Packages_PackageId",
                table: "ProjectPackageReferences",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SolutionProjects_Solutions_SolutionId",
                table: "SolutionProjects",
                column: "SolutionId",
                principalTable: "Solutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
