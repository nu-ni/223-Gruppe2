using L_Bank_W_Backend.DbAccess.Repositories;
using L_Bank.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace L_Bank_W_Backend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BookingsController(IBookingRepository bookingRepository) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Administrators")]
    public async Task<IActionResult> Post(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)]
        Booking booking
    )
    {
        // TODO: Error handling
        var success = await bookingRepository.Book(booking.SourceId, booking.DestinationId, booking.Amount, new CancellationToken());
        return success ? Ok() : Conflict();
    }
}