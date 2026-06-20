using BG.Invoice.Application.Dtos;
using BG.Invoice.Domain.Entities;

namespace BG.Invoice.Application.Mappings;

public static class CustomerMappings
{
    public static CustomerResponse ToResponse(this Customer customer, DateTime createdAt)
    {
        return new CustomerResponse(
            customer.Id, customer.Identification, customer.Name,
            customer.Phone, customer.Email, customer.Address,
            customer.Type, customer.IsActive, customer.CreditLimit, createdAt);
    }
}
