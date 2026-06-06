using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ResumeAIPro.Models;

namespace ResumeAIPro.Services
{
    public class AIAnalysisService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        private const string ApiBase    = "https://generativelanguage.googleapis.com/v1beta/models/";
        private const string ModelPrimary  = "gemini-2.0-flash-lite";
        private const string ModelFallback = "gemini-2.5-flash-lite";

        public AIAnalysisService(string apiKey)
        {
            _apiKey = apiKey;
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
        }

        // ══════════════════════════════════════════════════════════
        //  RESUME ANALYSIS
        // ══════════════════════════════════════════════════════════
        public async Task<AnalysisResult> AnalyzeResumeAsync(ResumeData resume)
        {
            var prompt = $@"You are an expert ATS (Applicant Tracking System) analyst and senior career coach.
Analyze the provided resume with extreme precision and return ONLY a valid JSON object.
No markdown, no explanation, no code blocks — just raw JSON.

Return this exact structure:
{{
  ""atsScore"": <integer 0-100, be realistic and strict>,
  ""overallSummary"": ""<3-4 sentence professional summary of the candidate>"",
  ""experienceLevel"": ""<Junior|Mid|Senior|Lead>"",
  ""yearsOfExperience"": <integer>,
  ""primaryDomain"": ""<e.g. Backend Development, Data Science, Mobile Development>"",
  ""skillCategories"": [
    {{ ""name"": ""<skill area>"", ""level"": <0-100>, ""color"": ""<hex color>"" }}
  ],
  ""strengths"": [""<specific strength 1>"", ""<specific strength 2>"", ""<specific strength 3>"", ""<strength 4>""],
  ""weaknesses"": [""<specific weakness 1>"", ""<specific weakness 2>"", ""<weakness 3>""],
  ""recommendations"": [""<actionable recommendation 1>"", ""<recommendation 2>"", ""<recommendation 3>"", ""<recommendation 4>""],
  ""techStack"": [""<tech1>"", ""<tech2>"", ""<tech3>""],
  ""softSkills"": [""<skill1>"", ""<skill2>"", ""<skill3>""],
  ""formatting"": {{ ""score"": <0-100>, ""feedback"": ""<specific formatting feedback>"" }},
  ""content"":    {{ ""score"": <0-100>, ""feedback"": ""<specific content feedback>"" }},
  ""impact"":     {{ ""score"": <0-100>, ""feedback"": ""<specific impact/achievement feedback>"" }}
}}

For skillCategories include 6-8 key skill areas with realistic proficiency levels.
Use colors: #7C3AED, #06B6D4, #10B981, #F59E0B, #EF4444, #8B5CF6, #EC4899, #3B82F6.
ATS score: penalize missing keywords, poor formatting, lack of metrics, gaps.

RESUME TO ANALYZE:
{resume.RawText}";

            var json = await CallGeminiAsync(prompt, 8192);
            return ParseAnalysisResult(json);
        }

        // ══════════════════════════════════════════════════════════
        //  JOB MATCHING
        // ══════════════════════════════════════════════════════════
        public async Task<MatchResult> MatchJobAsync(ResumeData resume, JobDescription job)
        {
            var prompt = $@"You are an expert AI recruiter and talent acquisition specialist.
Deeply analyze the resume against the job description and return ONLY a valid JSON object.
No markdown, no explanation, no code blocks — just raw JSON.

Return this exact structure:
{{
  ""overallMatchScore"": <integer 0-100>,
  ""skillMatchScore"": <integer 0-100>,
  ""experienceMatchScore"": <integer 0-100>,
  ""educationMatchScore"": <integer 0-100>,
  ""keywordMatchScore"": <integer 0-100>,
  ""matchedSkills"": [""<skill1>"", ""<skill2>""],
  ""missingSkills"": [""<skill1>"", ""<skill2>""],
  ""bonusSkills"": [""<extra skill candidate has beyond requirements>""],
  ""matchSummary"": ""<3-4 sentence detailed match summary>"",
  ""hiringOutlook"": ""<Strong|Good|Fair|Weak>"",
  ""improvementTips"": [""<specific tip1>"", ""<specific tip2>"", ""<specific tip3>""],
  ""interviewTopics"": [""<topic1>"", ""<topic2>"", ""<topic3>"", ""<topic4>""],
  ""salaryEstimate"": ""<e.g. $90,000 - $120,000/yr>"",
  ""alternativeTitles"": [""<similar role title 1>"", ""<similar role 2>""]
}}

Be realistic with scores. A perfect match is rare; most candidates score 60-85%.

RESUME:
{resume.RawText}

JOB TITLE: {job.Title}
COMPANY: {job.Company}
JOB DESCRIPTION:
{job.RawText}";

            var json = await CallGeminiAsync(prompt, 8192);
            return ParseMatchResult(json);
        }

        // ══════════════════════════════════════════════════════════
        //  COVER LETTER GENERATOR
        // ══════════════════════════════════════════════════════════
        public async Task<string> GenerateCoverLetterAsync(
            ResumeData resume, JobDescription job, string candidateName)
        {
            var name = string.IsNullOrWhiteSpace(candidateName) ? "Candidate" : candidateName;
            var prompt = $@"You are an expert career coach and professional cover letter writer.
Write a compelling, highly personalized cover letter for this candidate.

Requirements:
- Professional, confident, and enthusiastic tone
- Reference specific job requirements from the posting and match them to real experience
- Highlight 2-3 specific, quantifiable achievements or skills from the resume
- Structure: engaging opening hook → skills/experience alignment → key achievement → strong forward-looking close
- Opening: ""Dear Hiring Team,"" (or use hiring manager if you can infer it)
- Closing: ""Sincerely,"" followed by the candidate's name on next line
- Length: 3-4 solid paragraphs, no fluff, no generic phrases like ""I am writing to apply""
- Do NOT use square brackets or placeholders — write the full letter with real content

CANDIDATE NAME: {name}
JOB TITLE: {job.Title}
COMPANY: {job.Company}

RESUME:
{resume.RawText}

JOB DESCRIPTION:
{job.RawText}

Write only the cover letter. No preamble, no commentary after.";

            return await CallGeminiAsync(prompt, 1500);
        }

        // ══════════════════════════════════════════════════════════
        //  JOB FINDER
        //  Works with manual inputs OR auto-fills from analysis result.
        // ══════════════════════════════════════════════════════════
        public async Task<List<JobListing>> FindMatchingJobsAsync(
            string role, string location, string skills, string experienceLevel,
            ResumeData? resume = null)
        {
            var loc = string.IsNullOrWhiteSpace(location) ? "Remote or any location" : location;

            var resumeSection = resume != null && !string.IsNullOrWhiteSpace(resume.RawText)
                ? $"\nRESUME EXCERPT:\n{(resume.RawText.Length > 1000 ? resume.RawText[..1000] : resume.RawText)}"
                : "";

            var prompt = $@"You are an expert job search specialist with deep knowledge of the current global job market.
Based on the candidate profile below, generate 8 highly relevant, realistic job opportunities.

Use real company names, authentic job titles, and realistic salary ranges based on current market data.
Every listing must be a plausible opportunity at a real company that is known to hire for these skills.

Return ONLY a valid JSON array — no markdown fences, no preamble, no trailing text. Start with [ and end with ].

[
  {{
    ""title"": ""<exact realistic job title>"",
    ""company"": ""<real company name>"",
    ""location"": ""<City, Country or Remote>"",
    ""jobType"": ""<Full-time|Remote|Hybrid|Contract>"",
    ""salary"": ""<e.g. $95,000 - $130,000/yr or PKR 250,000 - 350,000/mo>"",
    ""description"": ""<2-3 sentence job description highlighting key responsibilities>"",
    ""applyUrl"": ""<plausible careers page URL>"",
    ""matchScore"": <integer 72-96>,
    ""requiredSkills"": [""<skill1>"", ""<skill2>"", ""<skill3>"", ""<skill4>""],
    ""postedDate"": ""<1 day ago|3 days ago|1 week ago|2 weeks ago>"",
    ""source"": ""<LinkedIn|Indeed|Glassdoor|Company Careers>""
  }}
]

CANDIDATE PROFILE:
- Target Role: {(string.IsNullOrWhiteSpace(role) ? "Software Engineer" : role)}
- Experience Level: {(string.IsNullOrWhiteSpace(experienceLevel) ? "Mid-level" : experienceLevel)}
- Key Skills: {(string.IsNullOrWhiteSpace(skills) ? "Programming, Problem Solving" : skills)}
- Preferred Location: {loc}
{resumeSection}

Rules:
- Sort listings by matchScore descending
- Mix well-known companies (FAANG, Fortune 500) with strong mid-size and regional tech companies
- Keep salary realistic for the location (use local currency if location is not USA/UK)
- Each listing must be distinct — no duplicate companies
- Output exactly 8 listings";

            var json = await CallGeminiAsync(prompt, 8192);
            return ParseJobListings(json);
        }

        // ══════════════════════════════════════════════════════════
        //  HTTP — STANDARD GEMINI CALL (with automatic model fallback)
        // ══════════════════════════════════════════════════════════
        private async Task<string> CallGeminiAsync(string prompt, int maxTokens = 8192)
        {
            // Try models in order — each has its own separate quota pool
            foreach (var model in new[] { ModelPrimary, ModelFallback, "gemini-2.0-flash", "gemini-2.5-flash" })
            {
                var url  = $"{ApiBase}{model}:generateContent?key={_apiKey}";
                var body = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    },
                    generationConfig = new
                    {
                        temperature     = 0.2,
                        maxOutputTokens = maxTokens
                    }
                };

                var content  = new StringContent(
                    JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var response = await _http.PostAsync(url, content);
                var raw      = await response.Content.ReadAsStringAsync();

                // 503 / 429 — quota or overload, try next model
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable
                    || response.StatusCode == System.Net.HttpStatusCode.TooManyRequests
                    || raw.Contains("\"UNAVAILABLE\"")
                    || raw.Contains("RESOURCE_EXHAUSTED"))
                    continue;

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Gemini API error {response.StatusCode}: {raw}");

                var obj   = JObject.Parse(raw);
                var parts = obj["candidates"]?[0]?["content"]?["parts"] as JArray;

                // Gemini 2.5 Flash may return thinking parts (marked "thought": true) before
                // the actual response. Find the last non-thought text part.
                string? text = null;
                if (parts != null)
                {
                    for (int i = parts.Count - 1; i >= 0; i--)
                    {
                        var p = parts[i];
                        var isThought = p["thought"]?.ToObject<bool>() ?? false;
                        if (!isThought)
                        {
                            text = p["text"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(text)) break;
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(text))
                    throw new Exception("Unexpected Gemini response format.");

                return CleanJson(text);
            }

            throw new Exception(
                "Gemini is experiencing high demand right now. Please wait a moment and try again.");
        }

        private static string CleanJson(string text)
        {
            text = text.Trim();
            if (text.StartsWith("```json")) text = text.Substring(7);
            if (text.StartsWith("```"))     text = text.Substring(3);
            if (text.EndsWith("```"))       text = text.Substring(0, text.LastIndexOf("```"));
            return SanitizeJsonStrings(text.Trim());
        }

        // Escape literal control characters inside JSON string values.
        // Gemini 2.5 sometimes emits raw \n or \r inside a quoted value,
        // which is invalid JSON and causes "Unterminated string" errors.
        private static string SanitizeJsonStrings(string json)
        {
            var sb = new System.Text.StringBuilder(json.Length);
            bool inString = false;
            bool escaped  = false;

            foreach (char c in json)
            {
                if (escaped)
                {
                    sb.Append(c);
                    escaped = false;
                    continue;
                }

                if (c == '\\' && inString)
                {
                    sb.Append(c);
                    escaped = true;
                    continue;
                }

                if (c == '"')
                {
                    inString = !inString;
                    sb.Append(c);
                    continue;
                }

                if (inString)
                {
                    switch (c)
                    {
                        case '\n': sb.Append("\\n");  break;
                        case '\r': sb.Append("\\r");  break;
                        case '\t': sb.Append("\\t");  break;
                        default:   sb.Append(c);      break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        // ══════════════════════════════════════════════════════════
        //  PARSERS
        // ══════════════════════════════════════════════════════════
        private static AnalysisResult ParseAnalysisResult(string json)
        {
            var j = JObject.Parse(json);
            return new AnalysisResult
            {
                AtsScore          = j["atsScore"]?.ToObject<int>() ?? 0,
                OverallSummary    = j["overallSummary"]?.ToString() ?? "",
                ExperienceLevel   = j["experienceLevel"]?.ToString() ?? "",
                YearsOfExperience = j["yearsOfExperience"]?.ToObject<int>() ?? 0,
                PrimaryDomain     = j["primaryDomain"]?.ToString() ?? "",
                SkillCategories   = ParseList<SkillCategory>(j["skillCategories"]),
                Strengths         = ParseStringList(j["strengths"]),
                Weaknesses        = ParseStringList(j["weaknesses"]),
                Recommendations   = ParseStringList(j["recommendations"]),
                TechStack         = ParseStringList(j["techStack"]),
                SoftSkills        = ParseStringList(j["softSkills"]),
                Formatting = new FormattingScore
                {
                    Score    = j["formatting"]?["score"]?.ToObject<int>() ?? 0,
                    Feedback = j["formatting"]?["feedback"]?.ToString() ?? ""
                },
                Content = new ContentScore
                {
                    Score    = j["content"]?["score"]?.ToObject<int>() ?? 0,
                    Feedback = j["content"]?["feedback"]?.ToString() ?? ""
                },
                Impact = new ImpactScore
                {
                    Score    = j["impact"]?["score"]?.ToObject<int>() ?? 0,
                    Feedback = j["impact"]?["feedback"]?.ToString() ?? ""
                }
            };
        }

        private static MatchResult ParseMatchResult(string json)
        {
            var j = JObject.Parse(json);
            return new MatchResult
            {
                OverallMatchScore    = j["overallMatchScore"]?.ToObject<int>() ?? 0,
                SkillMatchScore      = j["skillMatchScore"]?.ToObject<int>() ?? 0,
                ExperienceMatchScore = j["experienceMatchScore"]?.ToObject<int>() ?? 0,
                EducationMatchScore  = j["educationMatchScore"]?.ToObject<int>() ?? 0,
                KeywordMatchScore    = j["keywordMatchScore"]?.ToObject<int>() ?? 0,
                MatchedSkills        = ParseStringList(j["matchedSkills"]),
                MissingSkills        = ParseStringList(j["missingSkills"]),
                BonusSkills          = ParseStringList(j["bonusSkills"]),
                MatchSummary         = j["matchSummary"]?.ToString() ?? "",
                HiringOutlook        = j["hiringOutlook"]?.ToString() ?? "",
                ImprovementTips      = ParseStringList(j["improvementTips"]),
                InterviewTopics      = ParseStringList(j["interviewTopics"]),
                SalaryEstimate       = j["salaryEstimate"]?.ToString() ?? "",
                AlternativeTitles    = ParseStringList(j["alternativeTitles"])
            };
        }

        private static List<JobListing> ParseJobListings(string json)
        {
            var trimmed = json.Trim();

            // Extract just the JSON array
            var start = trimmed.IndexOf('[');
            var end   = trimmed.LastIndexOf(']');
            if (start >= 0 && end > start)
                trimmed = trimmed.Substring(start, end - start + 1);

            // If JSON was truncated, trim to the last complete object
            if (!trimmed.EndsWith("]"))
            {
                var lastClose = trimmed.LastIndexOf('}');
                if (lastClose >= 0)
                    trimmed = trimmed.Substring(0, lastClose + 1) + "]";
            }

            try
            {
                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling     = NullValueHandling.Ignore
                };
                return JsonConvert.DeserializeObject<List<JobListing>>(trimmed, settings) ?? new();
            }
            catch
            {
                // Last resort: parse object by object
                var results = new List<JobListing>();
                var array   = JArray.Parse(trimmed);
                foreach (var item in array)
                {
                    try { results.Add(item.ToObject<JobListing>()!); }
                    catch { /* skip malformed entries */ }
                }
                return results;
            }
        }

        private static List<string> ParseStringList(JToken? token)
        {
            var list = new List<string>();
            if (token is JArray arr)
                foreach (var item in arr)
                    list.Add(item.ToString());
            return list;
        }

        private static List<T> ParseList<T>(JToken? token)
        {
            if (token is JArray arr)
                return arr.ToObject<List<T>>() ?? new List<T>();
            return new List<T>();
        }
    }
}
