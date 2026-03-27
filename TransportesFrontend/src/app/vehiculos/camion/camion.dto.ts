export interface CamionDTO {
  id?: string;
  matricula: string;
  capacidadPeso: number;
  capacidadVolumen: number;
  activo: boolean;
}

export interface CreateCamionDTO {
  matricula: string;
  capacidadPeso: number;
  capacidadVolumen: number;
  activo: boolean;
}

export interface UpdateCamionDTO {
  matricula: string;
  capacidadPeso: number;
  capacidadVolumen: number;
  activo: boolean;
}
