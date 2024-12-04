﻿using L_Bank_W_Backend.DbAccess.Repositories;
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
        try
        {
            var success = await bookingRepository.Book(booking.SourceId, booking.DestinationId, booking.Amount,
                new CancellationToken());
            return success ? Ok($"Successfully transferred {booking.Amount} from ledger {booking.SourceId} to ledger {booking.DestinationId} ledger") : Conflict("Unable to process transaction.");
        }
        catch (Exception ex)
        {
            return Problem("An Error occurred while processing the booking transaction ");
        }
    }
}