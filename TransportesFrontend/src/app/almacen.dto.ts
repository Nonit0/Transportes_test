// Fíjate: No hay 'DeletedAt'. Es el contrato limpio y seguro.
export interface AlmacenDTO {
    id: string;
    nombre: string;
    direccionId: string;
    direccionCompleta: string;
    /* Ya no hacen falta pues estos pertenecen a direccion no a almacen
    ciudad: string;
    cp: string;
    provincia: string;
    pais: string;
    */
}

// El DTO de Escritura (Reflejo exacto de tu C#)
export interface CreateAlmacenDTO {
  nombre: string;
  direccionId: string;
  /* Ya no hacen falta pues estos pertenecen a direccion no a almacen
  ciudad: string;
  cp: string;
  provincia: string;
  pais: string;
  */
}

export interface DireccionDTO {
  id: string;
  textoMostrar: string; // lo que se muestra en el select
  calle: string;
  ciudad: string;
  provincia: string;
  cp: string;
  pais: string;
}

export interface UpdateAlmacenDTO {
  nombre: string;
  direccionId: string;
  /* Ya no hacen falta pues estos pertenecen a direccion no a almacen
  ciudad: string;
  cp: string;
  provincia: string;
  pais: string;
  */
}