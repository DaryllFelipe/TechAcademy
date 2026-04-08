using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechAcademyApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Restaurants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restaurants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RestaurantId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReviewerName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Restaurants",
                columns: new[] { "Id", "City", "CreatedDate", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "New York", new DateTime(2025, 1, 15, 10, 30, 0, 0, DateTimeKind.Unspecified), true, "The Italian Kitchen" },
                    { 2, "San Francisco", new DateTime(2025, 2, 20, 14, 45, 0, 0, DateTimeKind.Unspecified), true, "Dragon Palace" },
                    { 3, "Boston", new DateTime(2025, 1, 10, 9, 15, 0, 0, DateTimeKind.Unspecified), true, "Le Petit Bistro" },
                    { 4, "Seattle", new DateTime(2025, 3, 5, 11, 20, 0, 0, DateTimeKind.Unspecified), true, "Sakura Sushi House" },
                    { 5, "Austin", new DateTime(2025, 2, 28, 16, 0, 0, 0, DateTimeKind.Unspecified), true, "The Mexican Grill" }
                });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "Id", "Comment", "CreatedDate", "Rating", "RestaurantId", "ReviewerName" },
                values: new object[,]
                {
                    { 1, "Excellent pasta and authentic Italian flavors! The ambiance is warm and inviting.", new DateTime(2025, 1, 20, 12, 30, 0, 0, DateTimeKind.Unspecified), 5, 1, "Alice Johnson" },
                    { 2, "Great food, but a bit pricey. Service was attentive and quick.", new DateTime(2025, 1, 25, 18, 45, 0, 0, DateTimeKind.Unspecified), 4, 1, "Bob Smith" },
                    { 3, "Amazing dim sum! Fresh ingredients and perfect cooking. Must visit.", new DateTime(2025, 2, 22, 19, 0, 0, 0, DateTimeKind.Unspecified), 5, 2, "Charlie Brown" },
                    { 4, "Charming atmosphere and delicious food. French classics done right.", new DateTime(2025, 1, 12, 20, 15, 0, 0, DateTimeKind.Unspecified), 4, 3, "Diana Prince" },
                    { 5, "Best sushi in the city! Incredible quality and presentation. Highly recommend.", new DateTime(2025, 3, 8, 18, 30, 0, 0, DateTimeKind.Unspecified), 5, 4, "Emma Davis" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_RestaurantId",
                table: "Reviews",
                column: "RestaurantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Restaurants");
        }
    }
}
