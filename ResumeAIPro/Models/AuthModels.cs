using System;

namespace ResumeAIPro.Models
{
    public class UserSession
    {
        public string Email        { get; set; } = "";
        public string DisplayName  { get; set; } = "";
        public string LocalId      { get; set; } = "";
        public string IdToken      { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public DateTime ExpiresAt  { get; set; } = DateTime.MinValue;
        public bool IsGuest        { get; set; }

        public string Initials
        {
            get
            {
                if (IsGuest) return "G";
                if (!string.IsNullOrEmpty(DisplayName))
                {
                    var parts = DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    return parts.Length > 1
                        ? $"{parts[0][0]}{parts[^1][0]}".ToUpper()
                        : DisplayName[0].ToString().ToUpper();
                }
                return Email.Length > 0 ? Email[0].ToString().ToUpper() : "?";
            }
        }

        public string ShortName => IsGuest ? "Guest"
            : !string.IsNullOrEmpty(DisplayName) ? DisplayName.Split(' ')[0]
            : Email.Split('@')[0];
    }
}
