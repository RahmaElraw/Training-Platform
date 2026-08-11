using Training_Platform.DTO;

namespace Training_Platform.Service.IService
{
    public interface IQuizService
    {
        Task<TakeQuizViewModel?> GetQuizForTakingAsync(
            int quizId,
            CancellationToken cancellationToken = default);

        Task<int?> SubmitQuizAsync(
            int quizId,
            int userId,
            SubmitQuizDto dto,
            CancellationToken cancellationToken = default);

        Task<QuizResultViewModel?> GetResultAsync(
            int resultId,
            int userId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<QuizResultViewModel>> GetMyResultsAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
