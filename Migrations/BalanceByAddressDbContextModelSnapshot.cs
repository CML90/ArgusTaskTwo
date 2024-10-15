﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Argus_BAVer2.Migrations
{
    [DbContext(typeof(BalanceByAddressDbContext))]
    partial class BalanceByAddressDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("cardanoindexer")
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Argus_BAVer2.Data.Models.BalanceByAddress", b =>
                {
                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<decimal>("Balance")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("Balance");

                    b.HasKey("Address");

                    b.ToTable("BalanceByAddress", "cardanoindexer");
                });

            modelBuilder.Entity("Argus_BAVer2.Data.Models.InputsBySlot", b =>
                {
                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<decimal>("OutputIndex")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("Slot");

                    b.HasKey("TxHash", "OutputIndex");

                    b.ToTable("InputsBySlot", "cardanoindexer");
                });

            modelBuilder.Entity("Argus_BAVer2.Data.Models.OutputsBySlot", b =>
                {
                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<decimal>("TxIndex")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("Slot");

                    b.Property<decimal>("Value")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("Value");

                    b.HasKey("TxHash", "TxIndex");

                    b.ToTable("OutputsBySlot", "cardanoindexer");
                });

            modelBuilder.Entity("Cardano.Sync.Data.Models.ReducerState", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Name");

                    b.ToTable("ReducerStates", "cardanoindexer");
                });
#pragma warning restore 612, 618
        }
    }
}
