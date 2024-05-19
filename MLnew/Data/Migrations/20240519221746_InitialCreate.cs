using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MLnew.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Historique",
                columns: table => new
                {
                    HistoriqueID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateSimulation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historique", x => x.HistoriqueID);
                });

            migrationBuilder.CreateTable(
                name: "Methode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Param1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Param2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Param3 = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Methode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modele",
                columns: table => new
                {
                    ModeleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HistoriqueID = table.Column<int>(type: "int", nullable: false),
                    DureeSec = table.Column<int>(type: "int", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modele", x => x.ModeleID);
                    table.ForeignKey(
                        name: "FK_Modele_Historique_HistoriqueID",
                        column: x => x.HistoriqueID,
                        principalTable: "Historique",
                        principalColumn: "HistoriqueID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Simulation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MethodeId = table.Column<int>(type: "int", nullable: false),
                    Accuracy = table.Column<float>(type: "real", nullable: false),
                    Duree = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateSimulation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Simulation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Simulation_Methode_MethodeId",
                        column: x => x.MethodeId,
                        principalTable: "Methode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Parametre",
                columns: table => new
                {
                    ParametreID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModeleID = table.Column<int>(type: "int", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Valeur = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametre", x => x.ParametreID);
                    table.ForeignKey(
                        name: "FK_Parametre_Modele_ModeleID",
                        column: x => x.ModeleID,
                        principalTable: "Modele",
                        principalColumn: "ModeleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Modele_HistoriqueID",
                table: "Modele",
                column: "HistoriqueID");

            migrationBuilder.CreateIndex(
                name: "IX_Parametre_ModeleID",
                table: "Parametre",
                column: "ModeleID");

            migrationBuilder.CreateIndex(
                name: "IX_Simulation_MethodeId",
                table: "Simulation",
                column: "MethodeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Parametre");

            migrationBuilder.DropTable(
                name: "Simulation");

            migrationBuilder.DropTable(
                name: "Modele");

            migrationBuilder.DropTable(
                name: "Methode");

            migrationBuilder.DropTable(
                name: "Historique");
        }
    }
}
