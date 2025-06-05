using System.Globalization;
using System.Transactions;
using Trip.DTOs;

namespace Trip.Controllers;

using Microsoft.AspNetCore.Mvc;
using Trip.Data;
using Trip.Models;
using Trip.Services;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly IDbService _dbService;

    public TripsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        return Ok(await _dbService.GetTripsAsync(pageNumber, pageSize));
    }
    
    [HttpPost]
    [Route("{idTrip}/clients")]
    public async Task<IActionResult> AddClientToTrip(AddClientDto newTripsClient, int idTrip)
    {
        try
        {
            await _dbService.CheckClient(newTripsClient.Pesel);
            await _dbService.CheckIsClientAssignedToTrip(newTripsClient.Pesel);
            await _dbService.CheckTripExists(idTrip);
            await _dbService.CheckTripDateFrom(idTrip);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

        DateTime? paymentDate = null;
        if (!string.IsNullOrWhiteSpace(newTripsClient.PaymentDate))
        {
            string[] formats =
            {
                "yyyy-MM-dd",
                "MM/dd/yyyy",
                "dd/MM/yyyy",
                "M/d/yyyy",
                "MM/d/yyyy",
                "d/M/yyyy",
                "dd/M/yyyy",
                "yyyy/MM/dd",
                "dd.MM.yyyy",
                "M.d.yyyy",
                "yyyy.MM.dd",
                "yyyyMMdd",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ssZ"
            };

            if (!DateTime.TryParseExact(
                    newTripsClient.PaymentDate,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsedDate))
            {
                return BadRequest("Invalid date format for PaymentDate.");
            }

            paymentDate = parsedDate;
        }
        
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var clientId = await _dbService.AddClient(new Client()
            {
                FirstName = newTripsClient.FirstName,
                LastName = newTripsClient.LastName,
                Email = newTripsClient.Email,
                Telephone = newTripsClient.Telephone,
                Pesel = newTripsClient.Pesel
            });
            await _dbService.AddClientToTrip(clientId, idTrip, paymentDate);
            
            scope.Complete();
        }
        return Created();
    }
    
}