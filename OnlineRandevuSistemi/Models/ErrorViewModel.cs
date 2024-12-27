public class ErrorViewModel
{
    public string RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public string ErrorMessage { get; set; } // Genel hata mesajý
    public Exception Exception { get; set; } // Exception bilgisi
}
