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
            Category.Create("Bebidas"),
            Category.Create("Panadería"),
            Category.Create("Bocadillos"),
            Category.Create("Lácteos"),
            Category.Create("Frutas y Verduras")
        };
    }

    public IReadOnlyList<Product> GetDefaultProducts()
    {
        var products = new List<Product>();

        products.Add(Product.Create("BE-001", "Café Espresso 250g", 8.50m, 1, 100, 3.20m, "Granos de café espresso molidos"));
        products.Add(Product.Create("BE-002", "Caja de Té Verde 20 unidades", 4.20m, 1, 80, 1.50m, "Bolsitas de té verde premium"));
        products.Add(Product.Create("BE-003", "Jugo de Naranja 1L", 3.00m, 1, 60, 1.10m, "Jugo de naranja recién exprimido"));
        products.Add(Product.Create("BE-004", "Agua Mineral 500ml", 1.20m, 1, 200, 0.40m, "Agua mineral natural"));

        products.Add(Product.Create("PA-001", "Pan de Masa Madre", 5.50m, 2, 30, 2.00m, "Pan artesanal de masa madre"));
        products.Add(Product.Create("PA-002", "Croissants Pack de 4", 6.80m, 2, 25, 2.50m, "Croissants franceses con mantequilla"));
        products.Add(Product.Create("PA-003", "Bagel de Trigo Integral", 2.20m, 2, 50, 0.80m, "Bagel clásico de trigo integral"));
        products.Add(Product.Create("PA-004", "Muffin de Chocolate", 1.80m, 2, 40, 0.60m, "Muffin doble de chocolate"));

        products.Add(Product.Create("BO-001", "Papas Fritas 150g", 2.80m, 3, 90, 1.00m, "Papas fritas clásicas con sal"));
        products.Add(Product.Create("BO-002", "Mix de Frutos Secos 200g", 7.50m, 3, 50, 3.00m, "Mix premium de frutos secos"));
        products.Add(Product.Create("BO-003", "Barra de Chocolate Oscuro 80g", 3.50m, 3, 70, 1.20m, "Chocolate oscuro 70% cacao"));
        products.Add(Product.Create("BO-004", "Pretzels 250g", 3.20m, 3, 60, 1.10m, "Pretzels pequeños con sal"));

        products.Add(Product.Create("LA-001", "Leche Entera 1L", 1.50m, 4, 80, 0.60m, "Leche entera fresca"));
        products.Add(Product.Create("LA-002", "Yogurt Griego 500g", 4.80m, 4, 40, 1.80m, "Yogurt griego natural"));
        products.Add(Product.Create("LA-003", "Queso Cheddar 200g", 5.20m, 4, 35, 2.00m, "Queso cheddar añejado"));
        products.Add(Product.Create("LA-004", "Mantequilla 250g", 3.80m, 4, 45, 1.50m, "Mantequilla con sal"));

        products.Add(Product.Create("FV-001", "Bananos 1kg", 1.20m, 5, 100, 0.50m, "Bananos maduros frescos"));
        products.Add(Product.Create("FV-002", "Manzanas 1kg", 2.50m, 5, 80, 1.00m, "Manzanas rojas delicious"));
        products.Add(Product.Create("FV-003", "Tomates 500g", 2.20m, 5, 60, 0.80m, "Tomates de vid maduros"));
        products.Add(Product.Create("FV-004", "Aguacates 3 unidades", 3.80m, 5, 50, 1.50m, "Aguacates Hass"));

        return products;
    }

    public IReadOnlyList<Customer> GetDefaultCustomers()
    {
        return new List<Customer>
        {
            Customer.Create("00101234567", "Distribuidora La Esperanza S.A.", CustomerType.Empresa, "555100", "contacto@laesperanza.com", "Av. Principal 100, San Jose", 50000m),
            Customer.Create("00112345678", "Maria Fernandez", CustomerType.Persona, "555200", "maria.f@gmail.com", "Calle 5, Casa 20, Heredia", 5000m),
            Customer.Create("13112345672", "Tech Solutions CR Ltda.", CustomerType.Empresa, "555300", "compras@techsolutions.cr", "Edificio Torre, Piso 5, San Jose", 100000m),
            Customer.Create("00176543210", "Roberto Vargas", CustomerType.Persona, "555400", null, null, 2000m),
            Customer.Create("13176543218", "Cafeteria Aroma S.A.", CustomerType.Empresa, "555500", "info@aroma.cr", "Plaza Central Local 12", 30000m)
        };
    }

    public IReadOnlyList<Domain.Entities.Invoice> GetDefaultInvoices()
    {
        var date = DateTime.UtcNow;
        var invoices = new List<Domain.Entities.Invoice>();

        var inv1 = Domain.Entities.Invoice.Create(1, date.AddDays(-10), 1, 1, InvoiceType.Contado, null, "First demo invoice");
        inv1.AddDetail(1, 2, 8.50m, "Café Espresso 250g", "BE-001");
        inv1.AddDetail(6, 1, 6.80m, "Croissants Pack de 4", "PA-002");
        inv1.SetTaxAmount(Math.Round(inv1.Subtotal * 0.13m, 2, MidpointRounding.AwayFromZero), 13m);
        invoices.Add(inv1);

        var inv2 = Domain.Entities.Invoice.Create(2, date.AddDays(-5), 2, 1, InvoiceType.Credito, date.AddDays(25), "Credit sale");
        inv2.AddDetail(11, 1, 7.50m, "Mix de Frutos Secos 200g", "BO-002");
        inv2.AddDetail(16, 2, 4.80m, "Yogurt Griego 500g", "LA-002");
        inv2.SetTaxAmount(Math.Round(inv2.Subtotal * 0.13m, 2, MidpointRounding.AwayFromZero), 13m);
        invoices.Add(inv2);

        var inv3 = Domain.Entities.Invoice.Create(3, date.AddDays(-2), 3, 1, InvoiceType.Contado, null, "Office supply order");
        inv3.AddDetail(9, 3, 2.80m, "Papas Fritas 150g", "BO-001");
        inv3.AddDetail(19, 5, 1.20m, "Bananos 1kg", "FV-001");
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
