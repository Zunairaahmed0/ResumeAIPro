using System;
using System.IO;
using System.Text;
using ResumeAIPro.Models;

namespace ResumeAIPro.Services
{
    public class ExportService
    {
        public void ExportReport(
            ResumeData resume,
            AnalysisResult analysis,
            MatchResult? match,
            JobDescription? job,
            string filePath)
        {
            var sb = new StringBuilder();
            var line = new string('═', 55);

            sb.AppendLine(line);
            sb.AppendLine("         RESUMEAI PRO — FULL ANALYSIS REPORT");
            sb.AppendLine($"         Generated: {DateTime.Now:MMMM dd, yyyy  hh:mm tt}");
            sb.AppendLine(line);
            sb.AppendLine();

            sb.AppendLine($"RESUME:    {resume.FileName}");
            if (!string.IsNullOrEmpty(resume.Email))    sb.AppendLine($"EMAIL:     {resume.Email}");
            if (!string.IsNullOrEmpty(resume.Phone))    sb.AppendLine($"PHONE:     {resume.Phone}");
            if (!string.IsNullOrEmpty(resume.LinkedInUrl)) sb.AppendLine($"LINKEDIN:  {resume.LinkedInUrl}");
            sb.AppendLine();

            sb.AppendLine(new string('─', 55));
            sb.AppendLine("  RESUME ANALYSIS");
            sb.AppendLine(new string('─', 55));
            sb.AppendLine($"ATS Score:           {analysis.AtsScore} / 100");
            sb.AppendLine($"Experience Level:    {analysis.ExperienceLevel}");
            sb.AppendLine($"Years of Experience: {analysis.YearsOfExperience}");
            sb.AppendLine($"Primary Domain:      {analysis.PrimaryDomain}");
            sb.AppendLine($"Formatting Score:    {analysis.Formatting.Score} / 100");
            sb.AppendLine($"Content Score:       {analysis.Content.Score} / 100");
            sb.AppendLine($"Impact Score:        {analysis.Impact.Score} / 100");
            sb.AppendLine();
            sb.AppendLine("Summary:");
            sb.AppendLine(analysis.OverallSummary);
            sb.AppendLine();

            if (analysis.TechStack.Count > 0)
            {
                sb.AppendLine("Tech Stack:");
                sb.AppendLine("  " + string.Join("  •  ", analysis.TechStack));
                sb.AppendLine();
            }

            if (analysis.Strengths.Count > 0)
            {
                sb.AppendLine("Strengths:");
                foreach (var s in analysis.Strengths) sb.AppendLine($"  ✓  {s}");
                sb.AppendLine();
            }

            if (analysis.Weaknesses.Count > 0)
            {
                sb.AppendLine("Areas for Improvement:");
                foreach (var w in analysis.Weaknesses) sb.AppendLine($"  ✗  {w}");
                sb.AppendLine();
            }

            if (analysis.Recommendations.Count > 0)
            {
                sb.AppendLine("Action Recommendations:");
                foreach (var r in analysis.Recommendations) sb.AppendLine($"  •  {r}");
                sb.AppendLine();
            }

            if (analysis.Formatting.Feedback.Length > 0) sb.AppendLine($"Formatting Feedback: {analysis.Formatting.Feedback}");
            if (analysis.Content.Feedback.Length > 0)    sb.AppendLine($"Content Feedback:    {analysis.Content.Feedback}");
            if (analysis.Impact.Feedback.Length > 0)     sb.AppendLine($"Impact Feedback:     {analysis.Impact.Feedback}");

            if (match != null && job != null)
            {
                sb.AppendLine();
                sb.AppendLine(new string('─', 55));
                sb.AppendLine("  JOB MATCH ANALYSIS");
                sb.AppendLine(new string('─', 55));
                sb.AppendLine($"Position:         {job.Title}" + (job.Company.Length > 0 ? $" at {job.Company}" : ""));
                sb.AppendLine($"Overall Match:    {match.OverallMatchScore}%");
                sb.AppendLine($"Hiring Outlook:   {match.HiringOutlook}");
                sb.AppendLine($"Skills Match:     {match.SkillMatchScore}%");
                sb.AppendLine($"Experience:       {match.ExperienceMatchScore}%");
                sb.AppendLine($"Keywords:         {match.KeywordMatchScore}%");
                sb.AppendLine($"Education:        {match.EducationMatchScore}%");
                if (!string.IsNullOrEmpty(match.SalaryEstimate))
                    sb.AppendLine($"Salary Estimate:  {match.SalaryEstimate}");
                sb.AppendLine();
                sb.AppendLine("Match Summary:");
                sb.AppendLine(match.MatchSummary);
                sb.AppendLine();

                if (match.MatchedSkills.Count > 0)
                {
                    sb.AppendLine("Matched Skills:");
                    sb.AppendLine("  " + string.Join("  •  ", match.MatchedSkills));
                    sb.AppendLine();
                }
                if (match.MissingSkills.Count > 0)
                {
                    sb.AppendLine("Missing Skills (learn these):");
                    sb.AppendLine("  " + string.Join("  •  ", match.MissingSkills));
                    sb.AppendLine();
                }
                if (match.BonusSkills.Count > 0)
                {
                    sb.AppendLine("Bonus Skills (you have extra!):");
                    sb.AppendLine("  " + string.Join("  •  ", match.BonusSkills));
                    sb.AppendLine();
                }
                if (match.InterviewTopics.Count > 0)
                {
                    sb.AppendLine("Interview Preparation Topics:");
                    foreach (var t in match.InterviewTopics) sb.AppendLine($"  •  {t}");
                    sb.AppendLine();
                }
                if (match.ImprovementTips.Count > 0)
                {
                    sb.AppendLine("Improvement Tips:");
                    foreach (var t in match.ImprovementTips) sb.AppendLine($"  •  {t}");
                    sb.AppendLine();
                }
                if (match.AlternativeTitles.Count > 0)
                {
                    sb.AppendLine("Alternative Job Titles to Consider:");
                    sb.AppendLine("  " + string.Join("  •  ", match.AlternativeTitles));
                }
            }

            sb.AppendLine();
            sb.AppendLine(line);
            sb.AppendLine("             Powered by ResumeAI Pro");
            sb.AppendLine("       AI-Powered Career Intelligence Platform");
            sb.AppendLine(line);

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
