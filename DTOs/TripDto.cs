namespace Trip.DTOs;

public class TripsPageDto
{
    public int PageNum { get; set; }
    public int PageSize { get; set; }
    public int AllPages { get; set; }
    public ICollection<TripDto> Trips { get; set; } = null!;
}

public class TripDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public ICollection<CountryDto> Countries { get; set; } = null!;
    public ICollection<ClientDto> Clients { get; set; } = null!;
    
}

public class CountryDto
{
    public string Name { get; set; }
}

public class ClientDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}