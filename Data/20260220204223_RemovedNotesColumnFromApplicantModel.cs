using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATS.API.Data
{
    /// <inheritdoc />
    public partial class RemovedNotesColumnFromApplicantModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Applicants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Applicants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Applicants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Applicants");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Applicants");
        }
    }
}
