using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using efscaffold.Entities;

namespace Infrastructure.Postgres.Scaffolding;

public partial class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Login> Logins { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Login>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("login_pkey");

            entity.ToTable("login", "chatapp");

            entity.HasIndex(e => e.Username, "login_username_key").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Logins)
                .HasForeignKey(d => d.Roleid)
                .HasConstraintName("fk_login_role");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Messageid).HasName("message_pkey");

            entity.ToTable("message", "chatapp");

            entity.Property(e => e.Messageid).HasColumnName("messageid");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Recipientuserid).HasColumnName("recipientuserid");
            entity.Property(e => e.Roomid).HasColumnName("roomid");
            entity.Property(e => e.Senderuserid).HasColumnName("senderuserid");
            entity.Property(e => e.Sentat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("sentat");

            entity.HasOne(d => d.Recipientuser).WithMany(p => p.MessageRecipientusers)
                .HasForeignKey(d => d.Recipientuserid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_message_recipient");

            entity.HasOne(d => d.Room).WithMany(p => p.Messages)
                .HasForeignKey(d => d.Roomid)
                .HasConstraintName("fk_message_room");

            entity.HasOne(d => d.Senderuser).WithMany(p => p.MessageSenderusers)
                .HasForeignKey(d => d.Senderuserid)
                .HasConstraintName("fk_message_sender");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Roleid).HasName("role_pkey");

            entity.ToTable("role", "chatapp");

            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Rolename)
                .HasMaxLength(100)
                .HasColumnName("rolename");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Roomid).HasName("room_pkey");

            entity.ToTable("room", "chatapp");

            entity.Property(e => e.Roomid).HasColumnName("roomid");
            entity.Property(e => e.Roomname)
                .HasMaxLength(100)
                .HasColumnName("roomname");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
