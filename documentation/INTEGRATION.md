# 📘 Manual de Integração: Gestao.UI ↔ Gestao.Core

Este guia descreve como utilizar as funcionalidades do "motor" financeiro no projeto de interface WPF.

---

## 1. Configuração Inicial
Para utilizar qualquer serviço, deves instanciar a `DbConnectionFactory` e passá-la ao serviço pretendido.

```csharp
// Exemplo de inicialização no Form ou ViewModel 
var factory = new DbConnectionFactory();
var movimentoService = new MovimentoService(factory);
var authService = new AuthService(factory);
var categoriaService = new CategoriaService(factory);
```
## 2. Sistema de Login (Segurança)

O login utiliza `BCrypt`. Não precisas de te preocupar com a encriptação na UI; o Core trata disso internamente.

```csharp
try {
    bool loginSucesso = authService.ValidarUtilizador(txtEmail.Text, txtPassword.Password);
    
    if (loginSucesso) {
        // Abrir Dashboard
    } else {
        MessageBox.Show("Credenciais inválidas ou utilizador inativo.");
    }
}
catch (Exception ex) {
    Logger.LogError(ex); // Grava o erro técnico no log [cite: 8]
    MessageBox.Show("Erro ao ligar ao servidor.");
}
```

## 3. Gestão de Movimentos e Validações

Sempre que registares um movimento, o Core vai validar os dados automaticamente. Importante: Deves capturar a `BusinessException` para mostrar avisos amigáveis ao utilizador.

```csharp
var novoMovimento = new Movimento {
    Valor = decimal.Parse(txtValor.Text),
    Descricao = txtDescricao.Text,
    IdConta = 1, // Obter do utilizador logado
    IdCategoria = (int)cbCategorias.SelectedValue,
    IdTipoMovimento = (int)TipoMovimento.Saida,
    Data = dpData.SelectedDate ?? DateTime.Now
};

try {
    movimentoService.RegistarMovimento(novoMovimento);
    MessageBox.Show("Sucesso!");
}
catch (BusinessException ex) {
    [cite_start]// Erros de validação (ex: "Valor deve ser positivo") [cite: 12]
    MessageBox.Show(ex.Message, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
}
catch (DatabaseException ex) {
    // Erros de ligação ao SQL
    Logger.LogError(ex);
    MessageBox.Show("Erro crítico de base de dados.");
}
```
## 4. Dashboard e Gráficos
Para preencher o gráfico de despesas por categoria, utiliza o método `ObterGastosPorCategoria`. Ele devolve os totais e as percentagens já calculadas pelo Core.

```csharp
// Retorna IEnumerable<RelatorioCategoria>
var dadosGrafico = movimentoService.ObterGastosPorCategoria(idContaAtiva);

foreach (var item in dadosGrafico) {
    // Exemplo de uso:
    // item.CategoriaNome
    // item.TotalGasto
    // item.Percentagem
}
```
## 5. Preencher Listas (ComboBoxes)
Para garantir que o utilizador escolhe categorias válidas, carrega a lista no arranque do ecrã:

```csharp
cbCategorias.ItemsSource = categoriaService.ListarTodas(); 
cbCategorias.DisplayMemberPath = "Nome";
cbCategorias.SelectedValuePath = "IdCategoria";
```
## 6. Boas Práticas para a UI

>Try-Catch: Usa sempre em operações de escrita/leitura na Base de Dados. 

>Logger: Se o erro for inesperado (Exception genérica), chama sempre Logger.LogError(ex). 

>Enums: Usa TipoMovimento.Entrada ou TipoMovimento.Saida em vez de números soltos (1 ou 2).

## 7. Registo de Novos Utilizadores
Para criar uma nova conta, envia o objeto Utilizador e a password (ainda em texto limpo). O Core tratará de validar e encriptar.

```csharp
var novoUser = new Utilizador { 
    Nome = txtNome.Text, 
    Email = txtEmail.Text 
};

try {
    userService.RegistarUtilizador(novoUser, txtPassword.Password);
    MessageBox.Show("Conta criada com sucesso!");
}
catch (BusinessException ex) {
    MessageBox.Show(ex.Message, "Erro de Validação");
}

Nota de Arquitetura: Para manter o código limpo, utiliza sempre a interface IUserService para declarar as variáveis, e a classe UserService apenas para instanciar.

// Forma correta de instanciar na UI:
IUserService _userService = new UserService(new DbConnectionFactory());
```

## 8. Verificação de Versão
No arranque da aplicação, verifica se o utilizador tem a versão mais recente.

```csharp
var versionService = new VersionService(new DbConnectionFactory());
string versaoSoftware = "1.0.0"; // Versão desta App

if (!versionService.VerificarSeVersaoEAtual(versaoSoftware)) {
    var nova = versionService.ObterUltimaVersao();
    MessageBox.Show($"Nova versão disponível ({nova.Versao})! Descarregue em: {nova.UrlDownload}");
}
```
## 9. Gestão de Contas
Antes de lançar movimentos, deves obter as contas do utilizador logado.

```csharp
// Exemplo: Carregar ComboBox de Contas
var contas = contaService.ListarContasPorUtilizador(userLogado.IdUtilizador);
cbContas.ItemsSource = contas;
cbContas.DisplayMemberPath = "Nome";
cbContas.SelectedValuePath = "IdConta";
```
## 10. Histórico e Auditoria
O sistema regista automaticamente alterações críticas. Para operações de edição, deves sempre enviar o ID do utilizador que está a realizar a alteração.

- **Método:** `AtualizarMovimento(movimento, valorAntigo, idUser)`
- **Tabela Relacionada:** `Historico`

## 11. Sistema de Alertas
Podes listar as notificações pendentes do utilizador para mostrar alertas de saldo ou segurança.

```csharp
var alertas = alertService.ObterAlertasAtivos(userLogado.IdUtilizador);
foreach (var msg in alertas) {
    // Mostrar na lista de notificações da UI
}
```