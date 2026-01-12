using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Usuarios.Application.Dtos.UsuarioKeycloakDto;

namespace Usuarios.Application.Dtos.UsuarioKeycloakDto
{
    public class UsuarioKeycloak
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;
        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; set; } = false;
        [JsonPropertyName("credentials")]
        public List<Credenciales> Credentials { get; set; } = [];
        [JsonPropertyName("attributes")]
        public Atributos Attributes { get; set; } = new Atributos();
    }
}
