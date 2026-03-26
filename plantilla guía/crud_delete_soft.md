# Plantilla: CRUD - Método DELETE (Borrado Lógico / Soft Delete)

**Concepto:** En aplicaciones empresariales **nunca se hace un Hard Delete** (borrar físicamente la fila con `DELETE FROM...`). En su lugar, usamos **Soft Delete**: actualizamos una columna `DeletedAt` con la fecha y hora actual. El registro desaparece de la aplicación (porque en los `GET` filtramos los que tengan `DeletedAt == null`), pero se conserva en la base de datos por motivos de auditoría.

---

## 🛡️ Paso 1: El Backend (Controlador .NET)

Abre `Controllers/<NombreEntidad>Controller.cs`.
**Regla:** El estándar REST para un borrado exitoso dicta que debemos devolver un código HTTP **204 No Content** (que significa: "La acción se completó, pero no tengo datos que devolverte").

> **⚠️ NOTA DE ARQUITECTURA (using System):** Para usar `DateTime.UtcNow`, asegúrate de tener `using System;` en la parte superior de tu archivo.

```csharp
using System; 
using Microsoft.AspNetCore.Mvc;
// ... otros usings ...

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete<NombreEntidad>(string id)
        {
            // 1. Buscamos la entidad en la base de datos
            var entidad = await _context.<NombreDbSet>.FindAsync(id);

            // 2. Si no existe (o ya está borrada), devolvemos 404 Not Found
            if (entidad == null || entidad.DeletedAt != null)
            {
                return NotFound(new { mensaje = "El registro no existe o ya fue eliminado." });
            }

            // 3. LA MAGIA DEL SOFT DELETE: Actualizamos la fecha en lugar de borrar
            // NUNCA hacer: _context.<NombreDbSet>.Remove(entidad);
            entidad.DeletedAt = DateTime.UtcNow;

            // 4. Guardamos los cambios (Entity Framework hará un UPDATE, no un DELETE)
            await _context.SaveChangesAsync();

            // 5. Devolvemos 204 No Content
            return NoContent(); 
        }
```

---

## 🚀 ¿Por qué este enfoque es Superior?
1. **Seguridad de Datos:** Si un usuario borra algo por error, un administrador (o tú desde la BBDD) puede recuperarlo simplemente poniendo `DeletedAt = NULL`.
2. **Eficiencia en Red:** El `204 No Content` asegura que la red no envíe payloads de respuesta innecesarios.
