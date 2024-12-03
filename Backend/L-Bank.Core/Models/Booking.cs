using System.ComponentModel.DataAnnotations.Schema;
using L_Bank_W_Backend.Core.Models;

namespace L_Bank.Core.Models;

public class Booking
{
    public const string CollectionName = "Booking";
        
    public int Id { get; set; }
        
    [ForeignKey("Source")]
    public int SourceId { get; set; }
    
    [ForeignKey("Destination")]
    public int DestinationId { get; set; }
        
    public Decimal Amount { get; set; }
        
    public Ledger? Source { get; set; }
    public Ledger? Destination { get; set; }
}