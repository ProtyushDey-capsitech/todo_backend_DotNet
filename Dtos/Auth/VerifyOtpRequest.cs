namespace Projects.Dtos.Auth
{
    public class VerifyOtpRequest
    {
        public string sessionId { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
}
