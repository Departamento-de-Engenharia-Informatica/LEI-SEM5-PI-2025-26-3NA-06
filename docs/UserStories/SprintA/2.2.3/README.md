# US 2.2.3 - Register and Update Docks

## Descrição

As a Port Authority Officer, I want to register and update docks, so that the system accurately reflects the docking capacity of the port.

## Critérios de Aceitação

- A dock record must include a unique identifier, name/number, location within the port, and physical characteristics (e.g., length, depth, max draft).
- The officer must specify the vessel types allowed to berth there.
- Docks must be searchable and filterable by name, vessel type, and location.

## Perguntas do Fórum (Dev-Cliente)

**Q1:**
Is there any relation between MaxDraft and Depth of dock that the system should validate? If so, could you please explain it?

**A1:**
Yes, there is a relation between both. However, you may ignore it for now. Currently, the system checks a vessel's ability to berth at a given dock based on the relationship between the target dock and the vessel type.

---

**Q2:**
Which fields of a dock are allowed to be updated once it is registered? Should the system maintain a log of dock updates, recording who made the changes and when?

**A2:**
All fields except the identifier can be updated. Yes, the system must maintain a log of dock updates, recording who made the changes and when, in line with the requirement that all user interactions be carefully logged for auditing, traceability, diagnostics, and analysis.

---

**Q3:**
Regarding the user story for registering and updating a dock, we are not sure what is meant by "location within the port."
Should this be stored as geographic coordinates, or as a relative/semantic position (e.g., area, zone) within the port?

**A3:**
In this case, you may consider the "location within the port" as free text.

**Follow-up:**
Following up on the previous response, should the system prevent creating or editing a dock at the same location as an existing one?
In other words, can two docks share the same location?

**Follow-up Answer:**
In practice, they cannot.
But, as stated before, treat this information as free text.

---

**Q4:**
Regarding this user story, can you confirm if a dock supports only one vessel type?

**A4:**
No. An acceptance criterion states that "The officer must specify the vessel types allowed to berth there." A given dock may support several vessel types (e.g. Feeder and Panamax).

---

**Q5:**
Are docks considered Storage Areas also? Or should we assume that Storage Areas are only the yards and warehouses?

**A5:**
Docks are not storage areas.

Right.
