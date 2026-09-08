namespace Training_Platform.Services
{
    public interface ICertificateService
    {
        Task<string> GenerateCertificateAsync(
            Certificate certificate,
            ApplicationUser user,
            Course course,
            CancellationToken cancellationToken = default);
    }
}