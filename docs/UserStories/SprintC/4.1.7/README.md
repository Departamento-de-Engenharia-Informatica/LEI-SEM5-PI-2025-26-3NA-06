# US 4.1.7 - Create Vessel Visit Execution (VVE) Record

## Descrição

As a Logistics Operator, I want to create a Vessel Visit Execution (VVE) record when a vessel arrives at the port, so that the actual start of operations can be logged and monitored.

## Critérios de Aceitação

- The REST API must allow creating a new VVE referencing an existing VVN.
- Recorded fields must include vessel identifier, actual arrival time at the port, and creator user ID. An automatic VVE identifier must be also assigned, whose pattern is like VVN IDs.
- The SPA must easy the VVE creation using available VVN information.
- Once created, the VVE must be marked as "In Progress."

## Perguntas do Fórum (Dev-Cliente)

**Q1:**
Queríamos confirmar se os conceitos de VVE e Operation Plan estão interligados através do VVN.
Isto é, como na User Story 4.1.9 é dito que é possível atualizar VVEs com operações executadas — operações estas que vêm do Operation Plan — e, na User Story 4.1.7, para criar um VVE, este tem como referência um VVN, gostaríamos de perceber se o VVE conhece diretamente o Operation Plan devido ao que é descrito na US 4.1.9 ou se essa ligação é feita indiretamente através do VVN.

**A1:**
Parece-me que o que está em causa é mais uma questão técnica.
Contudo, uma VVE é sempre relativa a uma VVN.
O plano de operações dessa VVE é o que está estabelecido para a respetiva VVN (cf. sugerido na US 4.1.9).
Nota, no entanto, que a informação relativa à execução das operações pode ser distinta da planeada.
Por exemplo, a descarga do contentor X pode estar planeada para se iniciar às 13h52 e, na prática, apenas começar às 14h01.
Ou seja, neste exemplo, haveria um atraso de 9 minutos na realização dessa operação.

---

**Q2:**
"Execution updates must be stored with timestamps and operator ID."
Os timestamps devem ser guardados para cada atualização das operações dos Vessel Visit Executions ou a data da última atualização é suficiente?
Além disso, o ID do logistics operator que é guardado pode ser o email do mesmo?

**A2:**
É suficiente registar o timestamp e o ID de (i) quem criou/iniciou a execução da operação e (ii) de quem marcou a execução da operação como concluída/completa.
O "ID" deve ser, de facto, o ID do operador logístico. Se o "ID" for o email, ok; se não for, então não deveria ser o email.
Além disso, à luz da conformidade com questões legais (por exemplo, RGPD), devem ponderar até que ponto faz sentido que os "IDs" sejam endereços de email e quais as vantagens e desvantagens dessa decisão.
