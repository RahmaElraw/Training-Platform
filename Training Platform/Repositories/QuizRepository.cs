using Microsoft.EntityFrameworkCore;
using Training_Platform.DataAccess;
using Training_Platform.Models;

namespace Training_Platform.Repositories
{
    public class QuizRepository : IQuizRepository
    {
        private readonly ApplicationDbContext _context;

        public QuizRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Quiz?> GetQuizForTakingAsync(
            int quizId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Quizzes
                .AsNoTracking()
                .Include(q => q.Questions)
                .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(
                    q => q.Id == quizId,
                    cancellationToken);
        }

        public async Task<Quiz?> GetQuizForTrainerDetailsAsync(
            int quizId,
            int trainerId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Quizzes
                .AsNoTracking()
                .Include(q => q.Course)
                .Include(q => q.Questions)
                .Include(q => q.QuizResults)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(
                    q => q.Id == quizId &&
                         q.Course.TrainerId == trainerId,
                    cancellationToken);
        }
    }
}