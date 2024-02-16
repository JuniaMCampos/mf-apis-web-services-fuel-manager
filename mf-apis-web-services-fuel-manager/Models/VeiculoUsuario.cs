using System.ComponentModel.DataAnnotations.Schema;

namespace mf_apis_web_services_fuel_manager.Models
{
    [Table("VeiculosUsuarios")]
    public class VeiculoUsuario
    {
        public int VeiculoId { get; set; }

        public Veiculo Veiculo { get; set; }

        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; }
    }
}
