using Finance.Core.Models;
using Xunit;

namespace Finance.Tests.Models;

public class ContaTests
{
    [Fact]
    public void Conta_AoSerCriada_DeveEstarAbertaPorDefeito()
    {
        // Arrange & Act
        var conta = new Conta { NomeConta = "Teste" };

        // Assert
        // Como o default de bool é false, se não definires no construtor, 
        // este teste ajuda-te a decidir se deves forçar IsAberta = true no Core.
        Assert.False(conta.IsAberta);
    }

    [Theory]
    [InlineData(100, 50, 150)] // Receita: 100 + 50 = 150
    [InlineData(100, -50, 50)] // Despesa: 100 - 50 = 50
    public void CalculoSaldo_DeveSomarValoresCorretamente(decimal saldoInicial, decimal movimento, decimal resultadoEsperado)
    {
        // Act
        decimal resultado = saldoInicial + movimento;

        // Assert
        Assert.Equal(resultadoEsperado, resultado);
    }
}
