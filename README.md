## Contexto

Uma loja recebe pedidos de clientes e seleciona uma pessoa para realizar o atendimento do pedido, o sistema distribui igualmente os pedidos para os atendentes.

Em até 1 hora do cadastro do pedido, o cliente pode cancelar o pedido.

<br>

## Questão 1

Desenvolvendo um teste unitário para garantir o comportamento esperado do método de cancelamento de pedido.

### Cenário:

Um pedido é realizado no loja que possui uma política de permitir alterações no pedido até 1 hora após a realização do mesmo, para casos em que algum produto esteja em falta;

O pedido é recebido às 17:00.

Está configurado no restaurante que, após ultrapassar o limite de 1 hora para aprovar alterações, o pedido será automaticamente enviado para produção.

### Ação

Na tela de aprovar alteração de produto, cliente clica em “Cancelar pedido” as 18:20.

### Verificação

O sistema deve recusar a solicitação de cancelamento do pedido, pois a ação foi realizada após o limite de tempo de 1 hora, que expirou às 18:00.

<br>

## Questão 2

Um erro em produção está acontecendo ao distribuir os pedidos aos atendentes, corrija o erro utilizando testes automatizados para simular a distribuição dos pedidos.


## Cenário

Após x números de pedidos serem distribuídos, os proxímos pedidos passam a serem distribuídos sempre para o mesmo atendente. 

### Ação

Distribuir vários pedidos aos atendentes.

### Verificação

Verificar se cada atendente recebeu igualmente os pedidos.

<br>

## Questão 3

*(ainda não implementado)*

Uma atualização exige novos campos nos pedidos, porém a API de novos pedidos precisa ser funcionando para não prejudicar o setor do Marketing Digital.

Adicione os novos campos e garante que a API vai seguir funcionando.



# Gabarito bug

O seu código **quase** entrega o atendente que está mais tempo sem cliente, mas há um detalhe importante que pode estar te enganando. Vamos analisar:

### O que o código faz:

1. Para cada `Attendant`, ele:

   * Inclui os `Orders` (pedidos) relacionados.
   * Depois, inclui a `Store` de cada pedido (embora isso não seja usado na lógica do método).

2. Para cada atendente (`a`), ele cria um objeto anônimo com:

   * `a` (o próprio atendente)
   * `LastOrderCreatedAt`, que **na verdade** está pegando o **primeiro pedido ordenado por data de criação crescente**.

   ```csharp
   LastOrderCreatedAt = a.Orders!.OrderBy(o => o.CreatedAt).FirstOrDefault()
   ```

   Isso está pegando o **pedido mais antigo** daquele atendente, não o mais recente.

3. Em seguida, ordena os atendentes pela data desse pedido mais antigo (`LastOrderCreatedAt.CreatedAt`).

### Problema

Você quer o atendente que está **mais tempo sem receber pedido**, certo?

Para isso, precisa pegar o **último pedido** de cada atendente (o mais recente) e ordenar do **mais antigo para o mais recente** — ou seja, o que recebeu o último pedido há mais tempo.

### Correção

Troque:

```csharp
LastOrderCreatedAt = a.Orders!.OrderBy(o => o.CreatedAt).FirstOrDefault(),
```

Por:

```csharp
LastOrderCreatedAt = a.Orders!.OrderByDescending(o => o.CreatedAt).FirstOrDefault(),
```

### Código Corrigido:

```csharp
public class AttendantService(AppDbContext dbContext) : IAttendantService
{
    public async Task<Attendant?> GetNextAttendantIdForOrderDistributionAsync()
    {
        var result = await dbContext
            .Attendants.Include(x => x.Orders!)
            .ThenInclude(o => o.Store)
            .Select(a => new
            {
                a,
                LastOrderCreatedAt = a.Orders!
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefault()
            })
            .OrderBy(r =>
                r.LastOrderCreatedAt == null
                    ? DateTime.MinValue
                    : r.LastOrderCreatedAt.CreatedAt
            )
            .ToListAsync();

        return result.FirstOrDefault()?.a;
    }
}
```

### Explicação

* `OrderByDescending(o => o.CreatedAt)` → pega o pedido mais recente.
* Ao ordenar pelo `CreatedAt`, você acaba trazendo primeiro aquele atendente cujo **último pedido foi há mais tempo** — ou seja, o que está esperando há mais tempo por um novo pedido.

### Conclusão

Com essa correção, **sim**, o método passa a entregar corretamente o atendente que está mais tempo sem receber um pedido.

