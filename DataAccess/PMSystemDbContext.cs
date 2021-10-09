using PMSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace PMSystem.DataAccess
{
    public class PMSystemDbContext : DbContext
    {
        public PMSystemDbContext() : base("PMSystemFinalDB") { }
            public virtual DbSet<Project> Projects { get; set; }
            public virtual DbSet<SubTask> SubTasks { get; set; }
            public virtual DbSet<Task> Tasks { get; set; }
            public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Many-to-Many relationship between "User" and "Project"
            modelBuilder.Entity<User>()
                .HasMany(b => b.AssignedProjects)
                .WithMany(c => c.AssignedUsers)
                .Map(cs =>
                   {
                    cs.MapLeftKey("UserId");
                    cs.MapRightKey("ProjectId");
                    cs.ToTable("UserProjects");
                    });

            //Many-to-Many relationship between "User" and "Task"
            modelBuilder.Entity<User>()
                .HasMany(b => b.AssignedTasks)
                .WithMany(c => c.AssignedUsers)
                .Map(cs =>
                {
                    cs.MapLeftKey("UserId");
                    cs.MapRightKey("TaskId");
                    cs.ToTable("UserTasks");
                });

            //Many-to-Many relationship between "User" and "SubTask"
            //modelBuilder.Entity<User>()
            //    .HasMany(b => b.AssignedSubTasks)
            //  .WithMany(c => c.AssignedUsers)
            //.Map(cs =>
            //{
            //cs.MapLeftKey("UserId");
            //cs.MapRightKey("SubTaskId");
            //cs.ToTable("UserSubTasks");
            //});

            //One-to-Many relationship between "User" and "SubTask"
            modelBuilder.Entity<User>()
                .HasMany(e => e.AssignedSubTasks)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            //One-to-Many relationship between "Project" and "Task"
            modelBuilder.Entity<Project>()
                .HasMany(e => e.Tasks)
                .WithRequired(e => e.Project)
                .WillCascadeOnDelete(false);

            //One-to-Many relationship between "Task" and "SubTask"
            modelBuilder.Entity<Task>()
                .HasMany(e => e.SubTasks)
                .WithRequired(e => e.Task)
                .WillCascadeOnDelete(false);


        }

    }
    
    
}