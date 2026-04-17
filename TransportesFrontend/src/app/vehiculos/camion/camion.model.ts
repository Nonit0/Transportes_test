export interface Camion {
  id?: string;
  matricula: string;
  capacidadPeso: number;
  capacidadVolumen: number;
  activo: boolean;
  clienteId?: string;
}

export interface CreateCamion {
  matricula: string;
  capacidadPeso: number;
  capacidadVolumen: number;
  activo: boolean;
  clienteId?: string;
}

export interface UpdateCamion {
  matricula: string;
  capacidadPeso: number;
  capacidadVolumen: number;
  activo: boolean;
  clienteId?: string;
}
