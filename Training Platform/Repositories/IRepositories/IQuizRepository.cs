public interface IQuizRepository
{
    Task<Quiz?> GetQuizForTakingAsync(
        int quizId,
        CancellationToken cancellationToken = default);

    Task<Quiz?> GetQuizForTrainerDetailsAsync(
        int quizId,
        int trainerId,
        CancellationToken cancellationToken = default);
}