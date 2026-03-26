# Plantilla: CRUD - Método PUT (Actualizar) Avanzado con DTOs

**Concepto:** El método `PUT` se usa para actualizar un registro existente. El estándar REST dicta que el ID viaja en la URL (`/api/entidad/{id}`) y los datos nuevos viajan en el cuerpo (Body) usando un **UpdateDTO**. Al finalizar, devolvemos un **200 OK** con el objeto actualizado.

---

## 🛡️ Paso 1: Crear el Input DTO (`UpdateDTO`)
En la carpeta `DTOs/`, crea `Update<NombreEntidad>DTO.cs`.
**Nota:** A menudo es idéntico al `CreateDTO`. Sin embargo, a nivel de arquitectura, se separan en dos clases distintas porque en el futuro los requisitos cambian.

```csharp
using System.ComponentModel.DataAnnotations;

namespace <TuNamespace>.DTOs
{
    public class Update<NombreEntidad>DTO
    {
        [Required(ErrorMessage = "El campo es obligatorio")]
        public string <Propiedad1> { get; set; } 
        
        [Required]
        public string <DatoRelacionado> { get; set; } 
    }
}
```

---

## 📥 Paso 2: El Método PUT en el Controlador (Nivel Senior)
Abre `Controllers/<NombreEntidad>Controller.cs`. 

```csharp
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<<NombreEntidad>DTO>> Put<NombreEntidad>(string id, [FromBody] Update<NombreEntidad>DTO dto)
        {
            // 1. Validar DTO
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 2. Buscar la entidad existente (filtrando borrados lógicos)
            var entidad = await _context.<NombreDbSet>
                // .Include(e => e.<TablaRelacionada>) // Descomentar si vas a actualizar relaciones
                .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);

            if (entidad == null)
            {
                return NotFound(new { mensaje = "El registro no existe o ha sido eliminado." });
            }

            // 3. Mapear los nuevos valores del DTO a la Entidad
            entidad.<Columna1> = dto.<Propiedad1>;
            
            // Si hay entidades relacionadas (Ej: Dirección), actualízalas aquí.
            // entidad.Relacion.<Campo> = dto.<DatoRelacionado>;

            // 4. Guardar cambios (Entity Framework detecta automáticamente qué columnas cambiaron)
            await _context.SaveChangesAsync();

            // 5. Mapear al DTO de salida
            var entidadActualizadaDTO = new <NombreEntidad>DTO
            {
                Id = entidad.Id,
                <Propiedad1> = entidad.<Columna1>,
                // <PropiedadExtra> = entidad.<TablaRelacionada>.<Campo>
            };

            // 6. Devolver 200 OK con el objeto fresco
            return Ok(entidadActualizadaDTO);
        }
```
