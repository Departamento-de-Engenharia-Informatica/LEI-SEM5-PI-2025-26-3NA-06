# US 2.2.8 - Create/Submit Vessel Visit Notification

## Descrição

As a Shipping Agent Representative, I want to create/submit a Vessel Visit Notification, so that the vessel berthing and subsequent (un)loading operations at the port are scheduled and planned in space and timely manner.

## Critérios de Aceitação

- The Cargo Manifest data for unloading and/or loading is included.
- The system must validate that referred containers identifiers comply with the ISO 6346:2022 standard.
- Information about the crew (name, citizen id, nationality) might be requested, when necessary, for compliance with security protocols.
- Vessel Visit Notifications might become at an "in progress" status (e.g. cargo information is incomplete) to be further update/completed.
- When completed / ready for asking approval, the agent is required to change its state to "submitted".

## 3. Análise

### 3.1. Domínio

_A desenvolver: Identificar as entidades, agregados e value objects do domínio relacionados com esta US._

### 3.2. Regras de Negócio

_A desenvolver: Documentar as regras de negócio específicas desta funcionalidade._

### 3.3. Casos de Uso

_A desenvolver: Descrever os principais casos de uso e seus fluxos._

## Perguntas do Fórum (Dev-Cliente)

**Q1:**
Quando fazemos uma notificação, o navio só está a carregar um tipo de carga?

**A1:**
Não. O tipo de carga varia de um contentor para outro. No entanto, no mesmo manifesto de carga podem existir vários contentores com o mesmo tipo de carga.

**Follow-up:**
Sendo assim, o Vessel Visit Notification não tem de incluir cargo type e volume?

**Follow-up Answer:**
Não compreendo a pergunta. Tendo em conta a informação disponível, o que está descrito é claro.

**Follow-up 2:**
Agora percebi. O cargo type aplica-se a cada contentor individual, mas o volume mencionado aplica-se ao volume total de todos os contentores associados a um Vessel Visit Notification ou é também individual por contentor?

**Follow-up Answer 2:**
O "volume" refere-se aos contentores no seu conjunto. Por vezes são 100, outras 1000, outras 18 000 contentores, etc.

---

**Q2:**
Is there something that uniquely identifies the cargo manifest?

**A2:**
Being completely honest, I don't understand the scope/intend of your question. The Cargo Manifest is an integral part of a Vessel Visit Notification. I recommend reading section 3.1.6 of the System Description document.

**Follow-up:**
Is there something that uniquely identifies the cargo manifest?

**Follow-up Answer:**
Yes. A cargo manifest is identifiable by the VVN in which it belongs to, together with its purpose (loading or unloading). Other identifiers may exist but, currently, are out of scope.

---

**Q3:**
"Information about the crew (name, citizen id, nationality) might be requested, when necessary, for compliance with security protocols."
Gostaríamos de saber se esta informação é relativa apenas ao capitão ou a todos os membros da crew.

**A3:**
In the context of a Vessel Visit Notification, it is always necessary to request the captain's identification (name, citizen ID, nationality) and the total number of crew members on board.
Additionally, when transporting dangerous cargo, it is also necessary to request the identification (name, citizen ID, nationality) of the safety officers.

---

**Q4:**
In US 2.2.8, the acceptance criteria mention that information can be added in the future, giving the example of cargo information.
Can crew information or other data also be changed or added later, or only certain types of information?

**A4:**
While the Vessel Visit Notification status is "in progress" (US 2.2.8 and US 2.2.9), all its data can be changed or added.
Once it is submitted, it can no longer be changed by the Shipping Agent Representative.
