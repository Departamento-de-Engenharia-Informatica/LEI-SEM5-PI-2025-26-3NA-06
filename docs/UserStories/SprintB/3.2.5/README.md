# US 3.2.5 - Assign Internal Roles

## Descrição

As an Administrator, I want to assign (or update) the internal role(s) of a given user, so that they can access only the features appropriate to their responsibilities.

## Critérios de Aceitação

- Users are identified by IAM-provided attributes (userId, email, name).
- When authorizing a user for the first time:
  - A unique activation link is sent to their email.
  - By default, the users are set to a "deactivated" status.
- Internal roles determine system access level.
