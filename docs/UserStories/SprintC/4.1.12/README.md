# US 4.1.12 - Manage Incident Types Catalog

## Descrição

As a Port Authority Officer, I want to manage the catalog of Incident Types so that the classification of operational disruptions remains standardized, hierarchical, and clearly distinct from complementary tasks.

## Critérios de Aceitação

- The system must support hierarchical structuring of incident types (e.g., Fog is a subtype of Environmental Conditions), allowing grouping and filtering by parent type.
- CRUD operations for Incident Types must be available via the REST API.
- The SPA must provide an intuitive interface for listing, filtering, and managing these hierarchy of types.
- Each Incident Type must include a unique code (e.g., T-INC001), a name (e.g., Equipment Failure), a detailed description, a severity classification (e.g., Minor, Major, Critical).
- Examples of possible types:
  - Environmental Conditions: Fog, Strong Winds, Heavy Rain
  - Operational Failures: Crane Malfunction, Power Outage
  - Safety/Security Events: Security Alert
