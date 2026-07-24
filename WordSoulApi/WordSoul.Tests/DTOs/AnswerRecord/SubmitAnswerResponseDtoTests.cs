using System.Text.Json;
using FluentAssertions;
using WordSoul.Application.DTOs.AnswerRecord;

namespace WordSoul.Tests.DTOs.AnswerRecord;

public class SubmitAnswerResponseDtoTests
{
    [Fact]
    public void JsonContract_ExposesOnlyNewStageIndex()
    {
        var response = new SubmitAnswerResponseDto
        {
            NewStageIndex = 2
        };

        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        root.GetProperty("newStageIndex").GetInt32().Should().Be(2);
        root.TryGetProperty("newLevel", out _).Should().BeFalse();
    }
}
