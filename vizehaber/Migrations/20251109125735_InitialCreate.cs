using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace vizehaber.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.Id);
                    table.ForeignKey(
                        name: "FK_News_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_News_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "Id", "Bio", "Created", "Email", "IsActive", "Name", "Password", "PhotoPath", "Updated" },
                values: new object[,]
                {
                    { 1, "Siyaset alanında uzman gazeteci.", new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2288), "ahmet@yazar.com", true, "Ahmet Yazar", "1234", "/userPhotos/ahmet.jpg", new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2289) },
                    { 2, "Ekonomi yazılarıyla tanınan deneyimli yazar.", new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2291), "ayse@yazar.com", true, "Ayşe Yazar", "1234", "/userPhotos/ayse.jpg", new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2292) }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Created", "IsActive", "Name", "Updated" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2091), true, "Siyaset", new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2102) },
                    { 2, new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2105), true, "Ekonomi", new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2105) },
                    { 3, new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2112), true, "Spor", new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2112) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Created", "Email", "FullName", "IsActive", "Password", "PhotoPath", "Role", "Updated", "UserName" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2308), "ali@user.com", "Ali Kullanıcı", true, "1234", "/userPhotos/ali.png", "User", new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2309), "alikullanici" },
                    { 2, new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2312), "admin@admin.com", "Admin Kullanıcı", true, "admin", "/userPhotos/admin.png", "Admin", new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2312), "admin" }
                });

            migrationBuilder.InsertData(
                table: "News",
                columns: new[] { "Id", "AuthorId", "CategoryId", "Content", "Created", "ImagePath", "IsActive", "IsApproved", "PublishedDate", "Title", "Updated" },
                values: new object[,]
                {
                    { 1, 1, 1, "Siyaset gündemine dair haber 1.", new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2332), null, true, false, new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2331), "Siyaset Haberi 1", new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2333) },
                    { 2, 2, 2, "Ekonomiyle ilgili haber 1.", new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2336), null, true, false, new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2335), "Ekonomi Haberi 1", new DateTime(2025, 11, 9, 15, 57, 35, 6, DateTimeKind.Local).AddTicks(2336) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_News_AuthorId",
                table: "News",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_News_CategoryId",
                table: "News",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
