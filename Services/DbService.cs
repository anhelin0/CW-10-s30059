namespace Trip.Services;

using Trip.Data;
using Trip.DTOs;
using Trip.Exceptions;
using Trip.Models;
using Microsoft.EntityFrameworkCore;

public interface IDbService
{
    Task<TripsPageDto> GetTripsAsync(int pageNum, int pageSize = 10);
    Task DeleteClientAsync(int id);
    Task CheckClient(String pesel);
    Task CheckIsClientAssignedToTrip(String pesel);
    Task CheckTripExists(int id);
    Task CheckTripDateFrom(int id);
    Task<int> AddClient (Client client);
    Task AddClientToTrip(int idClient, int idTrip, DateTime? paymentDate);
}

public class DbService : IDbService
{
    private readonly TripContext _context;
    public DbService(TripContext context)
    {
        _context = context;
    }

    public async Task<TripsPageDto> GetTripsAsync(int pageNum = 1, int pageSize = 10)
    {
        if (pageNum < 1) pageNum = 1;
        if (pageSize < 1) pageSize = 10;
        var allTrips = await _context.Trips.CountAsync();
        var allPages = (int) Math.Ceiling(allTrips / (double) pageSize);

        var trips = await _context.Trips
            .OrderByDescending(t => t.DateFrom)
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .Select(trip => new TripDto()
        {
            Name = trip.Name,
            Description = trip.Description,
            DateFrom = trip.DateFrom,
            DateTo = trip.DateTo,
            MaxPeople = trip.MaxPeople,
            Countries = trip.IdCountries.Select(c => new CountryDto()
            {
                Name = c.Name,
            }).OrderBy(c => c.Name).ToList(),
            Clients = trip.ClientTrips.Select(a => a.IdClientNavigation).Select(c => new ClientDto()
            {
                FirstName = c.FirstName,
                LastName = c.LastName,
            }).ToList()
        }).ToListAsync();
        
        return new TripsPageDto()
        {
            PageNum = pageNum,
            PageSize = pageSize,
            AllPages = allPages, 
            Trips = trips
        };
    }

    public async Task DeleteClientAsync(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null)
        {
            throw new NotFoundException($"Klient o id {id} nie istnieje");
        }

        if (_context.ClientTrips.Any(ct => ct.IdClient == client.IdClient))
        {
            throw new NotFoundException($"Klient o id {id} jest przypisany do co najmniej jednej wycieczki");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
    }

    public async Task CheckClient(String pesel)
    {
        if (await _context.Clients.AnyAsync(c => c.Pesel == pesel))
        {
            throw new NotFoundException("Klient o podanym numerze PESEL już istnieje");
        }
    }

    public async Task CheckIsClientAssignedToTrip(String pesel)
    {
        if (await _context.ClientTrips.Select(c => c.IdClientNavigation).AnyAsync(c => c.Pesel == pesel))
        {
            throw new NotFoundException("Klient o podanym numerze PESEL jest już zapisany na daną wycieczkę");
        }
    }

    public async Task CheckTripExists(int id)
    {
        if (!await _context.Trips.AnyAsync(t => t.IdTrip == id))
        {
            throw new NotFoundException($"Wycieczka o id {id} nie istnieje");
        }
    }
    
    public async Task CheckTripDateFrom(int id)
    {
        if (await _context.Trips.Where(t => t.IdTrip == id).Where(t => t.DateFrom < DateTime.Now).AnyAsync())
        {
            throw new NotFoundException($"Wycieczka o id {id} już się odbyła");
        }
    }

    public async Task<int> AddClient(Client client)
    {
        var id = await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();
        return id.Entity.IdClient;
    }
    
    public async Task AddClientToTrip(int idClient, int idTrip, DateTime? paymentDate)
    {
        var id = await _context.ClientTrips.AddAsync(new ClientTrip()
        {
            IdClient = idClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = paymentDate
        });
        await _context.SaveChangesAsync();
    }
    
    
}