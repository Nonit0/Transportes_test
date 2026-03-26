import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router'; // 👈 Importamos Rutas

import { AppComponent } from './app.component';
import { AlmacenesComponent } from './almacenes/almacenes.component';

// 👈 Definimos el diccionario de Rutas
const misRutas: Routes = [
  { path: 'almacenes', component: AlmacenesComponent },
  { path: '', redirectTo: '/almacenes', pathMatch: 'full' }, // Redirección inicial
  { path: '**', redirectTo: '/almacenes' } // Control de errores en la URL
];

@NgModule({
  declarations: [
    AppComponent,
    AlmacenesComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot(misRutas) // 👈 Activamos las Rutas
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }