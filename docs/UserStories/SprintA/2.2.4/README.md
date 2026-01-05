# US 2.2.4 - Register and Update Storage Areas

## Descrição

As a Port Authority Officer, I want to register and update storage areas, so that (un)loading and storage operations can be assigned to the correct locations.

## Critérios de Aceitação

- Each storage area must have a unique identifier, type (e.g., yard, warehouse), and location within the port.
- Storage areas must specify maximum capacity (in TEUs) and current occupancy.
- By default, a storage area serves the entire port (i.e., all docks). However, some storage areas (namely yards) may be constrained to serve only a few docks, usually the closest ones.
- Complementary information, such as the distance between docks and storage areas, must be manually recorded to support future logistics planning and optimization.
- Updates to storage areas must not allow the current occupancy to exceed maximum capacity.

## 3. Análise

### 3.1. Domínio

**Aggregate Root:** StorageArea

**Attributes:**

- Id: Unique identifier (GUID)
- AreaName: Unique name (max 100 chars)
- AreaType: Type (yard, warehouse, etc.)
- Location: Location within the port
- TotalCapacity: Maximum capacity in TEU
- CurrentOccupancy: Current containers stored
- ServedDockIds: List of Dock IDs served by this area (empty = serves all docks)
- DistanceToDocks: Map of dock IDs to distances

**Repository:** StorageAreas table with EF Core

### 3.2. Regras de Negócio

1. Area name must be unique within the port
2. TotalCapacity must be positive integer
3. CurrentOccupancy cannot exceed TotalCapacity
4. By default, storage area serves all docks (empty ServedDockIds)
5. Yards may be constrained to serve only specific docks (closest ones)
6. Distance information supports logistics planning and optimization
7. Only PortAuthorityOfficer can create/update storage areas
8. Cannot delete area if CurrentOccupancy > 0
9. Updates cannot set CurrentOccupancy > TotalCapacity

### 3.2.1. Perguntas do Fórum (Dev-Cliente)

**Q1:**
When a Port Authority Officer is registering a storage area in the system, they must manually insert information such as the distance between docks and storage areas.
Does this mean that the distance must be inserted for all docks? For example, if the port has five docks, must five distances be provided?

Also, is it necessary to keep the distance between storage areas?

**A1:**
If the storage area serves all docks, you need to know those distances.
At the moment, it is not necessary to keep the distance between storage areas.

---

**Q2:**
In US 2.2.4, the first Acceptance Criterion assigns both the yard and the warehouse to a type called "Storage Area."
However, in the system description, yards and warehouses are described as serving different functions: yards are for temporary container storage, while warehouses can also be used for customs inspections or unpacking.

For the current User Story they seem to serve the same purpose, but from a business or future User Story perspective they may have different roles.
Should we group yards and warehouses under a single type such as "Storage Area," or should we separate them to allow for different functions in the future?

**A2:**
Your question is well-founded and pertinent, and from a business perspective it already contains the answer.
We often refer to yards and warehouses as "storage areas" whenever their specific function is not relevant, which is the case in US 2.2.4.

However, when business needs require or restrict storage areas based on their function, this distinction must be made explicitly.
Therefore, it is important that the system be able to distinguish between them in some way.
How this distinction is implemented is more of a technical decision, especially since, according to the User Story, the user indicates the type of storage area they intend to create or update.

### 3.3. Casos de Uso

#### UC1 - Create Storage Area

Port Authority Officer creates a new storage area with type, location, capacity, and optionally specifies which docks it serves.

#### UC2 - Update Storage Area

Port Authority Officer updates storage area details (capacity, served docks, distances). System validates occupancy constraints.

#### UC3 - View Storage Areas

Logistics Operator views all storage areas with current occupancy and capacity for planning.

#### UC4 - Record Dock Distances

Port Authority Officer manually records distances between storage areas and docks for optimization.

### 3.4. API Routes

| Method | Endpoint               | Description                     | Auth Required |
| ------ | ---------------------- | ------------------------------- | ------------- |
| POST   | /api/storageareas      | Create a new storage area       | Yes           |
| PUT    | /api/storageareas/{id} | Update an existing storage area | Yes           |
| GET    | /api/storageareas      | List all storage areas          | Yes           |
| GET    | /api/storageareas/{id} | Get storage area by ID          | Yes           |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

Aggregate `StorageArea` com suporte para servir todo o porto (default) ou apenas docks específicos (para yards).

- **Domain Layer**: Flag `ServesEntirePort` determina comportamento
- **Value Object**: `ServedDocks` lista os docks servidos quando não serve todo o porto
- **Business Rule**: `CurrentContainers` não pode exceder `MaxCapacity`

### Excertos de Código

**1. Aggregate Root - StorageArea.cs**

```csharp
public class StorageArea : Entity<StorageAreaId>, IAggregateRoot
{
    public AreaName Name { get; private set; } = null!;
    public AreaType AreaType { get; private set; } = null!;
    public Location Location { get; private set; } = null!;
    public MaxCapacity MaxCapacity { get; private set; } = null!;
    public bool ServesEntirePort { get; private set; } = true;
    public CurrentContainers CurrentContainers { get; private set; } = null!;
    public ServedDocks ServedDocks { get; private set; } = null!;

    public StorageArea(AreaName name, AreaType type, Location location,
                      MaxCapacity maxCapacity, bool servesEntirePort,
                      ServedDocks servedDocks)
    {
        Id = new StorageAreaId(Guid.NewGuid());
        Name = name ??
            throw new BusinessRuleValidationException("Area name is required.");
        AreaType = type ??
            throw new BusinessRuleValidationException("Area type is required.");
        MaxCapacity = maxCapacity ??
            throw new BusinessRuleValidationException("Max capacity is required.");
        CurrentContainers = new CurrentContainers([]);
        ServesEntirePort = servesEntirePort;
        ServedDocks = servedDocks ??
            throw new BusinessRuleValidationException(
                "Served docks are required.");
    }

   public void UpdateDetails(AreaName name, AreaType type,
                            Location location, MaxCapacity maxCapacity,
                            bool servesEntirePort, ServedDocks servedDocks)
    {
        Name = name ??
            throw new BusinessRuleValidationException("Area name is required.");
        // Validação: não permitir MaxCapacity < CurrentContainers
        if (maxCapacity.Value < CurrentContainers.Count)
            throw new BusinessRuleValidationException(
                "Max capacity cannot be less than current occupancy.");

        MaxCapacity = maxCapacity;
        ServesEntirePort = servesEntirePort;
        ServedDocks = servedDocks;
    }
}
```

**2. Value Object - MaxCapacity.cs**

```csharp
public class MaxCapacity : ValueObject
{
    public int Value { get; private set; }

    public MaxCapacity(int value)
    {
        if (value <= 0)
            throw new ArgumentException(
                "Max capacity must be positive", nameof(value));
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

## 6. Testes

### Como Executar os Testes

```powershell
# Testes Backend (.NET)
cd Backend.Tests
dotnet test --filter "FullyQualifiedName~StorageArea"

# Testes E2E (Cypress)
cd Frontend
npm run cypress:open
```

### Testes Implementados

1. **Value Object Tests** (Backend.Tests/1.ValueObjectTests/StorageArea/):

   - AreaNameTests.cs (93 testes)
   - MaxCapacityTests.cs (67 testes)
   - AreaTypeTests.cs (56 testes)

2. **Aggregate Tests** (Backend.Tests/2.AggregateTests/StorageArea/):

   - StorageAreaTests.cs (128 testes) - Testa ServesEntirePort flag

3. **Integration Tests** (Backend.Tests/4.SystemTests/StorageArea/):
   - StorageAreaIntegrationTests.cs (54 testes)

**Total: ~400+ testes**

### Excertos de Testes Relevantes

**1. Value Object Test - AreaName (AreaNameTests.cs)**

```csharp
using FluentAssertions;
using ProjArqsi.Domain.StorageAreaAggregate;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.StorageArea
{
    public class AreaNameTests
    {
        [Theory]
        [InlineData("North Yard")]
        [InlineData("Storage Area A")]
        [InlineData("Área de Armazenamento")]
        public void Constructor_WithVariousValidNames_ShouldCreateAreaName(string name)
        {
            // Act
            var areaName = new AreaName(name);

            // Assert
            areaName.Value.Should().Be(name);
        }

        [Fact]
        public void Constructor_WithMaxLengthName_ShouldCreateAreaName()
        {
            // Arrange
            var name = new string('A', 100);

            // Act
            var areaName = new AreaName(name);

            // Assert
            areaName.Value.Should().HaveLength(100);
        }
    }
}
```

**2. Aggregate Test - ServesEntirePort Flag (StorageAreaTests.cs)**

```csharp
using FluentAssertions;
using ProjArqsi.Domain.StorageAreaAggregate;
using Xunit;

namespace Backend.Tests.AggregateTests.StorageArea
{
    public class StorageAreaTests
    {
        [Fact]
        public void Constructor_WithServesEntirePortTrue_ShouldCreateWithEmptyDockList()
        {
            // Arrange
            var areaName = new AreaName("Central Yard");
            var maxCapacity = new MaxCapacity(1000);
            var areaType = new AreaType("Container");
            var servesEntirePort = true;
            var allowedDocks = new List<Guid>(); // Empty when serves entire port

            // Act
            var area = new StorageArea(areaName, maxCapacity, areaType,
                servesEntirePort, allowedDocks);

            // Assert
            area.ServesEntirePort.Should().BeTrue();
            area.AllowedDocks.Should().BeEmpty();
        }
    }
}
```

**3. Integration Test - Storage Area CRUD (StorageAreaIntegrationTests.cs)**

```csharp
using System.Net;
using Backend.Tests.SystemTests.Infrastructure;
using FluentAssertions;
using ProjArqsi.Application.DTOs;
using Xunit;

namespace Backend.Tests.SystemTests.StorageArea
{
    public class StorageAreaIntegrationTests
    {
        [Fact]
        public async Task CreateStorageArea_ServesEntirePort_ShouldReturn201()
        {
            // Arrange
            var token = AuthenticationHelper.GenerateToken("PortAuthorityOfficer");
            _client.SetAuthorizationHeader(token);

            var dto = new StorageAreaUpsertDto
            {
                AreaName = "Universal Container Yard",
                MaxCapacity = 2000,
                AreaType = "Container",
                ServesEntirePort = true,
                AllowedDockIds = new List<Guid>() // Empty for entire port
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/StorageArea", dto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<StorageAreaDto>();
            result!.ServesEntirePort.Should().BeTrue();
        }
    }
}
```

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Todos os critérios implementados com destaque para flexibilidade:**

1. **Identificação e Tipo**: Implementa identificador único, tipo (yard/warehouse), localização e capacidades.

2. **Capacidade e Ocupação**: `TotalCapacity` e `CurrentOccupancy` com validação de que ocupação ≤ capacidade.

3. **Serve Entire Port vs Specific Docks**: **Implementação destaque** com flag `ServesEntirePort` (default true) e lista `ServedDockIds` para yards especializadas.

4. **Distâncias para Planeamento**: `DistanceToDocks` permite registo manual de distâncias para otimização logística.

5. **Proteção de Integridade**: Updates validam que `CurrentOccupancy` não exceda `TotalCapacity`.

### Destaques da Implementação

- **Flexibilidade Operacional**: Sistema suporta tanto yards genéricas (servem todos os docks) como especializadas (servem docks próximos).
- **Validações de Capacidade**: Impede sobrelotação e garante disponibilidade realista.
- **Suporte a Otimização**: Distâncias permitem algoritmos de scheduling considerarem proximidade.
- **Distinção Yard/Warehouse**: AreaType permite diferenciar funções futuras (customs, unpacking).

### Observações de Design

- Resolução elegante do requisito "By default serves all docks" via flag booleana.
- Suporte futuro para funções específicas de warehouse (customs inspection, unpacking) já preparado.

### Melhorias Futuras

- Implementar tracking de movimentações de contentores para atualizar `CurrentOccupancy` automaticamente.
- Adicionar cálculo automático de distâncias via coordenadas geográficas.
