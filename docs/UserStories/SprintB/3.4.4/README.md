# US 3.4.4 - Alternative Heuristic Algorithm

## Descrição

As a Logistics Operator, I want an alternative scheduling algorithm for the loading and unloading operations of vessels arriving at the port on a given day, that produces a good (but not necessarily optimal) solution efficiently, so that the system can handle larger problem instances or time-constrained planning scenarios.

## Critérios de Aceitação

- This algorithm must be available for selection on the dedicated interface of the SPA and reuse the same data inputs and interfaces defined for the existing scheduling module.
- This algorithm must aim to minimize vessel departure delays but prioritize computational efficiency over optimality. Suitable approaches may include greedy strategies, local search, or other informed heuristics.
- Results must be comparable (e.g., total delay, computation time) against the previous algorithm using summary metrics.
- At this stage, results do not need to be persisted anywhere— they can be recomputed on demand.
