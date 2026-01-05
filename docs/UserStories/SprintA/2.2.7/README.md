# US 2.2.7 - Review Vessel Visit Notifications (Approve/Reject)

## Descrição

As a Port Authority Officer, I want to review pending Vessel Visit Notifications and approve or reject them, so that docking schedules remain under port control.

## Critérios de Aceitação

- When a notification is approved, the officer must assign a (temporarily) dock on which the vessel should berth.
- When a notification is rejected, the officer must provide a reason for rejection (e.g., information is missing).
- If rejected, the shipping agent representative might review / update the notification for further new decision.
- All decisions (approve/reject) must be logged with timestamp, officer ID, and decision outcome for auditing purposes.

## Perguntas do Fórum (Dev-Cliente)

**Q1:**
Knowing that a Vessel Visit Notification (VVN) can be modified after being rejected (and this may happen multiple times for the same VVN), is it necessary for the system to store the VVN information for each update stage it goes through?
This is not referring to the audit log (timestamp, officer ID, decision outcome), but to the actual VVN data, so that during a historical review it is possible to know the VVN's details at each stage.

**A1:**
It would be a nice feature.
I will only consider it later, as a system improvement.
