export interface Camion {
  id?: string;
  matricula: string;
  capacidadPeso: number;
  capacidadVolumen: number;
  activo: boolean;
}

export interface CreateCamion {
  matricula: string;
  capacidadPeso: number;
  capacidadVolumen: number;
  activo: boolean;
}

export interface UpdateCamion {
  matricula: string;
  capacidadPeso: number;
  capacidadVolumen: number;
  activo: boolean;
}
