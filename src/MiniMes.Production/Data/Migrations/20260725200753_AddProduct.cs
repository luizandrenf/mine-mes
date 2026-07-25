using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniMes.Production.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_production_orders_product_id",
                table: "production_orders",
                column: "product_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_products_code",
                table: "products",
                column: "code",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_production_orders_products_product_id",
                table: "production_orders",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_production_orders_products_product_id",
                table: "production_orders"
            );

            migrationBuilder.DropTable(name: "products");

            migrationBuilder.DropIndex(
                name: "IX_production_orders_product_id",
                table: "production_orders"
            );
        }
    }
}
