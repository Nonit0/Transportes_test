import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppRoutingModule } from './routing/app-routing.module';

import { AppComponent } from './app.component';
import { AlmacenesComponent } from './almacenes/almacenes.component';
import { VehiculosComponent } from './vehiculos/vehiculos.component';
import { InicioComponent } from './inicio/inicio.component';
import { ProductosComponent } from './productos/productos.component';
import { ConductoresComponent } from './conductores/conductores.component';
import { ClientesComponent } from './clientes/clientes.component';
import { FabricasComponent } from './fabricas/fabricas.component';
import { PedidosComponent } from './pedidos/pedidos.component';

@NgModule({
  declarations: [
    AppComponent,
    AlmacenesComponent,
    VehiculosComponent,
    InicioComponent,
    ProductosComponent,
    ConductoresComponent,
    ClientesComponent,
    FabricasComponent,
    PedidosComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }