namespace ControleGastos.Application.DTOs;

public sealed class CategoryTotalsResponseDto
{
    public IReadOnlyList<CategoryTotalsItemDto> Items { get; set; } = [];
    public TotalsSummaryDto Summary { get; set; } = new();
}
