using Training_Platform.ViewModels;

namespace Training_Platform.Repositories.IRepositories
{
    public interface IProgressRepository
    {
        Task<IEnumerable<TrainerProgressVM>> GetTrainerProgressAsync(
    int trainerId,
    int? courseId,
    CancellationToken cancellationToken = default);
    }
}