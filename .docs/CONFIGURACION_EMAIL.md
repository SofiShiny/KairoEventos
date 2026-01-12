# 📧 Configuración de Envío de Emails

El sistema de notificaciones ahora soporta el envío de correos electrónicos de confirmación de pago.

---

## ⚙️ Configuración Actual

Actualmente, el servicio está configurado con valores de prueba en `docker-compose.yml`. **Los correos fallarán** hasta que configures un servidor SMTP real.

## 📝 Cómo Configurar tus Credenciales

1. Abre `Eventos/Infraestructura/docker-compose.yml`.
2. Busca el servicio `notificaciones-api`.
3. Edita las variables de entorno bajo `EmailSettings`:

```yaml
    environment:
      # ...
      - EmailSettings__Host=smtp.gmail.com        # Ejemplo para Gmail
      - EmailSettings__Puerto=587
      - EmailSettings__Usuario=tu_email@gmail.com
      - EmailSettings__Password=tu_app_password   # Contraseña de aplicación (No tu password normal)
      - EmailSettings__NombreEmisor=Kairo Eventos
      - EmailSettings__EmailEmisor=tu_email@gmail.com
      - EmailSettings__UsarSsl=true               # true para Gmail/Outlook
```

### Proveedores Comunes

#### Gmail
- Host: `smtp.gmail.com`
- Puerto: `587`
- SSL: `true`
- **Importante**: Debes activar "Verificación en 2 pasos" y generar una "Contraseña de Aplicación".

#### Outlook / Hotmail
- Host: `smtp.office365.com`
- Puerto: `587`
- SSL: `true`

#### Ethereal (Pruebas)
- Crea una cuenta en [ethereal.email](https://ethereal.email)
- Copia las credenciales generadas.
- Host: `smtp.ethereal.email`
- Puerto: `587`
- SSL: `false` (generalmente usa STARTTLS con puerto 587)

---

## 🔄 Aplicar Cambios

Después de editar el archivo, reinicia el servicio:

```bash
docker compose up -d notificaciones-api
```

## 🔍 Verificar Envíos

Revisa los logs para ver si el envío fue exitoso o falló:

```bash
docker logs kairo-notificaciones --tail 50 -f
```

- ✅ Éxito: `Email enviado exitosamente a ...`
- ❌ Error: `Error crítico al enviar email ...`

El fallo del email **NO** interrumpe el proceso; la notificación en pantalla (SignalR) se enviará de todos modos.
