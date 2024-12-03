using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Repositories;
using L_Bank_W_Backend.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L_Bank_W_Backend.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class LedgersController(ILedgerRepository ledgerRepository) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Administrators,Users")]
    public async Task<IEnumerable<Ledger>> Get()
    {
        var allLedgers = await ledgerRepository.GetAllLedgers(new CancellationToken());
        return allLedgers;
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrators,Users")]
    public async Task<Ledger?> Get(int id)
    {
        var ledger = await ledgerRepository.SelectOneAsync(id, new CancellationToken());
        return ledger;
    }

    [HttpPost]
    [Authorize(Roles = "Administrators")]
    public async Task<IActionResult> Post([FromBody] CreateLedgerDto ledger)
    {
        if (string.IsNullOrEmpty(ledger.Name))
        {
            return BadRequest("Ledger name is required");
        }

        var id = await ledgerRepository.CreateLedger(ledger.Name);
        return Ok(id);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrators")]
    public void Put(int id, [FromBody] Ledger ledger)
    {
        ledgerRepository.Update(ledger);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrators")]
    public async Task<IActionResult> Delete(int id)
    {
        // TODO: Error handling
        var success = await ledgerRepository.DeleteLedger(id, new CancellationToken());

        if (!success) return NotFound();

        return Ok(new { Message = $"Ledger with ID {id} deleted successfully." });
    }
}