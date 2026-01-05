# US 2.2.4 - Register and Update Storage Areas

## Descrição

As a Port Authority Officer, I want to register and update storage areas, so that (un)loading and storage operations can be assigned to the correct locations.

## Critérios de Aceitação

- Each storage area must have a unique identifier, type (e.g., yard, warehouse), and location within the port.
- Storage areas must specify maximum capacity (in TEUs) and current occupancy.
- By default, a storage area serves the entire port (i.e., all docks). However, some storage areas (namely yards) may be constrained to serve only a few docks, usually the closest ones.
- Complementary information, such as the distance between docks and storage areas, must be manually recorded to support future logistics planning and optimization.
- Updates to storage areas must not allow the current occupancy to exceed maximum capacity.

## Perguntas do Fórum (Dev-Cliente)

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
