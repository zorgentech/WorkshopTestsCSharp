# Feature 1

### Arrange:

Um pedido é realizado no restaurante que possui uma política de permitir alterações no pedido até 1 hora após a realização do mesmo, para casos em que algum produto esteja em falta;

O pedido é recebido às 17:00.

Está configurado no restaurante que, após ultrapassar o limite de 1 hora para aprovar alterações, o pedido será automaticamente enviado para produção.

### Action

Na tela de aprovar alteração de produto, cliente clica em “Cancelar pedido” as 18:20.

### Assert

O sistema deve recusar a solicitação de cancelamento do pedido, pois a ação foi realizada após o limite de tempo de 1 hora, que expirou às 18:00.
