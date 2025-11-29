using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Services.Email.Templates.Reservations;

public class ReservationConfirmedEmail : IEmailTemplate
{
    private readonly TableReservation _reservation;

    public ReservationConfirmedEmail(TableReservation reservation)
    {
        _reservation = reservation;
    }

    public string Subject => "Reservation Confirmation";

    public string BuildBody()
    {
        return $@"
<p>Hello {_reservation.CustomerName},</p>

<p>Thank you for choosing our restaurant!</p>

<p>We are pleased to confirm your table reservation:</p>

<p>
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━<br>
Date: {_reservation.ReservationDate:dddd, MMMM dd, yyyy}<br>
Time: {_reservation.StartTime}<br>
Number of guests: {_reservation.NumberOfGuests}<br>
Table number: {_reservation.Table.TableNumber}<br>
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
</p>

{(string.IsNullOrEmpty(_reservation.Notes) ? "" : $"<p>Special requests: {_reservation.Notes}</p>")}

<p>Please arrive on time to ensure your table is held for you.</p>

<p>Best regards,<br>
{_reservation.Restaurant.Name} Team</p>
";
    }
}