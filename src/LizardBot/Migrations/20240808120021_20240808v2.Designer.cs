﻿// <auto-generated />
using System;
using LizardBot.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LizardBot.Migrations
{
    [DbContext(typeof(LizardBotDbContext))]
    [Migration("20240808120021_20240808v2")]
    partial class _20240808v2
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("LizardBot.Data.Model.BotChannel", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("NoticeId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("SettingType")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("bot_channel", "lizardbot");
                });

            modelBuilder.Entity("LizardBot.Data.Model.GptThread", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("AssistantId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uuid");

                    b.Property<int>("TokenUsage")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("gpt_thread", "lizardbot");
                });

            modelBuilder.Entity("LizardBot.Data.Model.PlatformUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("DiscordId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("platform_user", "lizardbot");
                });
#pragma warning restore 612, 618
        }
    }
}
