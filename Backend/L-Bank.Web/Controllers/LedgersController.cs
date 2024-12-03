using Microsoft.EntityFrameworkCore;
using L_Bank_W_Backend.Core.Models;
using Microsoft.EntityFrameworkCore;
using L_Bank_W_Backend.DbAccess.Repositories;
using L_Bank_W_Backend.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L_Bank_W_Backend.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LedgersController(ILedgerRepository ledgerRepository) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Administrators,Users")]
        public async Task<IEnumerable<Ledger>> Get()
        {
            var allLedgers = await ledgerRepository.GetAllLedgers();
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
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { Error = "Invalid ledger ID. ID must be a positive integer." });
                }

                var cancellationToken = new CancellationToken();
                var success = await ledgerRepository.DeleteLedger(id, cancellationToken);

                if (!success)
                {
                    return NotFound(new { Error = $"Ledger with ID {id} was not found or could not be deleted." });
                }

                return Ok(new { Message = $"Ledger with ID {id} deleted successfully." });
            }
            catch (DbUpdateException ex)
            {
                // Handles database-specific exceptions, such as foreign key constraint violations
                return StatusCode(500, new { Error = "An error occurred while interacting with the database.", Details = ex.Message });
            }
            catch (OperationCanceledException)
            {
                // Handles cases where the operation is canceled
                return StatusCode(500, new { Error = "The operation was canceled." });
            }
            catch (Exception ex)
            {
                // Handles all other unexpected exceptions
                return StatusCode(500, new { Error = "An unexpected error occurred.", Details = ex.Message });
            }
        }
    }
}
