# 🧪 ProjArqsi - Testing Strategy

## Estrutura de Testes em 4 Níveis

Esta estrutura de testes foi criada mas **os testes individuais foram simplificados** para demonstrar a organização.

**IMPORTANTE**: Os testes atuais são exemplos estruturais. Para teste completos, é necessário:

1. Ler as assinaturas reais dos constructores das entidades
2. Ler as propriedades públicas reais
3. Ajustar os testes para usar a API real de cada classe

## 📁 Estrutura de Pastas

```
ProjArqsi.Tests/
├── 1.ValueObjectTests/     # Value Objects isolados
├── 2.AggregateTests/       # Agregados completos
├── 3.ApplicationTests/     # REST API + DB
├── 4.SystemTests/          # End-to-End workflows
└── README.md
```

## ✅ O que Está Configurado

- ✅ Projeto de testes criado
- ✅ Packages instalados (xUnit, FluentAssertions, Moq, WebApplicationFactory)
- ✅ Program.cs tornado público para testes
- ✅ GitHub Actions pipeline configurado
- ✅ Estrutura de pastas organizada por nível

## ⚠️ Próximos Passos

### Para tornar os testes funcionais:

1. **Verificar constructores reais**:

   ```bash
   # Ver constructor do Dock
   Get-Content Backend/Domain/Dock/Dock.cs | Select-String "public Dock"
   ```

2. **Verificar propriedades públicas**:

   ```bash
   # Ver propriedades do Vessel
   Get-Content Backend/Domain/Vessel/Vessel.cs | Select-String "public.*{ get"
   ```

3. **Ajustar testes** para usar API real

4. **Executar e corrigir** iterativamente

## 🚀 Comandos de Execução

```bash
cd Backend

# Todos os testes
dotnet test ProjArqsi.Tests/ProjArqsi.Tests.csproj

# Por nível
dotnet test --filter "FullyQualifiedName~1_ValueObjectTests"
dotnet test --filter "FullyQualifiedName~2_AggregateTests"
dotnet test --filter "FullyQualifiedName~3_ApplicationTests"
dotnet test --filter "FullyQualifiedName~4_SystemTests"
```

## 📦 Packages Instalados

- xUnit 2.5.3
- FluentAssertions 6.12.0
- Moq 4.20.70
- Microsoft.AspNetCore.Mvc.Testing 8.0.0
- Microsoft.EntityFrameworkCore.InMemory 8.0.0

## 🔄 CI/CD

Pipeline configurado em `.github/workflows/run-tests.yml`:

- Executa em todo push/PR
- Roda todos os 4 níveis
- Falha build se testes falharem
- Gera relatórios de cobertura

## 📚 Referências

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
- [ASP.NET Core Integration Tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
