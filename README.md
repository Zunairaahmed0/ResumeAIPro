# ResumeAI Pro

> AI-powered career intelligence desktop app built with WPF and Google Gemini AI.

![Platform](https://img.shields.io/badge/Platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![AI](https://img.shields.io/badge/AI-Google%20Gemini-orange)

---

## What It Does

ResumeAI Pro analyzes your resume using Google's Gemini AI and gives you everything you need to land the job:

- **AI Resume Analysis** — ATS score, strengths, weaknesses, and recommendations
- **ATS Score Optimizer** — Check if your resume passes applicant tracking systems
- **Job Match Finder** — Paste a job description and get a compatibility score
- **AI Cover Letter Generator** — Generates a tailored cover letter in seconds
- **AI Job Finder** — Suggests real job listings matched to your skills
- **History** — Tracks all your past analyses

---

## Screenshots

<!-- Add screenshots here -->
> Coming soon

---

## Getting Started

### Prerequisites

- Windows 10/11
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- A free [Google Gemini API key](https://aistudio.google.com/apikey)
- *(Optional)* Firebase project for user authentication

### Run the App

```bash
git clone https://github.com/YOUR_USERNAME/ResumeAIPro.git
cd ResumeAIPro/ResumeAIPro
dotnet run
```

### Configure API Keys

**Option A — Hardcode in `AppConfig.cs`** (easiest for personal use):
```csharp
public const string GeminiApiKey  = "YOUR_GEMINI_API_KEY";
public const string FirebaseApiKey = "YOUR_FIREBASE_WEB_API_KEY";
```

**Option B — Enter in Settings tab at runtime** (keys are saved locally and persist between sessions).

---

## Tech Stack

| Layer | Technology |
|---|---|
| UI Framework | WPF (.NET 8) |
| MVVM | CommunityToolkit.Mvvm 8.4 |
| UI Components | Material Design Themes 5.3 |
| AI | Google Gemini API (gemini-2.5-flash) |
| Auth | Firebase Authentication REST API |
| PDF Parsing | iText7 |
| DOCX Parsing | DocumentFormat.OpenXml |
| JSON | Newtonsoft.Json |

---

## Project Structure

```
ResumeAIPro/
├── Assets/                  # Images and icons
├── Models/                  # Data models (ResumeData, AnalysisResult, etc.)
├── Services/
│   ├── AIAnalysisService.cs # Gemini AI integration
│   ├── AuthService.cs       # Firebase auth
│   ├── ResumeParserService.cs # PDF/DOCX/TXT parsing
│   ├── ExportService.cs     # Report export
│   ├── HistoryService.cs    # Local history persistence
│   └── SettingsService.cs   # API key storage
├── ViewModels/
│   └── MainViewModel.cs     # All app logic (MVVM)
└── Views/
    ├── MainWindow.xaml       # Main app window (8 tabs)
    └── LoginWindow.xaml      # Sign in / register / guest
```

---

## Features in Detail

### Resume Upload
Supports **PDF**, **DOCX**, and **TXT** files. Drag and drop or click to browse.

### AI Analysis
Powered by Gemini 2.5 Flash. Returns:
- ATS compatibility score (0–100)
- Formatting, content, and impact sub-scores
- Tech stack and soft skills extraction
- Top strengths and improvement recommendations
- Experience level detection

### Job Matching
Paste any job description. The AI compares it against your resume and returns an overall match score plus per-category breakdowns.

### Cover Letter
Auto-generates a personalized cover letter using your resume data and the target job description.

### Job Finder
AI suggests job listings tailored to your skills, role, location, and experience level.

---

## Authentication

- Sign in / register with **email + password** via Firebase Authentication
- **Guest mode** — all features available without an account
- Session persists between app launches

---

## License

MIT — free to use, modify, and distribute.

---

*Built with Google Gemini AI · Firebase Auth · Material Design*
