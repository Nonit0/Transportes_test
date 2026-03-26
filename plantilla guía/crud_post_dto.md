# Plantilla: CRUD - Método POST (Crear) Avanzado con DTOs

**Concepto:** Para insertar datos, usamos un **Input DTO** (evita *Mass Assignment*). Al finalizar, el estándar REST dicta que debemos devolver un código **201 Created**, la URL donde consultar el recurso, y el objeto recién creado formateado como un **Output DTO**.

---

## 🛡️ Paso 1: Crear el Input DTO (`CreateDTO`)
En la carpeta `DTOs/`, crea `Create<NombreEntidad>DTO.cs`.
**Regla:** NUNCA lleva `Id` ni campos de auditoría (`DeletedAt`). Solo lo que el usuario envía en el formulario.

> **⚠️ NOTA DE ARQUITECTURA (Validaciones de BBDD):** Si tu base de datos tiene columnas marcadas como `NOT NULL` (ej. Código Postal, Provincia), **debes** incluirlas en tu DTO con la etiqueta `[Required]`. Si no lo haces, Entity Framework intentará guardar un `null` y la base de datos lanzará un error 500.

```csharp
using System.ComponentModel.DataAnnotations;

namespace <TuNamespace>.DTOs
{
    public class Create<NombreEntidad>DTO
    {
        [Required(ErrorMessage = "El campo es obligatorio")]
        public string <Propiedad1> { get; set; } // Ej: Nombre
        
        // Si hay relación con otra tabla, pedimos los datos necesarios para enlazarla
        [Required]
        public string <DatoRelacionado> { get; set; } // Ej: Ciudad o DireccionId
    }
}
```

---

## 📥 Paso 2: El Método POST en el Controlador (Nivel Senior)
Abre `Controllers/<NombreEntidad>Controller.cs`. 

```csharp
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)] // Para que Swagger lo documente en verde
        public async Task<ActionResult<<NombreEntidad>DTO>> Post<NombreEntidad>([FromBody] Create<NombreEntidad>DTO dto)
        {
            // 1. Validar el DTO de entrada
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 2. [OPCIONAL] Inteligencia Relacional (Claves Foráneas)
            /*
            var entidadRelacionada = await _context.<TablaRelacionada>
                .FirstOrDefaultAsync(r => r.Campo == dto.<DatoRelacionado>);

            if (entidadRelacionada == null)
            {
                entidadRelacionada = new <ClaseRelacionada> { Campo = dto.<DatoRelacionado> };
                _context.<TablaRelacionada>.Add(entidadRelacionada);
                await _context.SaveChangesAsync();
            }
            */

            // 3. Mapear el DTO a la Entidad de Base de Datos
            var nuevaEntidad = new <NombreEntidad>
            {
                <Columna1> = dto.<Propiedad1>,
                // <ColumnaFK_Id> = entidadRelacionada.Id
            };

            // 4. Guardar en Base de Datos (MySQL genera el UUID automáticamente si está configurado)
            _context.<NombreDbSet>.Add(nuevaEntidad);
            await _context.SaveChangesAsync();

            // 5. Re-consultar y Mapear al Output DTO
            var entidadCreadaDTO = await _context.<NombreDbSet>
                .Where(e => e.Id == nuevaEntidad.Id)
                .Select(e => new <NombreEntidad>DTO
                {
                    Id = e.Id,
                    <Propiedad1> = e.<Columna1>,
                })
                .FirstAsync();

            // 6. Devolver 201 Created (Estándar REST)
            return CreatedAtAction(nameof(Get<NombreEntidad>s), new { id = entidadCreadaDTO.Id }, entidadCreadaDTO);
        }
```

---

## 🚀 ¿Por qué este enfoque es Superior?
1. **Cumple el protocolo REST:** `CreatedAtAction` devuelve un código **201** y añade la cabecera `Location`.
2. **Protege la integridad de la BD:** Evita errores de Claves Foráneas comprobando o creando las dependencias antes del insert principal.
3. **Prepara el terreno para el Frontend:** Devuelve exactamente el objeto DTO que el cliente necesita para repintar la interfaz sin hacer peticiones extra.
