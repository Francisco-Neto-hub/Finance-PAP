using Finance.Core.Enums;
using Xunit;

namespace Finance.Tests.Enums;

public class EnumTests
{
    [Fact]
    public void TipoMovimento_DeveTerValoresFixosParaSQL()
    {
        // Assert
        Assert.Equal(1, (int)TipoMovimento.Receita);
        Assert.Equal(2, (int)TipoMovimento.Despesa);
    }
}