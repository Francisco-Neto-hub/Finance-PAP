using FinanceUI.Models;
using FinanceUI.Services;
using System.Diagnostics;
using System.Globalization;

namespace FinanceUI.Views;

public partial class TransactionPage : ContentPage
{
    private readonly ApiService _apiService;
    private List<ContaDTO> _contas;
    private bool _isBusy;

    public TransactionPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarContasNoPicker();
    }

    private async Task CarregarContasNoPicker()
    {
        try
        {
            _contas = await _apiService.GetContasAsync();

            if (_contas != null && _contas.Count > 0)
            {
                ContaPicker.ItemsSource = _contas;
                // Verificação de segurança caso o picker de destino ainda não exista na UI
                if (ContaDestinoPicker != null)
                    ContaDestinoPicker.ItemsSource = _contas;
            }
            else
            {
                await DisplayAlertAsync("Aviso", "Nenhuma conta encontrada. Configure as suas contas primeiro.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Transaction Error]: {ex.Message}");
        }
    }

    private void OnTipoCambiado(object sender, CheckedChangedEventArgs e)
    {
        // Só executa se o evento for o "Marcado" (Checked = true)
        if (e.Value && LayoutContaDestino != null)
        {
            LayoutContaDestino.IsVisible = RadioTransferencia.IsChecked;
        }
    }

    private async void AoClicarGuardar(object sender, EventArgs e)
    {
        if (_isBusy) return;

        // 1. Validações de Seleção
        if (ContaPicker.SelectedIndex == -1)
        {
            await DisplayAlertAsync("Campo Obrigatório", "Selecione a conta de origem.", "OK");
            return;
        }

        if (RadioTransferencia.IsChecked)
        {
            if (ContaDestinoPicker.SelectedIndex == -1)
            {
                await DisplayAlertAsync("Campo Obrigatório", "Selecione a conta de destino.", "OK");
                return;
            }

            if (ContaPicker.SelectedItem == ContaDestinoPicker.SelectedItem)
            {
                await DisplayAlertAsync("Erro", "A conta de origem e destino não podem ser iguais.", "OK");
                return;
            }
        }

        if (CategoriaPicker.SelectedIndex == -1)
        {
            await DisplayAlertAsync("Erro", "Selecione uma categoria.", "OK");
            return;
        }

        // 2. Validação de Valor (Tratamento de vírgulas/pontos)
        string valorTexto = ValorEntry.Text?.Replace(',', '.') ?? "0";
        if (!decimal.TryParse(valorTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valorDigitado) || valorDigitado <= 0)
        {
            await DisplayAlertAsync("Erro", "Insira um valor válido superior a zero.", "OK");
            return;
        }

        try
        {
            _isBusy = true;
            Indicador.IsRunning = true;
            BtnGuardar.IsEnabled = false;

            string categoriaNome = CategoriaPicker.SelectedItem?.ToString() ?? "Outros";

            // 3. Preparação dos Dados
            var contaOrigem = (ContaDTO)ContaPicker.SelectedItem;

            // Mapeamento de Categoria (Melhorado com fallback)
            int idCat = MapearCategoriaParaId(categoriaNome);

            // Determinar Tipo (1: Receita, 2: Despesa, 3: Transferência)
            int tipoId = RadioReceita.IsChecked ? 1 : (RadioDespesa.IsChecked ? 2 : 3);

            // Ajuste de Data e Hora
            // Garantimos que pegamos a data do picker (ou a de hoje se falhar) 
            // e somamos a hora atual para o registo ficar preciso.
            DateTime dataDoPicker = (DateTime)DataPicker.Date; // O DatePicker do MAUI normalmente já é DateTime, não nullable
            DateTime dataFinalComHora = dataDoPicker.Add(DateTime.Now.TimeOfDay);

            var dados = new TransacaoRequestDTO
            {
                NomeTransacao = string.IsNullOrWhiteSpace(DescricaoEntry.Text) ? categoriaNome : DescricaoEntry.Text.Trim(),
                ValorTransacao = valorDigitado,
                IdCategoria = idCat,
                DataTransacao = dataFinalComHora,                
                IdContaOrigem = contaOrigem.Id,
                IdTipo = tipoId,
                IdContaDestino = RadioTransferencia.IsChecked ? ((ContaDTO)ContaDestinoPicker.SelectedItem).Id : null
            };

            // 4. Enviar para API
            bool sucesso = await _apiService.PostTransacaoAsync(dados);

            if (sucesso)
            {
                await DisplayAlertAsync("Sucesso ✨", "Movimento registado!", "OK");
                LimparCampos();

                // Opcional: Voltar para a Dashboard automaticamente
                // await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlertAsync("Erro", "A API recusou a transação. Verifique se tem saldo suficiente.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro Crítico", "Falha na ligação com o servidor.", "OK");
            Debug.WriteLine(ex.Message);
        }
        finally
        {
            _isBusy = false;
            Indicador.IsRunning = false;
            BtnGuardar.IsEnabled = true;
        }
    }

    private int MapearCategoriaParaId(string categoriaNome)
    {
        return categoriaNome switch
        {
            "Salário" => 1,
            "Alimentação" => 2,
            "Transporte" => 3,
            "Lazer" => 4,
            "Saúde" => 5,
            "Habitação" => 6,
            _ => 1002 // Categoria "Outros" ou padrão
        };
    }

    private void LimparCampos()
    {
        DescricaoEntry.Text = string.Empty;
        ValorEntry.Text = string.Empty;
        CategoriaPicker.SelectedIndex = -1;
        ContaPicker.SelectedIndex = -1;

        if (ContaDestinoPicker != null)
            ContaDestinoPicker.SelectedIndex = -1;

        DataPicker.Date = DateTime.Now;
        RadioDespesa.IsChecked = true; // Geralmente as pessoas registam mais despesas
    }
}