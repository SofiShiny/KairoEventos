using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Usuarios.Application.Dtos.UsuarioKeycloakDto
{
    public class Atributos
    {
        [JsonPropertyName("number")]
        public List<string> Number { get; set; } = [];
        [JsonPropertyName("address")]
        public List<string> Address { get; set; } = [];
        [JsonPropertyName("name")]
        public List<string> Name { get; set; } = [];
    }
}
