import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { InicioComponent } from '../inicio/inicio.component';
import { AlmacenesComponent } from '../almacenes/almacenes.component';
import { VehiculosComponent } from '../vehiculos/vehiculos.component';
import { ProductosComponent } from '../productos/productos.component';
import { ConductoresComponent } from '../conductores/conductores.component';
import { ClientesComponent } from '../clientes/clientes.component';
import { FabricasComponent } from '../fabricas/fabricas.component';
import { PedidosComponent } from '../pedidos/pedidos.component';
import { LoginComponent } from '../auth/login/login.component';
import { RegistrarComponent } from '../auth/registrar/registrar.component';
import { AuthGuard } from '../guards/auth.guard';

const routes: Routes = [
  { path: '', component: InicioComponent }, // La ruta vacía es el Inicio
  { path: 'almacenes', component: AlmacenesComponent, canActivate: [AuthGuard] },
  { path: 'vehiculos', component: VehiculosComponent, canActivate: [AuthGuard] },
  { path: 'productos', component: ProductosComponent, canActivate: [AuthGuard] },
  { path: 'conductores', component: ConductoresComponent, canActivate: [AuthGuard] },
  { path: 'clientes', component: ClientesComponent, canActivate: [AuthGuard] },
  { path: 'fabricas', component: FabricasComponent, canActivate: [AuthGuard] },
  { path: 'pedidos', component: PedidosComponent, canActivate: [AuthGuard] },
  { path: 'login', component: LoginComponent },
  { path: 'registrar', component: RegistrarComponent },
  { path: '**', redirectTo: '', pathMatch: 'full' } // Si el usuario escribe una ruta que no existe, lo mandamos al inicio
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
