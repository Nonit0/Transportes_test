# Plantilla: Angular - CRUD Unificado Full-Stack (Template-Driven Forms)

**Concepto:** En lugar de tener un HTML y un método TypeScript para Crear, y otro distinto para Editar, aplicamos el principio **DRY (Don't Repeat Yourself)**. Usamos un único formulario reactivo. Si la variable `idEdicion` es nula, el formulario hace un POST. Si tiene un ID, hace un PUT. 

---

## 🧠 1. El TypeScript (`app.component.ts`)

```typescript
import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { <NombreEntidad>DTO, Create<NombreEntidad>DTO } from './<entidad>.dto'; 

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  listaElementos: <NombreEntidad>DTO[] = [];
  idEdicion: string | null = null; // Controla el Modo Edición vs Creación
  
  // Objeto unificado conectado al HTML con ngModel
  formulario: Create<NombreEntidad>DTO = { propiedad1: '', propiedad2: '' };

  constructor(private http: HttpClient) {}

  ngOnInit(): void { this.cargarDatos(); }

  // 🔵 GET
  cargarDatos() {
    this.http.get<<NombreEntidad>DTO[]>('https://localhost:5001/api/<NombreEntidad>s')
      .subscribe({ next: (data) => this.listaElementos = data });
  }

  // 🟢 POST / PUT Unificado
  guardarElemento() {
    if (this.idEdicion) {
      // MODO EDICIÓN (PUT)
      this.http.put<<NombreEntidad>DTO>(`https://localhost:5001/api/<NombreEntidad>s/${this.idEdicion}`, this.formulario)
        .subscribe({
          next: (actualizado) => {
            // Reemplazamos visualmente el viejo por el nuevo usando .map()
            this.listaElementos = this.listaElementos.map(e => e.id === actualizado.id ? actualizado : e);
            this.resetearFormulario();
          }
        });
    } else {
      // MODO CREACIÓN (POST)
      this.http.post<<NombreEntidad>DTO>('https://localhost:5001/api/<NombreEntidad>s', this.formulario)
        .subscribe({
          next: (creado) => {
            // Añadimos visualmente el nuevo registro arriba del todo
            this.listaElementos.unshift(creado); 
            this.resetearFormulario();
          }
        });
    }
  }

  // 🔴 DELETE (Soft Delete)
  eliminarElemento(id: string, nombre: string) {
    if (confirm(`¿Eliminar "${nombre}"?`)) {
      this.http.delete(`https://localhost:5001/api/<NombreEntidad>s/${id}`)
        .subscribe({
          next: () => this.listaElementos = this.listaElementos.filter(e => e.id !== id)
        });
    }
  }

  // 🟡 Utilidades UX
  cargarParaEdicion(elemento: <NombreEntidad>DTO) {
    this.idEdicion = elemento.id;
    // Llenamos el formulario con los datos de la tabla
    this.formulario = { propiedad1: elemento.prop1, propiedad2: elemento.prop2 };
    window.scrollTo({ top: 0, behavior: 'smooth' }); // Sube el scroll
  }

  resetearFormulario() {
    this.idEdicion = null;
    this.formulario = { propiedad1: '', propiedad2: '' };
  }
}
```

---

## 🎨 2. El HTML Unificado (`app.component.html`)

```html
<div class="contenedor">
  <h2>{{ idEdicion ? '✏️ Editar Registro' : '➕ Nuevo Registro' }}</h2>
  
  <form #miForm="ngForm" (ngSubmit)="guardarElemento()">
    
    <input type="text" name="prop1" [(ngModel)]="formulario.propiedad1" required>

    <div class="botones-form">
      <button type="submit" [disabled]="miForm.invalid">
        {{ idEdicion ? 'Actualizar' : 'Guardar' }}
      </button>
      
      <button type="button" class="btn-cancelar" *ngIf="idEdicion" (click)="resetearFormulario()">
        Cancelar
      </button>
    </div>
  </form>

  <hr>

  <div class="grid-elementos">
    <div *ngFor="let elemento of listaElementos" class="tarjeta">
      <div class="tarjeta-header">
        <h2>{{ elemento.propiedad1 }}</h2>
        <div>
          <button class="btn-editar" (click)="cargarParaEdicion(elemento)">✏️</button>
          <button class="btn-eliminar" (click)="eliminarElemento(elemento.id, elemento.propiedad1)">🗑️</button>
        </div>
      </div>
    </div>
  </div>
</div>
```
