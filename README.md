# BG Invoice — Backend

API REST para gestión de facturación, construida en .NET 10 con Clean Architecture. Réplica del demo de referencia [obedalvarado.com/demos/simple_invoice](https://obedalvarado.com/demos/simple_invoice) con mejoras propias (menús role-aware, número de factura atómico, etc.).

> **Stack**: .NET 10 · EF Core (SQLite) · ASP.NET Core · JWT · xUnit + FluentAssertions + Moq
>
> **Estado**: ✅ 93/93 tests pasando · 0 advertencias · 0 comentarios · Deployado en Render

---

## 🎯 ¿Por qué Clean Architecture para este proyecto?

Esta es una prueba de desarrollo backend donde se evalúa la capacidad de estructurar código mantenible, testeable y preparado para crecer. **Clean Architecture** fue la elección porque:

1. **El enunciado lo permite** — no es un proyecto de startup donde se prioriza velocidad sobre estructura. Es una evaluación, así que la calidad arquitectónica ES el entregable.
2. **11 entidades con relaciones complejas** — Customer, Product, Invoice, InvoiceDetail, Payment, User, Role, Menu, RoleMenu, Category, CompanyConfig. Sin capas, el código se vuelve spaghetti en 2 semanas.
3. **Las pruebas de integración importan** — Clean Architecture permite mockear todo a nivel de abstracción (IInvoiceRepository, IRoleService, etc.) sin tocar EF Core ni HTTP.
4. **El número de factura atómico requiere control fino** — la entidad `Invoice` y el `InvoiceNumberGenerator` viven en Domain, que NO depende de nadie. Las reglas de negocio críticas no se filtran a la infraestructura.

---

## 🏛️ Arquitectura

El proyecto sigue **Clean Architecture** estricta con 4 capas y regla de dependencia hacia adentro (las capas internas NO conocen a las externas):

src/
├── BG.Invoice.Domain/          ← Entidades + reglas de negocio puras
├── BG.Invoice.Application/     ← Casos de uso (services) + DTOs + interfaces
├── BG.Invoice.Infrastructure/  ← EF Core, SQLite, repos, seed, password hasher
└── BG.Invoice.Api/             ← Controllers, middleware, Program.cs

### Capas en detalle

| Capa | Responsabilidad | Puede depender de |
|---|---|---|
| **Domain** | Entidades, excepciones tipadas, enums, interfaces de `IAuditable` | Nada (cero dependencias) |
| **Application** | Services con lógica de aplicación, DTOs, validadores FluentValidation, abstracciones (`IRepository<T>`, `ISeedDataProvider`) | Domain |
| **Infrastructure** | EF Core DbContext, configuraciones, migraciones, repos concretos, `SeedDataProvider`, hashing, JWT, generador de número de factura | Application + Domain |
| **Api** | Controllers, middleware (auth, exception, request id), Swagger, CORS, health checks | Application + Infrastructure + Domain |

<img width="349" height="606" alt="imagen" src="https://github.com/user-attachments/assets/6ecfb26e-3b2c-4b1b-92ca-ad9b66ab7681" />

---

## 🎨 Patrones de diseño aplicados

### 1. **Repository Pattern** (con especilización por agregado)

`IRepository<T>` genérico para CRUD básico, más repos especializados para queries que necesitan `.Include()` (no se puede con el genérico):

```csharp
// Genérico — CRUD simple
public interface IRepository<T> where T : class, IAuditable
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? filter = null, CancellationToken ct = default);
    // ...
}

// Especializado — queries con .Include()
public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Invoice>> GetRecentAsync(int sellerId, int count, CancellationToken ct = default);
    // ...
}
```
Por qué: el IRepository<T> genérico NO soporta .Include(u => u.Role) (no conoce navegación). Los repos especializados sí. Esto es la causa del bug de JWT con role vacío que tuvimos al inicio.
2. Result Pattern (en services, no en controllers)
Los services devuelven Result<T> con IsSuccess + Value + ErrorMessage + ValidationErrors. Los controllers lanzan excepciones tipadas que el GlobalExceptionMiddleware mapea a HTTP status codes.
Por qué: la lógica de negocio compleja (validación de crédito, cálculo de totales, transición de estados) NO debería depender de HTTP. El service decide si la operación es válida; el controller decide cómo responder al cliente.
3. Typed Exceptions + Centralized Messages
    5 excepciones tipadas en Domain/Exceptions/:
    - NotFoundException → 404
    - BusinessRuleException → 409
    - UnauthorizedException → 401
    - ForbiddenException → 403
    - AccountLockedException → 423
Mensajes centralizados en Application/Common/Errors.cs (7 grupos, 17 constantes, 2 métodos estáticos). Cero strings inline de error en services o validators.
Por qué: cambiar el mensaje de "rol no encontrado" se hace en 1 lugar, no en 12.
4. Domain-Driven Design (DDD táctico)
    - Entidades con comportamiento: Invoice.Cancel(), Product.DecrementStock(), User.RecordFailedLogin() — la lógica está en el dominio, no en services
    - Aggregate roots: Invoice es el aggregate root de InvoiceDetail y Payment (solo se accede vía Invoice)
    - Value objects implícitos: Email, Price (decimal con invariantes), Identification
    - Factory methods: User.Create(...), Product.Create(...) con validación en el constructor
5. DI Registration (explícito, sin magia)
Cada IXxxService se registra explícitamente en AddApplication() / AddInfrastructure(). Cero convenciones por nombre, cero reflection-based registration.
Por qué: cuando un service no se registra, el error es inmediato (UnableToResolveService en startup), no en runtime cuando alguien lo pide.
6. InternalsVisibleTo (test seam sin reflexión)
El proyecto de tests accede a setters internos de las entidades via:
[assembly: InternalsVisibleTo("BG.Invoice.UnitTests")]
[assembly: InternalsVisibleTo("BG.Invoice.IntegrationTests")]
Las entidades tienen { get; internal set; } en propiedades que el dominio controla pero los tests necesitan asignar (ej: Role.Id, Menu.Id). Cero reflexión (que es brittle a cambios del compilador).
7. SeedHostService idempotente
if (existingUsers.Count > 0) { return; }  // skip si ya hay datos
El seed corre solo la primera vez. En Render (disco efímero), corre en cada deploy. En local con DB persistente, corre solo una vez.
8. GlobalExceptionMiddleware + RequestIdMiddleware
Cada request tiene un RequestId único. Si hay una excepción no tipada, el response incluye el RequestId y los logs lo tienen — correlación inmediata para debugging.
✅ Principios SOLID aplicados
Principio	Cómo se aplica
S (Single Responsibility)	Cada service hace UNA cosa (InvoiceService no se mezcla con auth). Cada controller tiene una responsabilidad clara.
O (Open/Closed)	Extensibilidad via interfaces (IInvoiceRepository puede tener otra impl). Services registrados en DI.
L (Liskov Substitution)	Todos los repos respetan el contrato de IRepository<T>. Specialized repos extienden sin romper.
I (Interface Segregation)	Repos especializados separados del genérico. Services con interfaces chiquitas (IRoleService tiene 1 método, no 20).
D (Dependency Inversion)	Application no conoce EF Core. Infrastructure implementa las abstracciones de Application.
Otros principios:
- DRY: ISeedDataProvider es la única fuente de verdad para datos de seed. Tests y producción lo usan. Si cambias un email, los tests reflejan el cambio automáticamente.
- KISS: controllers chiquitos (~30 líneas c/u), services con un solo público method por responsabilidad.
- YAGNI: cero features "por si acaso". Cada controller hace lo que el enunciado pide, nada más.
- Fail Fast: excepciones tipadas, validación en constructor de entidades, validators en Application.
🧪 Estrategia de testing (93/93)
Capas de testing
tests/
├── BG.Invoice.UnitTests/         ← xUnit + Moq (mocks de repos)
│   └── Services/                  ← 34 tests de services + 30 de validators + domain
└── BG.Invoice.IntegrationTests/  ← WebApplicationFactory + SQLite in-memory
    └── Endpoints/                 ← 14 tests de API (auth, menus, products, invoices, health)
Lo que testea cada capa
Capa	Qué verifica	Cómo
Unit (Services)	Lógica de negocio, validaciones, transiciones de estado	Mocks de IRepository<T>, IPasswordHasher, etc.
Unit (Validators)	Reglas de FluentValidation	Solo instanciar el validator y correrlo
Unit (Domain)	Comportamiento de entidades (ej: Invoice.Cancel() ya cancelada)	Instanciar y llamar métodos
Integration	Flujo HTTP completo, JWT, role-based authorization, navegación de EF Core	WebApplicationFactory<Program> con SQLite in-memory (file-based + WAL para R-1)
Test destacado: R-1 (atomicidad de número de factura)
InvoiceTests.Create_10ParallelCalls_ProduceDistinctNumbers lanza 10 requests HTTP en paralelo a POST /api/invoices y verifica que los 10 números generados son distintos (1001, 1002, ..., 1010). El test usa el InvoiceNumberGenerator real (no mockeado) para probar el bloqueo atómico de SQLite con WAL journal mode.
Por qué importa: sin la protección del InvoiceNumberGenerator, dos requests simultáneos podrían leer el mismo "último número" y generar facturas con número duplicado. Eso es un bug que en producción pasa una vez cada 1000 y nadie se entera.
<img width="1558" height="586" alt="imagen" src="https://github.com/user-attachments/assets/6c60610e-fa24-4ac6-9f2e-5cf44556555a" />

🌟 Puntos fuertes del backend
1. Número de factura atómico
InvoiceNumberGenerator usa IDbContextFactory<AppDbContext> con isolation Serializable + BEGIN IMMEDIATE (implícito en SQLite) para garantizar que dos requests simultáneos no generen el mismo número. Probado con 10 requests paralelos.
2. Menús role-aware
GET /api/menus devuelve 6 menús para admin, 4 para vendor. La asociación Role ↔ Menu es M:N via RoleMenu. Cambiar el menú de un rol es 1 update en la tabla pivote.
3. Vendor isolation
Un vendor solo ve SUS propias invoices. El InvoiceService.GetByIdAsync chequea sellerId == currentUser.UserId || isAdmin y lanza ForbiddenException si no coincide. Probado en integration test.
4. Single source of truth para seed
ISeedDataProvider (Application) → SeedDataProvider (Infrastructure). Tanto SeedHostedService (producción) como CustomWebApplicationFactory (tests) usan el mismo provider. Si cambiás un email en el seed, los integration tests reflejan el cambio sin tocar nada.
5. Zero comentarios en código
El código se autodocumenta via nombres descriptivos y tests. No hay un solo // esto hace X en todo el repo. La intención se lee en el nombre del método o en el test que lo cubre.
6. Manejo de errores centralizado
5 excepciones tipadas en Domain. 1 middleware las mapea a HTTP. 1 archivo Errors.cs con todos los mensajes. Cero strings inline de error en services o validators.
7. Build limpio absoluto
TreatWarningsAsErrors=true. 0 advertencias, 0 errores. Cada commit pasa por el build completo (los 4 proyectos compilan + los 2 proyectos de tests).
🚀 Cómo correrlo localmente
Requisitos
- .NET 10 SDK
- Git
Pasos
git clone https://github.com/victorguillen20/pruebaBGBack.git
cd pruebaBGBack
.\run.bat           # Windows cmd
# o .\run.ps1       # PowerShell
# o ./run.sh        # bash / WSL
El script ejecuta dotnet run --project src/BG.Invoice.Api con el flag correcto. Si tipeás dotnet run solo vas a ver el error porque hay múltiples .csproj en el árbol.
<img width="1522" height="178" alt="imagen" src="https://github.com/user-attachments/assets/6a124830-9fd4-4b76-8452-5a00f1052f6f" />

⚠️ Cambiar el password en producción vía JwtSettings__Secret (env var) y forzar MustChangePassword=true para los usuarios seeded.
Health check
curl http://localhost:5000/health
# → Healthy
Swagger
Disponible en desarrollo en http://localhost:5000/swagger.
📡 API documentada en Postman
El repositorio incluye una colección de Postman completa con los 36 endpoints, ejemplos de payloads, y auth flow automatizado (el login request setea el {{token}} automáticamente).
<img width="981" height="670" alt="imagen" src="https://github.com/user-attachments/assets/a4332db9-1f7e-46e0-8682-742f34cea26a" />
<img width="982" height="667" alt="imagen" src="https://github.com/user-attachments/assets/3a854003-4e49-4572-9e31-989dfd303eaf" />

Cómo usar:
1. Abrí Postman → Import → subí postman/collection.json y postman/environment.json
2. Seleccioná el environment "BG Invoice - Dev"
3. Ejecutá "Auth → Login" (con admin / Admin123!) — el token se setea automáticamente
4. Todos los demás requests ya heredan el Bearer {{token}}
URL de producción: cambiá el baseUrl del environment a https://pruebabgback.onrender.com para testear el deploy.
🗄️ Modelo de datos
11 entidades con 9 relaciones (incluyendo M:N vía RoleMenu). El diagrama ER completo está en docs/er-diagram.pdf.
<img width="703" height="790" alt="imagen" src="https://github.com/user-attachments/assets/3ec52bd7-e414-47a9-9336-d6dc56fae26e" />

Highlights del modelo:
- Invoice es aggregate root de InvoiceDetail y Payment (cascade delete)
- RoleMenu es la tabla pivote para el M:N entre Role y Menu
- CompanyConfig es un singleton (Id=1 siempre)
- 4 propiedades de auditoría (CreatedAt, CreatedBy, ModifiedAt, ModifiedBy) heredadas via IAuditable y aplicadas por AuditSaveChangesInterceptor
🌍 Deploy
Producción actual: https://pruebabgback.onrender.com (https://pruebabgback.onrender.com) (Render free tier)
Stack de deploy
- Docker multi-stage (.NET 10 SDK → ASP.NET 10 runtime)
- Render Web Service, plan Free, region Oregon
- SQLite file-based con SeedHostedService que re-corre en cada deploy
- HTTPS manejado por el load balancer de Render (no por la app)
Caveats del free tier
- ⚠️ Disco efímero: la DB se borra en cada redeploy. El seed se restaura, pero los datos creados por usuarios se pierden.
- ⚠️ Cold start: si nadie pega la app por 15+ min, el container se apaga. La primera request después tarda ~30s.
- ⚠️ RAM: 512 MB. Suficiente para el demo.
Deployar otra versión
- Push a main → Render redeploy automático
- O manual desde el dashboard de Render
<img width="1909" height="1021" alt="imagen" src="https://github.com/user-attachments/assets/1ee55d75-10b7-47b3-a94b-3447abb0204b" />
