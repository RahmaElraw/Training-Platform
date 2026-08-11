namespace Training_Platform.DTO
{
    public class SubmitQuizDto
    {
        public int QuizId { get; set; }

        public List<QuizAnswerDto> Answers { get; set; } = new();
    }

    public class QuizAnswerDto
    {
        public int QuestionId { get; set; }

        public int SelectedOptionId { get; set; }
    }
}
