using Microsoft.EntityFrameworkCore;
using Training_Platform.DataAccess;
using Training_Platform.Repositories.IRepositories;
using Training_Platform.ViewModels;

namespace Training_Platform.Repositories
{
    public class ProgressRepository : IProgressRepository
    {
        private readonly ApplicationDbContext _context;

        public ProgressRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TrainerProgressVM>> GetTrainerProgressAsync(
    int trainerId,
    int? courseId,
    CancellationToken cancellationToken = default)
        {
            var result = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.Course.TrainerId == trainerId)
                .Select(e => new TrainerProgressVM
                {
                    EnrollmentId = e.Id,

                    UserId = e.UserId,

                    TraineeName =
                        ((e.User.FirstName ?? "") + " " +
                         (e.User.LastName ?? "")).Trim(),

                    TraineeEmail = e.User.Email,

                    CourseId = e.CourseId,

                    CourseTitle = e.Course.Title,

                    TotalLessons = e.Course.Lessons.Count(),

                    CompletedLessons = e.Course.Lessons
                        .Count(l =>
                            l.UserProgresses.Any(up =>
                                up.UserId == e.UserId &&
                                up.IsCompleted)),

                    IsCompleted = e.IsCompleted,

                    
                })
                .ToListAsync(cancellationToken);

            foreach (var item in result)
            {
                item.ProgressPercentage =
                    item.TotalLessons == 0
                        ? 0
                        : Math.Round(
                            item.CompletedLessons * 100.0 /
                            item.TotalLessons,
                            2);
            }

            return result;
        }
    }
}