import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CamionDTO, CreateCamionDTO, UpdateCamionDTO } from '../vehiculos/camion/camion.dto';

@Injectable({
  providedIn: 'root'
})
export class VehiculoService {

  private apiUrl = 'https://localhost:5011/api/Camiones';

  constructor(private http: HttpClient) { }

  getCamiones(): Observable<CamionDTO[]> {
    return this.http.get<CamionDTO[]>(this.apiUrl);
  }

  createCamion(camion: CreateCamionDTO): Observable<CamionDTO> {
    return this.http.post<CamionDTO>(this.apiUrl, camion);
  }

  updateCamion(id: string, camion: UpdateCamionDTO): Observable<CamionDTO> {
    return this.http.put<CamionDTO>(`${this.apiUrl}/${id}`, camion);
  }

  deleteCamion(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
