namespace Trip.Models;

public class ClientTrip
{
    public int IdClient { get; set; }
    public Client Client { get; set; }
    public int IdTrip { get; set; }
    public Trip Trip { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? PaymentDate { get; set; }
    
    public virtual Client IdClientNavigation { get; set; } = null!;
    public virtual Trip IdTripNavigation { get; set; } = null!;
}