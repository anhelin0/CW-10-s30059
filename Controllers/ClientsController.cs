using Microsoft.AspNetCore.Mvc;
using Trip.Data;
using Trip.Exceptions;
using Trip.Services;

namespace Trip.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly IDbService _dbService;

    public ClientsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClientAsync(int idClient)
    {
        try
        {
            await _dbService.DeleteClientAsync(idClient);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return BadRequest($"Klient o id {idClient} jest przypisany do co najmniej jednej wycieczki");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    
}


