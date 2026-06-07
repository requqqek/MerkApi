using MerkApi.Models;

namespace MerkApi.Services;

public class AssignmentChecker
{
    public (int grade, int maxGrade, string comment) CheckAnswer(Assignment assignment, string studentAnswer)
    {
        return assignment.Type switch
        {
            "SingleChoice" => CheckSingleChoice(assignment.CorrectAnswer, studentAnswer),
            "MultipleChoice" => CheckMultipleChoice(assignment.CorrectAnswer, studentAnswer),
            "TextInput" => CheckTextInput(assignment.CorrectAnswer, studentAnswer),
            "Code" => CheckCode(assignment.CorrectAnswer, studentAnswer),
            _ => (0, 1, "Неизвестный тип задания")
        };
    }

    private (int, int, string) CheckSingleChoice(string correct, string student)
    {
        if (correct.Trim() == student.Trim())
            return (1, 1, "Правильно!");
        return (0, 1, "Неправильно.");
    }

    private (int, int, string) CheckMultipleChoice(string correct, string student)
    {
        var correctIndices = correct.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).OrderBy(x => x).ToList();
        var studentIndices = student.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).OrderBy(x => x).ToList();

        int maxGrade = correctIndices.Count;

        if (correctIndices.SequenceEqual(studentIndices))
            return (maxGrade, maxGrade, "Все ответы верны!");

        var correctCount = studentIndices.Count(x => correctIndices.Contains(x));
        return (correctCount, maxGrade, $"Верно {correctCount} из {maxGrade} ответов.");
    }

    private (int, int, string) CheckTextInput(string correct, string student)
    {
        if (student.Trim().Equals(correct.Trim(), StringComparison.OrdinalIgnoreCase))
            return (1, 1, "Отлично! Ответ верный.");
        return (0, 1, "Ответ неверный.");
    }

    private (int, int, string) CheckCode(string correct, string student)
    {
        var expectedOutput = correct.Trim().Replace("\r\n", "\n").Replace("\r", "\n");
        var actualOutput = student.Trim().Replace("\r\n", "\n").Replace("\r", "\n");

        if (expectedOutput == actualOutput)
            return (1, 1, "Код работает правильно!");

        if (double.TryParse(expectedOutput, out var expectedNum) &&
            double.TryParse(actualOutput, out var actualNum))
        {
            if (Math.Abs(expectedNum - actualNum) < 0.001)
                return (1, 1, "Ответ верный!");
        }

        return (0, 1, $"Ожидалось: {expectedOutput}\nПолучено: {actualOutput}");
    }
}