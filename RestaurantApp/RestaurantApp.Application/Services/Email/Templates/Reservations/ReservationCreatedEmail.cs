using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application.Services.Email.Templates.Reservations;

public class ReservationCreatedEmail: IEmailTemplate
{
    private readonly TableReservation _reservation;

    public ReservationCreatedEmail(TableReservation reservation)
    {
        _reservation = reservation;
    }

    public string Subject => "Pending reservation created";

    public string BuildBody()
    {
        return $@"
<p>Hello {_reservation.CustomerName},</p>

<p>Thank you for choosing our restaurant!</p>

<p>We registered your reservation in our system:</p>

<p>
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━<br>
Date: {_reservation.ReservationDate:dddd, MMMM dd, yyyy}<br>
Time: {_reservation.StartTime}<br>
Number of guests: {_reservation.NumberOfGuests}<br>
Table number: {_reservation.Table.TableNumber}<br>
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
</p>

{(string.IsNullOrEmpty(_reservation.Notes) ? "" : $"<p>Special requests: {_reservation.Notes}</p>")}

<p>You will receive an e-mail when restaurant's staff accepts your reservation</p>

<p>Best regards,<br>
{_reservation.Restaurant.Name} Team</p>
";
    }
}