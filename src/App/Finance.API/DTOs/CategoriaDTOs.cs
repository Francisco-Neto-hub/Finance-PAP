namespace Finance.API.DTOs
{
    // O que devolvemos (Read)
    public class CategoriaReadDTO
    {
        public int IdCategoria { get; set; }
        public string Nome { get; set; }
    }

    // O que o Admin envia para criar/editar (Create/Update)
    public class CategoriaCRUDDTO
    {
        public string Nome { get; set; }
    }
}