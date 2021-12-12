using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TG.Auth.Api.Migrations
{
    public partial class Ban : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ban_id",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "bans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ban_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    banned_till = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    unbanned_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    reason = table.Column<int>(type: "integer", nullable: false),
                    admin_user_login = table.Column<string>(type: "text", nullable: true),
                    comment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bans", x => x.id);
                    table.ForeignKey(
                        name: "fk_bans_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bans_user_id",
                table: "bans",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bans");

            migrationBuilder.DropColumn(
                name: "ban_id",
                table: "users");
        }
    }
}
