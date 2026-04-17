import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ClarityModule } from '@clr/angular';
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
import { LoginComponent } from './auth/login/login.component';
import { RegistrarComponent } from './auth/registrar/registrar.component';
import { JwtInterceptor } from './services/jwt.interceptor';


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
    PedidosComponent,
    LoginComponent,
    RegistrarComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    AppRoutingModule,
    ClarityModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true }
  ],
  bootstrap: [AppComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class AppModule { }