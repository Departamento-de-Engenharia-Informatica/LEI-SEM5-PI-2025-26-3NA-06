# US 3.4.2 - Generate Daily Schedule

## Descrição

As a Logistics Operator, I want to generate a daily schedule for the loading and unloading operations of vessels arriving at the port on a given day, so that delays relative to desired departure times are minimized.

## Critérios de Aceitação

- The objective of the scheduling algorithm is to minimize total delay between the actual completion and desired departure times of vessels.
- Currently, the scheduling algorithm must only consider:
  - One vessel per dock at a time.
  - One crane (system) per unloading/loading operation.
  - One storage location for the unloading/loading operations.
  - Availability of physical resources (crane) and qualified staff within their operational windows.
- The scheduling computation must be executed through the Planning & Scheduling back-end module, which consumes the required data (e.g., vessel arrivals/departures, resources, staff data) from other APIs.
- The scheduling process must be initiated through a dedicated interface on the SPA. This UI must:
  - Allow the operator to specify the target date (day).
  - Display the results in a summary table (e.g., vessel, start/end time, assigned crane, staff) and, if feasible, through a timeline approach.
  - Provide feedback on progress and completion, including warnings about infeasibility (e.g., lack of resources or staff).
- At this stage, results do not need to be persisted anywhere— they can be recomputed on demand.
