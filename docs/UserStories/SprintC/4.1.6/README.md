# US 4.1.6 - Query Resource Allocation Time

## Descrição

As a Logistics Operator, I want to query, for a given period, the total allocation time of a specific resource (e.g., crane, dock, or staff), so that I can assess resource utilization and workload distribution.

## Critérios de Aceitação

- The REST API must provide endpoints aggregating Operation Plan data by resource and period.
- Returned data must include total allocated time and number of operations.
- The SPA must display results in a summary table.
- Data must only include saved Operation Plans.
