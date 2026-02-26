using ControleGastos.Application.DTOs;
using ControleGastos.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControleGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionController(TransactionService transactionService) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateTransactionDto transactionDto)
    {
        var transactionId = await transactionService.CreateAsync(transactionDto);
        var transaction = await transactionService.GetByIdAsync(transactionId);
        return CreatedAtAction(nameof(GetById), new { id = transactionId }, Map(transaction));
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(Map(await transactionService.GetByIdAsync(id)));
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok((await transactionService.GetAllAsync()).Select(Map));

    private static TransactionResponseDto Map(ControleGastos.Domain.Entities.Transaction transaction)
        => new()
        {
            Id = transaction.Id,
            Description = transaction.Description,
            Value = transaction.Value,
            Date = transaction.Date,
            PersonId = transaction.PersonId,
            CategoryId = transaction.CategoryId,
            Type = transaction.Type
        };
}
