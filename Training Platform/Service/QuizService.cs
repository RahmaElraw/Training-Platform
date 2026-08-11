using System.Linq.Expressions;
using Training_Platform.DTO;
using Training_Platform.Service.IService;

namespace Training_Platform.Services
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IRepository<QuizResult> _quizResultRepository;

        public QuizService(
            IQuizRepository quizRepository,
            IRepository<QuizResult> quizResultRepository)
        {
            _quizRepository = quizRepository;
            _quizResultRepository = quizResultRepository;
        }

        public async Task<TakeQuizViewModel?> GetQuizForTakingAsync(
            int quizId,
            CancellationToken cancellationToken = default)
        {
            var quiz = await _quizRepository.GetQuizForTakingAsync(
                quizId,
                cancellationToken);

            if (quiz == null)
                return null;

            return new TakeQuizViewModel
            {
                QuizId = quiz.Id,
                QuizTitle = quiz.Title,

                Questions = quiz.Questions.Select(q => new QuestionViewModel
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,

                    Options = q.QuestionOptions.Select(o => new OptionViewModel
                    {
                        Id = o.Id,
                        OptionText = o.OptionText

                    }).ToList()

                }).ToList()
            };
        }

        public async Task<int?> SubmitQuizAsync(
            int quizId,
            int userId,
            SubmitQuizDto dto,
            CancellationToken cancellationToken = default)
        {
            var quiz = await _quizRepository.GetQuizForTakingAsync(
                quizId,
                cancellationToken);

            if (quiz == null)
                return null;

            if (!quiz.Questions.Any())
                return null;

            int totalMarks = quiz.Questions.Sum(q => q.Mark);
            int earnedMarks = 0;

            foreach (var question in quiz.Questions)
            {
                var answer = dto.Answers
                    .FirstOrDefault(a => a.QuestionId == question.Id);

                if (answer == null)
                    continue;

                var selectedOption = question.QuestionOptions
                    .FirstOrDefault(o => o.Id == answer.SelectedOptionId);

                if (selectedOption != null && selectedOption.IsCorrect)
                {
                    earnedMarks += question.Mark;
                }
            }

            int score = totalMarks == 0
                ? 0
                : (int)Math.Round(
                    (double)earnedMarks / totalMarks * 100);

            bool isPassed = score >= quiz.PassingScore;

            var result = new QuizResult
            {
                QuizId = quizId,
                UserId = userId,
                Score = score,
                IsPassed = isPassed,
                SubmittedAt = DateTime.UtcNow
            };

            await _quizResultRepository.AddAsync(
                result,
                cancellationToken);

            await _quizResultRepository.CommitAsync(
                cancellationToken);

            return result.Id;
        }

        public async Task<QuizResultViewModel?> GetResultAsync(
            int resultId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var result = await _quizResultRepository.GetOneAsync(
                r => r.Id == resultId && r.UserId == userId,
                new Expression<Func<QuizResult, object>>[]
                {
                    r => r.Quiz
                },
                tracked: false,
                cancellationToken);

            if (result == null)
                return null;

            return new QuizResultViewModel
            {
                QuizTitle = result.Quiz.Title,
                Score = result.Score,
                IsPassed = result.IsPassed,
                SubmittedAt = result.SubmittedAt
            };
        }

        public async Task<IEnumerable<QuizResultViewModel>> GetMyResultsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var results = await _quizResultRepository.GetAsync(
                r => r.UserId == userId,
                new Expression<Func<QuizResult, object>>[]
                {
                    r => r.Quiz
                },
                tracked: false,
                cancellationToken);

            return results.Select(r => new QuizResultViewModel
            {
                QuizTitle = r.Quiz.Title,
                Score = r.Score,
                IsPassed = r.IsPassed,
                SubmittedAt = r.SubmittedAt
            });
        }
    }
}
