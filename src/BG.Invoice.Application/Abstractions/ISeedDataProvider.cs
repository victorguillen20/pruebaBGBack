using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Enums;

namespace BG.Invoice.Application.Abstractions;

public interface ISeedDataProvider
{
    IReadOnlyList<Role> GetDefaultRoles();
    IReadOnlyList<Menu> GetDefaultMenus();
    IReadOnlyList<RoleMenu> GetDefaultRoleMenus();
    IReadOnlyList<User> GetDefaultUsers(string passwordHash);
    IReadOnlyList<Category> GetDefaultCategories();
    IReadOnlyList<Product> GetDefaultProducts();
    IReadOnlyList<Customer> GetDefaultCustomers();
    IReadOnlyList<Domain.Entities.Invoice> GetDefaultInvoices();
    CompanyConfig GetDefaultCompanyConfig();
}
