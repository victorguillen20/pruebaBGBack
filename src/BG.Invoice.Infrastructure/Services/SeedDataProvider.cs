using BG.Invoice.Application.Abstractions;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Enums;

namespace BG.Invoice.Infrastructure.Services;

public class SeedDataProvider : ISeedDataProvider
{
    public IReadOnlyList<Role> GetDefaultRoles()
    {
        return new List<Role>
        {
            Role.Create("Admin", "Full system access"),
            Role.Create("Vendedor", "Sales access, can only see own invoices")
        };
    }

    public IReadOnlyList<Menu> GetDefaultMenus()
    {
        return new List<Menu>
        {
            Menu.Create("dashboard", "Dashboard", "dashboard", "/dashboard", 1),
            Menu.Create("invoices", "Invoices", "receipt", "/invoices", 2),
            Menu.Create("customers", "Customers", "people", "/customers", 3),
            Menu.Create("products", "Products", "inventory", "/products", 4),
            Menu.Create("users", "Users", "group", "/users", 5),
            Menu.Create("config", "Configuration", "settings", "/config", 6)
        };
    }

    public IReadOnlyList<RoleMenu> GetDefaultRoleMenus()
    {
        return new List<RoleMenu>
        {
            new RoleMenu(1, 1),
            new RoleMenu(2, 1),
            new RoleMenu(3, 1),
            new RoleMenu(4, 1),
            new RoleMenu(5, 1),
            new RoleMenu(6, 1),
            new RoleMenu(1, 2),
            new RoleMenu(2, 2),
            new RoleMenu(3, 2),
            new RoleMenu(4, 2)
        };
    }

    public IReadOnlyList<User> GetDefaultUsers(string passwordHash)
    {
        return new List<User>
        {
            User.Create("admin", "admin@bg.com", passwordHash, "System", "Admin", 1),
            User.Create("vendor1", "vendor1@bg.com", passwordHash, "Maria", "Gonzalez", 2),
            User.Create("vendor2", "vendor2@bg.com", passwordHash, "Carlos", "Ramirez", 2)
        };
    }

    public IReadOnlyList<Category> GetDefaultCategories()
    {
        return new List<Category>
        {
            Category.Create("Beverages"),
            Category.Create("Bakery"),
            Category.Create("Snacks"),
            Category.Create("Dairy"),
            Category.Create("Produce")
        };
    }

    public IReadOnlyList<Product> GetDefaultProducts()
    {
        var products = new List<Product>();

        products.Add(Product.Create("BV-001", "Espresso Coffee 250g", 8.50m, 1, 100, 3.20m, "Ground espresso beans"));
        products.Add(Product.Create("BV-002", "Green Tea Box 20pcs", 4.20m, 1, 80, 1.50m, "Premium green tea bags"));
        products.Add(Product.Create("BV-003", "Orange Juice 1L", 3.00m, 1, 60, 1.10m, "Fresh squeezed orange juice"));
        products.Add(Product.Create("BV-004", "Sparkling Water 500ml", 1.20m, 1, 200, 0.40m, "Natural mineral water"));

        products.Add(Product.Create("BK-001", "Sourdough Bread Loaf", 5.50m, 2, 30, 2.00m, "Artisan sourdough bread"));
        products.Add(Product.Create("BK-002", "Croissant Pack of 4", 6.80m, 2, 25, 2.50m, "Buttery French croissants"));
        products.Add(Product.Create("BK-003", "Whole Wheat Bagel", 2.20m, 2, 50, 0.80m, "Classic whole wheat bagel"));
        products.Add(Product.Create("BK-004", "Chocolate Muffin", 1.80m, 2, 40, 0.60m, "Double chocolate muffin"));

        products.Add(Product.Create("SN-001", "Potato Chips 150g", 2.80m, 3, 90, 1.00m, "Classic salted potato chips"));
        products.Add(Product.Create("SN-002", "Mixed Nuts 200g", 7.50m, 3, 50, 3.00m, "Premium mixed nuts"));
        products.Add(Product.Create("SN-003", "Dark Chocolate Bar 80g", 3.50m, 3, 70, 1.20m, "70% cocoa dark chocolate"));
        products.Add(Product.Create("SN-004", "Pretzels 250g", 3.20m, 3, 60, 1.10m, "Salted mini pretzels"));

        products.Add(Product.Create("DR-001", "Whole Milk 1L", 1.50m, 4, 80, 0.60m, "Fresh whole milk"));
        products.Add(Product.Create("DR-002", "Greek Yogurt 500g", 4.80m, 4, 40, 1.80m, "Plain Greek yogurt"));
        products.Add(Product.Create("DR-003", "Cheddar Cheese 200g", 5.20m, 4, 35, 2.00m, "Aged cheddar cheese"));
        products.Add(Product.Create("DR-004", "Butter 250g", 3.80m, 4, 45, 1.50m, "Salted butter"));

        products.Add(Product.Create("PR-001", "Bananas 1kg", 1.20m, 5, 100, 0.50m, "Fresh ripe bananas"));
        products.Add(Product.Create("PR-002", "Apples 1kg", 2.50m, 5, 80, 1.00m, "Red delicious apples"));
        products.Add(Product.Create("PR-003", "Tomatoes 500g", 2.20m, 5, 60, 0.80m, "Vine ripe tomatoes"));
        products.Add(Product.Create("PR-004", "Avocados 3pcs", 3.80m, 5, 50, 1.50m, "Hass avocados"));

        return products;
    }

    public IReadOnlyList<Customer> GetDefaultCustomers()
    {
        return new List<Customer>
        {
            Customer.Create("001-0123456-7", "Distribuidora La Esperanza S.A.", CustomerType.Empresa, "555-1001", "contacto@laesperanza.com", "Av. Principal 100, San Jose", 50000m),
            Customer.Create("002-0234567-8", "Maria Fernandez", CustomerType.Persona, "555-1002", "maria.f@gmail.com", "Calle 5, Casa 20, Heredia", 5000m),
            Customer.Create("003-0345678-9", "Tech Solutions CR Ltda.", CustomerType.Empresa, "555-1003", "compras@techsolutions.cr", "Edificio Torre, Piso 5, San Jose", 100000m),
            Customer.Create("004-0456789-0", "Roberto Vargas", CustomerType.Persona, "555-1004", null, null, 2000m),
            Customer.Create("005-0567890-1", "Cafeteria Aroma S.A.", CustomerType.Empresa, "555-1005", "info@aroma.cr", "Plaza Central Local 12", 30000m)
        };
    }

    public IReadOnlyList<Domain.Entities.Invoice> GetDefaultInvoices()
    {
        var date = DateTime.UtcNow;
        var invoices = new List<Domain.Entities.Invoice>();

        var inv1 = Domain.Entities.Invoice.Create(1, date.AddDays(-10), 1, 1, InvoiceType.Contado, null, "First demo invoice");
        inv1.AddDetail(1, 2, 8.50m, "Espresso Coffee 250g", "BV-001");
        inv1.AddDetail(6, 1, 6.80m, "Croissant Pack of 4", "BK-002");
        inv1.SetTaxAmount(Math.Round(inv1.Subtotal * 0.13m, 2, MidpointRounding.AwayFromZero), 13m);
        invoices.Add(inv1);

        var inv2 = Domain.Entities.Invoice.Create(2, date.AddDays(-5), 2, 1, InvoiceType.Credito, date.AddDays(25), "Credit sale");
        inv2.AddDetail(11, 1, 7.50m, "Mixed Nuts 200g", "SN-002");
        inv2.AddDetail(16, 2, 4.80m, "Greek Yogurt 500g", "DR-002");
        inv2.SetTaxAmount(Math.Round(inv2.Subtotal * 0.13m, 2, MidpointRounding.AwayFromZero), 13m);
        invoices.Add(inv2);

        var inv3 = Domain.Entities.Invoice.Create(3, date.AddDays(-2), 3, 1, InvoiceType.Contado, null, "Office supply order");
        inv3.AddDetail(9, 3, 2.80m, "Potato Chips 150g", "SN-001");
        inv3.AddDetail(19, 5, 1.20m, "Bananas 1kg", "PR-001");
        inv3.AddPayment(PaymentMethod.Efectivo, inv3.Total, "Cash payment", date.AddDays(-1));
        invoices.Add(inv3);

        return invoices;
    }

    public CompanyConfig GetDefaultCompanyConfig()
    {
        return CompanyConfig.Create(
            companyName: "BG Invoice Demo",
            taxPercent: 13m,
            currencySymbol: "$",
            phone: "+506 2222-3333",
            email: "info@bg.com",
            taxId: "3-101-123456",
            address: "Avenida Central, Edificio 1",
            city: "San Jose",
            region: "Central",
            postalCode: "10101",
            logoUrl: null);
    }
}
