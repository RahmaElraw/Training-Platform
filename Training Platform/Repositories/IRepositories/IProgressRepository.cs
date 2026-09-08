using Training_Platform.ViewModels;

namespace Training_Platform.Repositories.IRepositories
{
    public interface IProgressRepository
    {
        Task<IEnumerable<TrainerProgressVM>> GetTrainerProgressAsync(
            int trainerId,
            CancellationToken cancellationToken = default);
    }
}