using FinanceUI.Models;
using System.Diagnostics;

namespace FinanceUI.Views;

public partial class TransactionPage : ContentPage
{
    private readonly ApiService _apiService;
    private List<ContaExemplo> _contas;

    // O construtor recebe o ApiService via Injeção de Dependência conforme configurado no MauiProgram
    public TransactionPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    // Método executado sempre que a página aparece no ecrã
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarContasNoPicker();
    }
    private async Task CarregarContasNoPicker()
    {
        try
        {
            // 1. Puxa as contas do utilizador através do serviço
            _contas = await _apiService.GetContasAsync();

            if (_contas != null && _contas.Count > 0)
            {
                // 2. Preenche o picker de Origem
                ContaPicker.ItemsSource = _contas;
                
                // 3. ADICIONA ESTA LINHA: Preenche também o picker de Destino
                ContaDestinoPicker.ItemsSource = _contas;
            }
            else
            {
                await DisplayAlertAsync("Aviso", "Não foram encontradas contas. Crie uma conta primeiro.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao carregar contas: {ex.Message}");
        }
    }

    // 1. Controlar a visibilidade do campo de destino
    private void OnTipoCambiado(object sender, CheckedChangedEventArgs e)
    {
        if (LayoutContaDestino != null)
            LayoutContaDestino.IsVisible = RadioTransferencia.IsChecked;
    }

    private async void AoClicarGuardar(object sender, EventArgs e)
    {
        // 1. Validações Iniciais
        if (ContaPicker.SelectedIndex == -1)
        {
            await DisplayAlertAsync("Campo Obrigatório", "Por favor, selecione a conta.", "OK");
            return;
        }

        // Validação extra se for transferência: precisa de conta de destino
        // (Certifica-te que já criaste o 'RadioTransferencia' e o 'ContaDestinoPicker' no XAML)
        if (RadioTransferencia.IsChecked && ContaDestinoPicker.SelectedIndex == -1)
        {
            await DisplayAlertAsync("Campo Obrigatório", "Por favor, selecione a conta de destino para a transferência.", "OK");
            return;
        }

        if (RadioTransferencia.IsChecked && ContaPicker.SelectedItem == ContaDestinoPicker.SelectedItem)
        {
            await DisplayAlertAsync("Erro", "A conta de origem e destino não podem ser a mesma.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(ValorEntry.Text) || CategoriaPicker.SelectedIndex == -1)
        {
            await DisplayAlertAsync("Erro", "Preencha o valor e a categoria!", "OK");
            return;
        }

        if (!decimal.TryParse(ValorEntry.Text, out decimal valorDigitado))
        {
            await DisplayAlertAsync("Erro", "O valor inserido é inválido!", "OK");
            return;
        }

        // 2. UI Feedback
        Indicador.IsRunning = true;
        BtnGuardar.IsEnabled = false;

        // Obtemos a conta selecionada
        var contaSelecionada = (ContaExemplo)ContaPicker.SelectedItem;

        // --- LÓGICA DE TRADUÇÃO PARA A API ---

        // A. Traduzir Categoria (Texto do Picker -> ID da Base de Dados)
        // Nota: Certifica-te que estes IDs batem certo com a tua tabela 'Categoria'
        string? categoriaNome = CategoriaPicker.SelectedItem?.ToString();
        int idCat = 1002; // Valor padrão: 1002 (Outros)

        switch (categoriaNome)
        {
            case "Salário": idCat = 1; break;
            case "Alimentação": idCat = 2; break;
            case "Transporte": idCat = 3; break;
            case "Lazer": idCat = 4; break;
            case "Saúde": idCat = 5; break;
            case "Habitação": idCat = 6; break;
            case "Outros": idCat = 1002; break;
        }

        // B. Traduzir Tipo de Movimento (1 = Receita, 2 = Despesa, 3 = Transferência)
        int tipoId = RadioReceita.IsChecked ? 1 : (RadioDespesa.IsChecked ? 2 : 3);

        // C. RESOLVER A HORA: Juntar a data escolhida no Picker com a hora exata atual do sistema
        DateTime dataSelecionada = DataPicker.Date ?? DateTime.Now;
        TimeSpan horaAtual = DateTime.Now.TimeOfDay;
        DateTime dataFinalComHora = dataSelecionada.Add(horaAtual);

        // 3. Preparar os dados (TransacaoRequest) alinhados com o Backend
        var dados = new TransacaoRequest
        {
            NomeTransacao = DescricaoEntry.Text,
            ValorTransacao = valorDigitado,     // Vai sempre positivo (a API trata do resto)
            IdCategoria = idCat,                // Agora enviamos o ID numérico
            DataTransacao = dataFinalComHora,   // Agora vai com a hora real (ex: 15:30) em vez de 00:00!
            IdContaOrigem = contaSelecionada.Id,// Nome corrigido para bater com a API
            IdTipo = tipoId,                     // Enviamos 1 ou 2 em vez de texto
            // Só tenta ler a conta de destino se for uma transferência
            IdContaDestino = RadioTransferencia.IsChecked ? ((ContaExemplo)ContaDestinoPicker.SelectedItem).Id : null
        };

        try
        {
            // 4. Chamada real à API
            bool sucesso = await _apiService.PostTransacaoAsync(dados);

            if (sucesso)
            {
                await DisplayAlertAsync("Sucesso", "Transação registada com sucesso!", "OK");
                
                // 5. Limpar os campos para permitir registar logo outro movimento
                LimparCampos();

                // Comentei o PopAsync para a página não fechar automaticamente. 
                // Assim o utilizador pode registar várias coisas seguidas.
                // await Navigation.PopAsync();
            }
            else
            {
                // Se der erro, a API provavelmente enviou uma mensagem (Saldo ou Conta)
                await DisplayAlertAsync("Erro", "A transação foi recusada pela API. Verifique os dados ou o saldo.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro Crítico", $"Erro de ligação: {ex.Message}", "OK");
        }
        finally
        {
            // 5. Restaurar a UI
            Indicador.IsRunning = false;
            BtnGuardar.IsEnabled = true;
        }
    }

    // NOVO MÉTODO PARA LIMPAR A UI
    private void LimparCampos()
    {
        DescricaoEntry.Text = string.Empty;
        ValorEntry.Text = string.Empty;
        CategoriaPicker.SelectedIndex = -1;
        ContaPicker.SelectedIndex = -1;

        // Se já tens o picker de destino na UI, limpamos também:
        if (ContaDestinoPicker != null) ContaDestinoPicker.SelectedIndex = -1;

        DataPicker.Date = DateTime.Now;
        RadioReceita.IsChecked = true; // Faz reset do tipo de movimento para "Receita"
    }
}