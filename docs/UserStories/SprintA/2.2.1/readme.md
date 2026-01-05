# US 2.2.1 - Create and Update Vessel Types

## 1. Descrição da User Story

As a Port Authority Officer, I want to create and update vessel types, so that vessels can be classified consistently and their operational constraints are properly defined.

## 2. Critérios de Aceitação

- Vessel types must include attributes such as name, description, capacity, and operational constraints (e.g.: maximum number of rows, bays, and tiers).
- Vessel types must be available for reference when registering vessel records.
- Vessel types must be searchable and filterable by name and description.

## 3. Análise

### 3.1. Domínio

### 3.2. Regras de Negócio

### 3.3. Casos de Uso

#### UC1 - [Nome do Caso de Uso]

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

### 4.2. Diagrama de Sequência Detalhado

### 4.3. Modelo de Domínio

### 4.4. Padrões Aplicados

## 5. Implementação

## 6. Testes

## 7. Observações

## 8. Perguntas do Fórum (Dev-Cliente)

**Q1:**
Good afternoon,

When you state "Vessel types must include attributes such as name, description, capacity, and operational constraints (e.g.: maximum number of rows, bays, and tiers).", is the description something general and always the same for each vessel type? Or is it unique to each record?

Thank you for your time and consideration,

**A1:**
I'm not sure I understood the question.
The "description" is the description of the vessel type being registered.
A different vessel type has, naturally, a different description. But, this doesn't mean that descriptions of vessel types are unique.
What is the doubt, here?

---

**Q2:**
I meant unique to each type.
I.e. the description for a panamax vessel is always the same to all panamax vessels or can the vessels, being the same type, have different descriptions?

**A2:**
You are mixing distinct things...
Each Vessel Type has a name, description, capacity, etc. (check US 2.2.1).
Each Vessel has an IMO number, vessel name, operator/owner and is classified by a (vessel) type (check US 2.2.2).
Several vessels may be classified by the same type.

---

**Q3:**
Good afternoon,

The 4 examples of vessel types (feeder, panamax, ...) that the system specification gives us are the only ones that our app is working with? Or should we assume that, further ahead, more types can be introduced/created by the user through the app?

**A3:**
The question is a bit pointless.
Obviously, those vessel types are just examples.
Notice that there is a user story (US 2.2.1) that allows users to create any type of vessel they want, so they are not limited to these four examples.

---

**Q4:**
In many user stories there is the need to search and/or filter data. For example, in US 221 one of the acceptance criteria is: "Vessel types must be searchable and filterable by name and description." Does this mean that we need to be able to input a, for example, vessel type name "Tanker" and then get "Gas Tanker", "Oil Tanker" if those are vessel types that exist in the system? Or should we need to input BOTH the name AND the description to get some results? And if we need to input only a description then should it only return direct matches (only return the vessel types that perfectly, word for word match the given filtration description) or would the user input a word or two and we would need to show all vessel types containing that word in their description? We don't quite understand how the user wants to use these functions so it would be nice to see an example.

**A4:**
Your example is perfect. While searching for text fields, the better approach is returning partial match. E.g. searching by "tanker" returns all records whose description contains such word on a case insensitive case. However, as a user it would be nice if I could refine the search by setting the kind of operator to be applied (e.g. equals, contains).

---

**Q5:**
Can there be more than one vessel type with the same name?

**A5:**
No. Vessel type names are unique.

---

**Q6:**
As stated in a previous clarification, the answer to "Can there be more than one vessel type with the same name?" was "No! Names are unique."
Does this mean that we can use the vessel type name as a business identifier, or is there another concept that should be used to identify a Vessel Type?

**A6:**
Definitely, yes.
