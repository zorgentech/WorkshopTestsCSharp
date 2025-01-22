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


