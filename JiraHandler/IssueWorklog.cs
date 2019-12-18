using Atlassian.Jira;
using System;

namespace jiraps.JiraHandler
{
    public class IssueWorklog {

        public IssueWorklog(Worklog l, string issueId) {
            IssueId = issueId;
            Author = l.Author;
            Comment = l.Comment;
            StartDate = l.StartDate;
            TimeSpent = l.TimeSpent;
            Id = l.Id;
            TimeSpentInSeconds = l.TimeSpentInSeconds;
            CreateDate = l.CreateDate;
            UpdateDate = l.UpdateDate;
        }

        public string IssueId { get;set; } = "";
        public string Author { get; set; } = "";
        public string Comment { get; set; } = "";
        public DateTime? StartDate { get; set; }
        public string TimeSpent { get; set; } = "";
        public string Id { get; } = "";
        public long TimeSpentInSeconds { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}