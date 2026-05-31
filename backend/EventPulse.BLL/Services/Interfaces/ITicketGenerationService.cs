namespace EventPulse.BLL.Interfaces;

public interface ITicketGenerationService
{
    Task GenerateTicketDocumentsAsync(DAL.Entities.Booking booking);
}
