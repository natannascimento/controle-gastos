namespace ControleGastos.Application.DTOs;

public sealed class PersonTotalsResponseDto
{
    public IReadOnlyList<PersonTotalsItemDto> Items { get; set; } = [];
    public TotalsSummaryDto Summary { get; set; } = new();
}
