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
    }
}
