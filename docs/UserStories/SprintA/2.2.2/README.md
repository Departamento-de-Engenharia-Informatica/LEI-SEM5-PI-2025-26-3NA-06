# US 2.2.2 - Register and Update Vessel Records

## Descrição

As a Port Authority Officer, I want to register and update vessel records, so that valid vessels can be referenced in visit notifications.

## Critérios de Aceitação

- Each vessel record must include key attributes such as IMO number, vessel name, vessel type and operator/owner.
- The system must validate that the IMO number follows the official format (seven digits with a check digit), otherwise reject it.
- Vessel records must be searchable by IMO number, name, or operator.

## 3. Análise

### 3.1. Domínio

_A desenvolver: Identificar as entidades, agregados e value objects do domínio relacionados com esta US._

### 3.2. Regras de Negócio

_A desenvolver: Documentar as regras de negócio específicas desta funcionalidade._

### 3.3. Casos de Uso

_A desenvolver: Descrever os principais casos de uso e seus fluxos._

## Perguntas do Fórum (Dev-Cliente)

**Q1:**
Does the client want to store separate information about the operator and separate information about the owner? Or do they treat this information as the same person? What kind of information do we need to store about it/each of them?

**A1:**
You may treat operator and owner separately. Either, for now, we just need to capture its name, i.e. operator name and owner name.

---

**Q2:**
Can operators change over time? I mean can the same vessel be operated by different operators?

**A2:**
Yes, operator may change over time. When that happens, the vessel record must be updated.

---

**Q3:**
When you refer to "Vessel records must be searchable by IMO number, name, or operator", I understand that the IMO number and the vessel name are unique.
Is the operator/owner also unique, or can the same operator/owner have multiple vessels?

**A3:**
No. The same entity or organization may operate or own several vessels.
Moreover, searching by name (for example, "Saint") may return several vessels, i.e., all vessels whose name contains the word "saint".

---

**Q4:**
We need to implement validation for IMO numbers (International Maritime Organization ship identification numbers) in our vessel management system.

Context:

- IMO numbers have the format: IMO followed by 7 digits (e.g., "IMO 9074729")
- The last digit is a check digit used for validation
- We need to validate IMO numbers when registering/updating vessels

Questions:

- What is the exact algorithm for calculating and validating the IMO number check digit?
- Is it similar to the ISO 6346 container check digit algorithm, or completely different?
- Are there any special cases or exceptions we should handle?

Our understanding:

- We suspect it's a module-based calculation using position weights
- We need the official algorithm to ensure correct validation

Impact:
This is required for US 2.2.2 (Register & Manage Vessel Records) to ensure data integrity when storing vessel information.

**A4:**
For a IMO number of a vessel, you may apply the algorithm briefly described in https://en.wikipedia.org/wiki/IMO_number
