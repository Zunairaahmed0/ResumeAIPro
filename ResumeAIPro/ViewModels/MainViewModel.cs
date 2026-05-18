using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ResumeAIPro.Models;
using ResumeAIPro.Services;

namespace ResumeAIPro.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // ── Services ──────────────────────────────────────────────
        private readonly ResumeParserService _parser   = new();
        private readonly SettingsService     _settings = new();
        private readonly HistoryService      _history  = new();
        private readonly ExportService       _export   = new();
        private AIAnalysisService?           _ai;

        // ── Navigation ────────────────────────────────────────────
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentTabTitle))]
        private int _currentTabIndex = 0;

        // ── Auth ──────────────────────────────────────────────────
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DashboardGreeting))]
        private UserSession? _currentUser;
        [ObservableProperty] private string _firebaseApiKey = "";

        // ── Loading / Status ──────────────────────────────────────
        [ObservableProperty] private bool   _isLoading      = false;
        [ObservableProperty] private string _loadingMessage = "Processing...";
        [ObservableProperty] private string _statusMessage  = "Ready";
        [ObservableProperty] private bool   _hasError       = false;
        [ObservableProperty] private string _errorMessage   = "";

        // ── Settings ──────────────────────────────────────────────
        [ObservableProperty] private string _apiKey = "";

        // ── Resume ────────────────────────────────────────────────
        [ObservableProperty] private ResumeData?     _resumeData    = null;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AtsScoreColor))]
        [NotifyPropertyChangedFor(nameof(DashboardSkillsCount))]
        [NotifyPropertyChangedFor(nameof(DashboardScoreLabel))]
        [NotifyPropertyChangedFor(nameof(DashboardTopStrengths))]
        [NotifyPropertyChangedFor(nameof(DashboardTopWeaknesses))]
        private AnalysisResult? _analysisResult = null;
        [ObservableProperty] private bool   _resumeUploaded = false;
        [ObservableProperty] private bool   _analysisReady  = false;
        [ObservableProperty] private string _uploadDropText = "Drop your resume here\nor click to browse";

        // ── Job Matching ──────────────────────────────────────────
        [ObservableProperty] private string      _jobTitle       = "";
        [ObservableProperty] private string      _jobCompany     = "";
        [ObservableProperty] private string      _jobDescription = "";
        [ObservableProperty] private MatchResult? _matchResult   = null;
        [ObservableProperty] private bool  _matchReady = false;

        // ── Cover Letter ──────────────────────────────────────────
        [ObservableProperty] private string _candidateName        = "";
        [ObservableProperty] private string _generatedCoverLetter = "";
        [ObservableProperty] private bool   _coverLetterReady     = false;

        // ── Job Finder ────────────────────────────────────────────
        [ObservableProperty] private string _jobSearchRole     = "";
        [ObservableProperty] private string _jobSearchLocation = "";
        [ObservableProperty] private string _jobSearchSkills   = "";
        [ObservableProperty] private string _jobSearchExp      = "Mid-level";
        [ObservableProperty] private bool   _jobsFound         = false;
        [ObservableProperty] private bool   _jobFinderLoading  = false;
        public ObservableCollection<JobListing> JobListings { get; } = new();

        // ── History ───────────────────────────────────────────────
        public ObservableCollection<AnalysisHistoryEntry> History { get; } = new();

        // ── Computed ─────────────────────────────────────────────
        public string AtsScoreColor   => GetScoreColor(AnalysisResult?.AtsScore ?? 0);
        public string MatchScoreColor => GetScoreColor(MatchResult?.OverallMatchScore ?? 0);
        public string CurrentTabTitle => CurrentTabIndex switch
        {
            0 => "Dashboard",
            1 => "Upload Resume",
            2 => "Analysis",
            3 => "Job Match",
            4 => "Cover Letter",
            5 => "Job Finder",
            6 => "History",
            7 => "Settings",
            _ => "ResumeAI Pro"
        };

        // ── Dashboard Computed ────────────────────────────────────
        public string DashboardGreeting
        {
            get
            {
                var hour  = DateTime.Now.Hour;
                var greet = hour < 12 ? "Good morning" : hour < 17 ? "Good afternoon" : "Good evening";
                var name  = CurrentUser?.DisplayName ?? "there";
                return $"{greet}, {name}!";
            }
        }

        public int DashboardSkillsCount
            => (AnalysisResult?.TechStack?.Count ?? 0) + (AnalysisResult?.SoftSkills?.Count ?? 0);

        public string DashboardScoreLabel => AnalysisResult?.AtsScore switch
        {
            null  => "Run analysis to see your score",
            >= 90 => "Outstanding!",
            >= 80 => "Great Score!",
            >= 70 => "Good Score",
            >= 60 => "Fair Score",
            >= 40 => "Needs Work",
            _     => "Low Score"
        };

        public IReadOnlyList<string> DashboardTopStrengths
            => AnalysisResult?.Strengths?.Take(3).ToList() ?? (IReadOnlyList<string>)Array.Empty<string>();

        public IReadOnlyList<string> DashboardTopWeaknesses
            => ((AnalysisResult?.Weaknesses?.Take(2) ?? Enumerable.Empty<string>())
                .Concat(AnalysisResult?.Recommendations?.Take(1) ?? Enumerable.Empty<string>()))
               .ToList();

        // ── Events ────────────────────────────────────────────────
        public event EventHandler? RequestLogout;

        // ── Constructor ───────────────────────────────────────────
        public MainViewModel()
        {
            ApiKey         = _settings.LoadApiKey();
            FirebaseApiKey = _settings.LoadFirebaseKey();
            CurrentUser    = App.CurrentUser;

            var saved = _history.Load();
            foreach (var entry in saved)
                History.Add(entry);
        }

        // ══════════════════════════════════════════════════════════
        //  COMMANDS
        // ══════════════════════════════════════════════════════════

        [RelayCommand]
        private async Task UploadResume()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Resume Files|*.pdf;*.docx;*.txt|PDF Files|*.pdf|Word Documents|*.docx|Text Files|*.txt",
                Title  = "Select Your Resume"
            };
            if (dialog.ShowDialog() != true) return;
            await ProcessResumeFile(dialog.FileName);
        }

        public async Task ProcessResumeFile(string filePath)
        {
            try
            {
                IsLoading      = true;
                LoadingMessage = "Reading your resume...";
                HasError       = false;
                AnalysisReady  = false;
                MatchReady     = false;
                CoverLetterReady = false;

                ResumeData = await _parser.ParseAsync(filePath);
                ResumeUploaded = true;
                UploadDropText = $"✓  {ResumeData.FileName}";

                if (!string.IsNullOrWhiteSpace(ApiKey))
                    await AnalyzeResume();
                else
                {
                    IsLoading     = false;
                    StatusMessage = "Resume loaded. Enter your Gemini API key in Settings and click Analyze.";
                }
            }
            catch (Exception ex)
            {
                IsLoading    = false;
                HasError     = true;
                ErrorMessage = $"Failed to read file: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task AnalyzeResume()
        {
            if (ResumeData is null)
            {
                HasError = true; ErrorMessage = "Please upload a resume first."; return;
            }
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                HasError = true; ErrorMessage = "Please enter your Gemini API key in Settings."; return;
            }

            try
            {
                IsLoading      = true;
                LoadingMessage = "AI is analyzing your resume...";
                HasError       = false;

                _ai ??= new AIAnalysisService(ApiKey);

                AnalysisResult = await _ai.AnalyzeResumeAsync(ResumeData);
                AnalysisReady  = true;

                // Auto-populate Job Finder fields from analysis result
                if (string.IsNullOrWhiteSpace(JobSearchRole))
                    JobSearchRole = AnalysisResult.PrimaryDomain;
                if (string.IsNullOrWhiteSpace(JobSearchSkills))
                    JobSearchSkills = string.Join(", ", AnalysisResult.TechStack.Take(6));
                JobSearchExp = AnalysisResult.ExperienceLevel;

                StatusMessage   = $"Analysis complete — ATS Score: {AnalysisResult.AtsScore}/100";
                CurrentTabIndex = 2;
            }
            catch (Exception ex)
            {
                HasError     = true;
                ErrorMessage = $"Analysis failed: {ex.Message}";
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task MatchJob()
        {
            if (ResumeData is null)
            {
                HasError = true; ErrorMessage = "Please upload and analyze a resume first."; return;
            }
            if (string.IsNullOrWhiteSpace(JobDescription))
            {
                HasError = true; ErrorMessage = "Please paste a job description."; return;
            }
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                HasError = true; ErrorMessage = "Please enter your Gemini API key in Settings."; return;
            }

            try
            {
                IsLoading      = true;
                LoadingMessage = "Matching your resume to the job...";
                HasError       = false;

                _ai ??= new AIAnalysisService(ApiKey);

                var job = new JobDescription
                {
                    Title   = JobTitle,
                    Company = JobCompany,
                    RawText = JobDescription
                };

                MatchResult = await _ai.MatchJobAsync(ResumeData, job);
                MatchReady  = true;

                OnPropertyChanged(nameof(MatchScoreColor));

                var entry = new AnalysisHistoryEntry
                {
                    ResumeFileName = ResumeData.FileName,
                    JobTitle       = JobTitle.Length > 0 ? JobTitle : "Unnamed Position",
                    AtsScore       = AnalysisResult?.AtsScore ?? 0,
                    MatchScore     = MatchResult.OverallMatchScore
                };
                History.Insert(0, entry);
                _history.Save(History);

                CurrentTabIndex = 3;
                StatusMessage   = $"Match complete — Score: {MatchResult.OverallMatchScore}%";
            }
            catch (Exception ex)
            {
                HasError     = true;
                ErrorMessage = $"Job matching failed: {ex.Message}";
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private async Task GenerateCoverLetter()
        {
            if (ResumeData is null)
            {
                HasError = true; ErrorMessage = "Please upload a resume first."; return;
            }
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                HasError = true; ErrorMessage = "Please enter your Gemini API key in Settings."; return;
            }

            try
            {
                IsLoading      = true;
                LoadingMessage = "Writing your personalized cover letter...";
                HasError       = false;

                _ai ??= new AIAnalysisService(ApiKey);

                var job = new JobDescription
                {
                    Title   = JobTitle,
                    Company = JobCompany,
                    RawText = JobDescription
                };

                GeneratedCoverLetter = await _ai.GenerateCoverLetterAsync(ResumeData, job, CandidateName);
                CoverLetterReady     = true;
                StatusMessage = "Cover letter generated successfully!";
            }
            catch (Exception ex)
            {
                HasError     = true;
                ErrorMessage = $"Cover letter generation failed: {ex.Message}";
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void CopyCoverLetter()
        {
            if (!string.IsNullOrEmpty(GeneratedCoverLetter))
            {
                Clipboard.SetText(GeneratedCoverLetter);
                StatusMessage = "Cover letter copied to clipboard!";
            }
        }

        [RelayCommand]
        private async Task FindJobs()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                HasError = true; ErrorMessage = "Please enter your Gemini API key in Settings."; return;
            }

            // Resolve search parameters — prefer manual inputs, fall back to analysis result
            var role  = !string.IsNullOrWhiteSpace(JobSearchRole)
                ? JobSearchRole
                : AnalysisResult?.PrimaryDomain ?? "";

            var skills = !string.IsNullOrWhiteSpace(JobSearchSkills)
                ? JobSearchSkills
                : AnalysisResult != null
                    ? string.Join(", ", AnalysisResult.TechStack.Take(8))
                    : "";

            var exp = !string.IsNullOrWhiteSpace(JobSearchExp)
                ? JobSearchExp
                : AnalysisResult?.ExperienceLevel ?? "Mid-level";

            if (string.IsNullOrWhiteSpace(role))
            {
                HasError = true;
                ErrorMessage = "Please enter the role you're looking for.";
                return;
            }

            try
            {
                IsLoading        = true;
                JobFinderLoading = true;
                LoadingMessage   = "Searching for matching jobs...";
                HasError         = false;
                JobsFound        = false;
                JobListings.Clear();

                _ai ??= new AIAnalysisService(ApiKey);

                var results = await _ai.FindMatchingJobsAsync(
                    role, JobSearchLocation, skills, exp, ResumeData);
                foreach (var listing in results)
                    JobListings.Add(listing);

                JobsFound     = JobListings.Count > 0;
                StatusMessage = $"Found {JobListings.Count} matching job opportunities!";
            }
            catch (Exception ex)
            {
                HasError     = true;
                ErrorMessage = $"Job search failed: {ex.Message}";
            }
            finally
            {
                IsLoading        = false;
                JobFinderLoading = false;
            }
        }

        [RelayCommand]
        private void OpenJobUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url, UseShellExecute = true
                });
            }
            catch { }
        }

        [RelayCommand]
        private void ExportReport()
        {
            if (AnalysisResult is null)
            {
                HasError = true; ErrorMessage = "No analysis to export. Please analyze a resume first."; return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter   = "Text Files|*.txt|All Files|*.*",
                FileName = $"ResumeAnalysis_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                Title    = "Export Analysis Report"
            };
            if (dialog.ShowDialog() != true) return;

            var job = MatchReady ? new JobDescription
            {
                Title   = JobTitle,
                Company = JobCompany,
                RawText = JobDescription
            } : null;

            _export.ExportReport(ResumeData!, AnalysisResult, MatchResult, job, dialog.FileName);
            StatusMessage = $"Report exported: {Path.GetFileName(dialog.FileName)}";
        }

        [RelayCommand]
        private void SaveApiKey()
        {
            _ai = null;
            _settings.SaveApiKey(ApiKey);
            StatusMessage = "Gemini API key saved!";
        }

        [RelayCommand]
        private void SaveFirebaseApiKey()
        {
            _settings.SaveFirebaseKey(FirebaseApiKey);
            StatusMessage = "Firebase API key saved! Restart the app to enable login.";
        }

        [RelayCommand]
        private void Logout()
        {
            _settings.ClearSession();
            App.CurrentUser = null;
            RequestLogout?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void ClearAll()
        {
            ResumeData           = null;
            AnalysisResult       = null;
            MatchResult          = null;
            ResumeUploaded       = false;
            AnalysisReady        = false;
            MatchReady           = false;
            CoverLetterReady     = false;
            UploadDropText       = "Drop your resume here\nor click to browse";
            JobTitle             = "";
            JobCompany           = "";
            JobDescription       = "";
            GeneratedCoverLetter = "";
            JobsFound            = false;
            JobListings.Clear();
            JobSearchRole        = "";
            JobSearchSkills      = "";
            JobSearchExp         = "Mid-level";
            CurrentTabIndex = 0;
            StatusMessage   = "Ready for a new analysis.";
        }

        // ── Helpers ───────────────────────────────────────────────
        private static string GetScoreColor(int score) => score switch
        {
            >= 80 => "#10B981",
            >= 60 => "#06B6D4",
            >= 40 => "#F59E0B",
            _     => "#EF4444"
        };
    }
}
