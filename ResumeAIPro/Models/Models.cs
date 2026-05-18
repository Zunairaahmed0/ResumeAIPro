using System.Collections.Generic;

namespace ResumeAIPro.Models
{
    // ─────────────────────────────────────────────
    //  Core Resume Model
    // ─────────────────────────────────────────────
    public class ResumeData
    {
        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = "";
        public string RawText  { get; set; } = "";
        public string CandidateName { get; set; } = "";
        public string Email  { get; set; } = "";
        public string Phone  { get; set; } = "";
        public string Location { get; set; } = "";
        public string LinkedInUrl { get; set; } = "";
    }

    // ─────────────────────────────────────────────
    //  AI Analysis Result
    // ─────────────────────────────────────────────
    public class AnalysisResult
    {
        public int    AtsScore          { get; set; }
        public string OverallSummary    { get; set; } = "";
        public string ExperienceLevel   { get; set; } = "";
        public int    YearsOfExperience { get; set; }
        public string PrimaryDomain     { get; set; } = "";

        public List<SkillCategory> SkillCategories { get; set; } = new();
        public List<string>  Strengths       { get; set; } = new();
        public List<string>  Weaknesses      { get; set; } = new();
        public List<string>  Recommendations { get; set; } = new();
        public List<string>  TechStack       { get; set; } = new();
        public List<string>  SoftSkills      { get; set; } = new();

        public FormattingScore Formatting { get; set; } = new();
        public ContentScore    Content    { get; set; } = new();
        public ImpactScore     Impact     { get; set; } = new();
    }

    public class SkillCategory
    {
        public string Name   { get; set; } = "";
        public int    Level  { get; set; }
        public string Color  { get; set; } = "#7C3AED";
    }

    public class FormattingScore { public int Score { get; set; } public string Feedback { get; set; } = ""; }
    public class ContentScore    { public int Score { get; set; } public string Feedback { get; set; } = ""; }
    public class ImpactScore     { public int Score { get; set; } public string Feedback { get; set; } = ""; }

    // ─────────────────────────────────────────────
    //  Job Description Model
    // ─────────────────────────────────────────────
    public class JobDescription
    {
        public string Title       { get; set; } = "";
        public string Company     { get; set; } = "";
        public string RawText     { get; set; } = "";
        public string Department  { get; set; } = "";
        public string Location    { get; set; } = "";
        public string SeniorityLevel { get; set; } = "";
    }

    // ─────────────────────────────────────────────
    //  Job Match Result
    // ─────────────────────────────────────────────
    public class MatchResult
    {
        public int    OverallMatchScore   { get; set; }
        public int    SkillMatchScore     { get; set; }
        public int    ExperienceMatchScore{ get; set; }
        public int    EducationMatchScore { get; set; }
        public int    KeywordMatchScore   { get; set; }

        public List<string> MatchedSkills  { get; set; } = new();
        public List<string> MissingSkills  { get; set; } = new();
        public List<string> BonusSkills    { get; set; } = new();

        public string MatchSummary        { get; set; } = "";
        public string HiringOutlook       { get; set; } = "";
        public List<string> ImprovementTips { get; set; } = new();
        public List<string> InterviewTopics { get; set; } = new();
        public string SalaryEstimate      { get; set; } = "";
        public List<string> AlternativeTitles { get; set; } = new();
    }

    // ─────────────────────────────────────────────
    //  Job Listing (from web search)
    // ─────────────────────────────────────────────
    public class JobListing
    {
        public string Title        { get; set; } = "";
        public string Company      { get; set; } = "";
        public string Location     { get; set; } = "";
        public string JobType      { get; set; } = "";
        public string Salary       { get; set; } = "";
        public string Description  { get; set; } = "";
        public string ApplyUrl     { get; set; } = "";
        public int    MatchScore   { get; set; }
        public List<string> RequiredSkills { get; set; } = new();
        public string PostedDate   { get; set; } = "";
        public string Source       { get; set; } = "";
    }

    // ─────────────────────────────────────────────
    //  History Entry
    // ─────────────────────────────────────────────
    public class AnalysisHistoryEntry
    {
        public string Id           { get; set; } = System.Guid.NewGuid().ToString();
        public string ResumeFileName{ get; set; } = "";
        public string JobTitle     { get; set; } = "";
        public int    AtsScore     { get; set; }
        public int    MatchScore   { get; set; }
        public System.DateTime AnalyzedAt { get; set; } = System.DateTime.Now;
    }
}
