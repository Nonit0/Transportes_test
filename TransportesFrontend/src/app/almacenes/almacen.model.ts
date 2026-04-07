// Interfaz que refleja la entidad Almacen del backend
export interface Almacen {
    id: string;
    nombre: string;
    direccionId: string;
    deletedAt: string | null;
    // Viene gracias al .Include(a => a.Direccion) del backend
    direccion: Direccion | null;
}

// Interfaz que refleja la entidad Direccion del backend
export interface Direccion {
    id: string;
    calle: string;
    ciudad: string;
    provincia: string;
    cp: string;
    pais: string;
    deletedAt: string | null;
}

// Para el combo del select de direcciones (lo que devuelve DireccionesController GET)
export interface DireccionCombo {
  id: string;
  textoMostrar: string;
}