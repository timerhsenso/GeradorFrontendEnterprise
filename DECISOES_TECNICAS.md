# Decisões Técnicas - Gerador Frontend Enterprise v2.0

## Fase 1-3: Arquitetura e Modelos

### 1. Estrutura de Pastas (Clean Architecture)
- **Core/** - Contratos, modelos, enumerações (sem dependências externas)
- **Infrastructure/** - Implementações de componentes (SchemaReader, ManifestClient, TemplateEngine)
- **Services/** - Serviços de negócio (Orchestrator, Generator)
- **Tests/** - Testes unitários e de integração

**Justificativa:** Separação clara de responsabilidades, facilita testes e manutenção.

---

## Fase 4: Schema Reader

### 2. Uso de System.Data.SqlClient vs Microsoft.Data.SqlClient
**Decisão:** System.Data.SqlClient (inicialmente)
**Justificativa:** 
- Compatibilidade com .NET Framework legado
- Será migrado para Microsoft.Data.SqlClient em versão futura
- Avisos de obsolescência são aceitáveis nesta fase

### 3. Fonte de Metadados SQL Server
**Decisão:** Information_Schema + Extended Properties
**Justificativa:**
- Information_Schema: Padrão SQL, portável
- Extended Properties: Suporta descrições customizadas
- Combinação oferece visão completa do schema

### 4. Mapeamento SQL → CLR Types
**Decisão:** Mapeamento automático com fallback para `object`
**Justificativa:**
- Cobertura de 95% dos tipos comuns
- Fallback seguro para tipos desconhecidos
- Suporta tipos nullable automaticamente

### 5. Validação em 3 Níveis
```
Nível 1: TableSchema.Validate() - Integridade do schema
Nível 2: ColumnSchema.Validate() - Cada coluna
Nível 3: ValidationResult - Consistência banco vs manifesto
```
**Justificativa:** Detecção precoce de problemas, mensagens claras.

---

## Fase 5: Manifest Client

### 6. Comunicação HTTP com API
**Decisão:** HttpClient com Dependency Injection
**Justificativa:**
- Padrão recomendado .NET
- Reutilização de conexões
- Fácil mock para testes

### 7. Tratamento de Erros
**Decisão:** Log + Exception + Graceful Degradation
**Justificativa:**
- Rastreabilidade completa
- Falhas não silenciosas
- Recuperação possível em camadas superiores

### 8. Caching de Templates (Futuro)
**Decisão:** Dictionary em memória (implementado)
**Justificativa:**
- Performance
- Reduz I/O
- Invalidação simples

---

## Fase 6: Template Engine (Scriban)

### 9. Escolha de Scriban vs Roslyn
**Decisão:** Scriban para templates, Roslyn para análise de código (futuro)
**Justificativa:**
- Scriban: Simples, rápido, seguro (sem execução de código)
- Roslyn: Complexo, mas necessário para análise de código gerado

### 10. Estrutura de Templates
```
Templates/
├── Controller.cs.scriban
├── Index.cshtml.scriban
├── ViewModel.cs.scriban
├── JavaScript.js.scriban
└── Styles.css.scriban
```
**Justificativa:** Modularidade, reutilização, fácil manutenção.

### 11. Variáveis Globais em Templates
```scriban
{{ entity.entity_id }}
{{ entity.entity_name }}
{{ config.config_hash }}
{{ now }}
{{ grid.columns }}
{{ form.fields }}
```
**Justificativa:** Consistência, documentação automática.

---

## Fase 7-12: Próximas Decisões (Planejadas)

### 12. Orchestrator Service
- Padrão Saga para coordenação
- Transações distribuídas (se necessário)
- Circuit breaker para falhas de API

### 13. Geração de Código
- Idempotência via hash de configuração
- Separação `.generated.cs` vs `.custom.cs`
- Versionamento de templates

### 14. Testes
- xUnit para unitários
- SQLite in-memory para integração
- Moq para mocks

### 15. Deployment
- Docker para containerização
- CI/CD com GitHub Actions
- Versionamento semântico

---

## Princípios Aplicados

### Hierarquia de Fontes da Verdade
```
Banco de Dados > Manifesto > Configuração do Wizard
```
- Banco: Estrutura real
- Manifesto: Regras de negócio
- Wizard: Apresentação visual

### Idempotência
- Gerar N vezes = mesmo resultado
- Hash SHA256 da configuração
- Rastreabilidade completa

### Não-Inferência
- ❌ Não inferir tipos por nome
- ✅ Usar metadata explícito
- ✅ Falhar rápido se ambíguo

### Separação Gerado vs Customizado
- `*.generated.cs` - sempre regerado
- `*.custom.cs` - nunca alterado
- Partial classes para extensão

---

## Métricas de Qualidade

| Métrica | Alvo | Status |
|---------|------|--------|
| Cobertura de testes | 80% | Em progresso |
| Complexidade ciclomática | < 10 | OK |
| Linhas de código por método | < 50 | OK |
| Documentação XML | 100% | ✓ Completo |
| Build sem erros | 100% | ✓ Completo |
| Build sem warnings | 100% | ⚠️ 16 warnings (obsolescência) |

---

## Próximas Revisões

1. **Migração para Microsoft.Data.SqlClient**
2. **Implementação de cache distribuído**
3. **Suporte a múltiplos bancos (PostgreSQL, MySQL)**
4. **Geração de testes unitários automáticos**
5. **Dashboard de monitoramento**

---

**Última atualização:** 2025-12-26  
**Versão:** 2.0  
**Status:** Em desenvolvimento (Fase 5)
