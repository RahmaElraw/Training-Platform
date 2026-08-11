namespace Training_Platform.Repositories.IRepositories
{
    public interface IQuizRepository
    {
        Task<Quiz?> GetQuizForTakingAsync(
            int quizId,
            CancellationToken cancellationToken = default);
    }
}
