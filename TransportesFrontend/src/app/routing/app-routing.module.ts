import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { InicioComponent } from '../inicio/inicio.component';
import { AlmacenesComponent } from '../almacenes/almacenes.component';
import { VehiculosComponent } from '../vehiculos/vehiculos.component';

const routes: Routes = [
  { path: '', component: InicioComponent }, // La ruta vacía es el Inicio
  { path: 'almacenes', component: AlmacenesComponent },
  { path: 'vehiculos', component: VehiculosComponent },
  { path: '**', redirectTo: '', pathMatch: 'full' } // Si el usuario escribe una ruta que no existe, lo mandamos al inicio
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
