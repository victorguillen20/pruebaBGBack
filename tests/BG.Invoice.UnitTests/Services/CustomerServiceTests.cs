using System.Linq.Expressions;
using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Common;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Services;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Enums;
using BG.Invoice.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.UnitTests.Services;

public class CustomerServiceTests
{
    private readonly Mock<IRepository<Customer>> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ILogger<CustomerService> _logger = Mock.Of<ILogger<CustomerService>>();
    private readonly CustomerService _sut;

    public CustomerServiceTests()
    {
        _sut = new CustomerService(_repository.Object, _unitOfWork.Object, _logger);
    }

    private static Customer CreateCustomer(int id, string identification, string name)
    {
        var customer = Customer.Create(identification, name, CustomerType.Persona);
        customer.Id = id;
        return customer;
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCustomer_ReturnsCustomerResponse()
    {
        var customer = CreateCustomer(1, "00101234567", "Test Customer");

        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var result = await _sut.GetByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Test Customer");
        result.Value.Identification.Should().Be("00101234567");
    }

    [Fact]
    public async Task CreateAsync_UniqueIdentification_CreatesCustomer()
    {
        var request = new CreateCustomerRequest("00101234567", "New Customer", CustomerType.Persona, null, null, null, null);

        _repository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Customer, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer>().AsReadOnly());

        var result = await _sut.CreateAsync(request);

        result.IsSuccess.Should().BeTrue();
        _repository.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateIdentification_ThrowsBusinessRuleException()
    {
        var existing = CreateCustomer(1, "00101234567", "Existing");
        var request = new CreateCustomerRequest("00101234567", "New Customer", CustomerType.Persona, null, null, null, null);

        _repository.Setup(r => r.ListAsync(It.IsAny<Expression<Func<Customer, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer> { existing }.AsReadOnly());

        var act = async () => await _sut.CreateAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage(Errors.Customer.IdentificationExists);
    }

    [Fact]
    public async Task UpdateAsync_ExistingCustomer_UpdatesFields()
    {
        var customer = CreateCustomer(1, "00101234567", "Old Name");
        var request = new UpdateCustomerRequest("New Name", CustomerType.Persona, "555100", "new@test.com", "New Address", 1000m);

        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var result = await _sut.UpdateAsync(1, request);

        result.IsSuccess.Should().BeTrue();
        customer.Name.Should().Be("New Name");
        customer.Phone.Should().Be("555100");
        _repository.Verify(r => r.Update(customer), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
