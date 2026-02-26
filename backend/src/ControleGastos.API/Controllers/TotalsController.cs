using ControleGastos.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControleGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TotalsController(TotalsService totalsService) : ControllerBase
{
    [HttpGet("persons")]
    public async Task<IActionResult> GetTotalsByPerson()
        => Ok(await totalsService.GetTotalsByPersonAsync());

    [HttpGet("categories")]
    public async Task<IActionResult> GetTotalsByCategory()
        => Ok(await totalsService.GetTotalsByCategoryAsync());
}
