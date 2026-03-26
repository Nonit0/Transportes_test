// Fíjate: No hay 'DeletedAt'. Es el contrato limpio y seguro.
export interface AlmacenDTO {
    id: string;
    nombre: string;
    direccionCompleta: string;
    ciudad: string;
    cp: string;
    provincia: string;
    pais: string;
}

// El DTO de Escritura (Reflejo exacto de tu C#)
export interface CreateAlmacenDTO {
  nombre: string;
  direccionCompleta: string;
  ciudad: string;
  cp: string;
  provincia: string;
  pais: string;
}

export interface UpdateAlmacenDTO {
  nombre: string;
  direccionCompleta: string;
  ciudad: string;
  cp: string;
  provincia: string;
  pais: string;
}