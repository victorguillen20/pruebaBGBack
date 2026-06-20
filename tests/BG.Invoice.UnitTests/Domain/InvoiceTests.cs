using FluentAssertions;
using Xunit;

namespace BG.Invoice.UnitTests.Domain
{
    using BG.Invoice.Domain.Entities;
    using BG.Invoice.Domain.Enums;
    using BG.Invoice.Domain.Exceptions;

    public class InvoiceTests
    {
        private static Invoice CreateTestInvoice()
        {
            return Invoice.Create(number: 1, dateUtc: DateTime.UtcNow, customerId: 10, sellerId: 20, type: InvoiceType.Contado);
        }

        [Fact]
        public void Create_WithValidData_StartsAsPendiente()
        {
            var invoice = CreateTestInvoice();
            invoice.Status.Should().Be(InvoiceStatus.Pendiente);
            invoice.Subtotal.Should().Be(0m);
            invoice.Total.Should().Be(0m);
        }

        [Fact]
        public void Create_Credito_WithoutDueDate_Throws()
        {
            var act = () => Invoice.Create(number: 1, dateUtc: DateTime.UtcNow, customerId: 10, sellerId: 20, type: InvoiceType.Credito);
            act.Should().Throw<BusinessRuleException>().WithMessage("*Credit invoices require a DueDate*");
        }

        [Fact]
        public void AddDetail_RecalculatesSubtotal()
        {
            var invoice = CreateTestInvoice();
            invoice.AddDetail(productId: 1, quantity: 2, unitPrice: 10m, productNameSnapshot: "Brocas", productCodeSnapshot: "P001");
            invoice.AddDetail(productId: 2, quantity: 3, unitPrice: 5m, productNameSnapshot: "Tornillos", productCodeSnapshot: "P002");

            invoice.Subtotal.Should().Be(35m);  // 2*10 + 3*5
            invoice.Total.Should().Be(35m);     // no tax set yet
        }

        [Fact]
        public void AddPayment_TotalCovered_SetsStatusToPagada()
        {
            var invoice = CreateTestInvoice();
            invoice.AddDetail(1, 1, 100m, "X", "P1");
            invoice.SetTaxAmount(13m, 13m);
            // Total = 113
            invoice.AddPayment(PaymentMethod.Efectivo, 113m, null, DateTime.UtcNow);
            invoice.Status.Should().Be(InvoiceStatus.Pagada);
        }

        [Fact]
        public void AddPayment_Partial_StaysPendiente()
        {
            var invoice = CreateTestInvoice();
            invoice.AddDetail(1, 1, 100m, "X", "P1");
            invoice.AddPayment(PaymentMethod.Efectivo, 50m, null, DateTime.UtcNow);
            invoice.Status.Should().Be(InvoiceStatus.Pendiente);
        }

        [Fact]
        public void Cancel_SetsStatusToAnulada()
        {
            var invoice = CreateTestInvoice();
            invoice.Cancel();
            invoice.Status.Should().Be(InvoiceStatus.Anulada);
        }
    }
}
