import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { AppRoutingModule } from './routing/app-routing.module';

import { AppComponent } from './app.component';
import { AlmacenesComponent } from './almacenes/almacenes.component';
import { VehiculosComponent } from './vehiculos/vehiculos.component';
import { InicioComponent } from './inicio/inicio.component';
import { ProductosComponent } from './productos/productos.component';
import { ConductoresComponent } from './conductores/conductores.component';
import { ClientesComponent } from './clientes/clientes.component';

@NgModule({
  declarations: [
    AppComponent,
    AlmacenesComponent,
    VehiculosComponent,
    InicioComponent,
    ProductosComponent,
    ConductoresComponent,
    ClientesComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }