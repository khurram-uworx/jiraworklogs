﻿using Microsoft.EntityFrameworkCore;
using System;
using UWorx.JiraWorkLogs;

namespace JiraWorkLogsService.Data;

class TeamMember
{
    public int TeamMemberId { get; set; }
    public string TeamMemberName { get; set; } = string.Empty;
    public string TeamMemberEmail { get; set; } = string.Empty;
    public bool Concerned { get; set; } = false;
}

class Worklog
{
    public int WorklogId { get; set; }
    public string WorklogJiraId { get; set; } = string.Empty;
    public int TeamMemberId { get; set; }
    public DateTime WorklogDate { get; set; }
    public long TimeSpentSeconds { get; set; } = 0;

    public TeamMember TeamMember { get; set; }
}

class JiraDbContext : DbContext
{
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<Worklog> Worklogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(JiraWorkLogConstants.DatabaseConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasIndex(e => e.TeamMemberEmail).IsUnique();
        });

        modelBuilder.Entity<Worklog>(entity =>
        {
            entity.HasIndex(e => e.WorklogJiraId).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}
