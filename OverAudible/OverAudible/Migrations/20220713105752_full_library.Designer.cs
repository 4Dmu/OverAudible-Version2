﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OverAudible.DbContexts;

#nullable disable

namespace OverAudible.Migrations
{
    [DbContext(typeof(MainDbContext))]
    [Migration("20220713105752_full_library")]
    partial class full_library
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.0-preview.5.22302.2");

            modelBuilder.Entity("OverAudible.Models.DTOs.ItemDTO", b =>
                {
                    b.Property<string>("Asin")
                        .HasColumnType("TEXT");

                    b.Property<string>("ContentMetadataJson")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Item")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Asin");

                    b.ToTable("OfflineLibrary");
                });

            modelBuilder.Entity("OverAudible.Models.DTOs.NoMetaItemDTO", b =>
                {
                    b.Property<string>("Asin")
                        .HasColumnType("TEXT");

                    b.Property<string>("Item")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Asin");

                    b.ToTable("FullLibrary");
                });
#pragma warning restore 612, 618
        }
    }
}