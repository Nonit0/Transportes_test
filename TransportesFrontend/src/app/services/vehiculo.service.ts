import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Camion, CreateCamion, UpdateCamion } from '../vehiculos/camion/camion.model';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class VehiculoService {

  private apiUrl = `${environment.apiUrl}/Camiones`;

  constructor(private http: HttpClient) { }

  getCamiones(): Observable<Camion[]> {
    return this.http.get<any>(this.apiUrl)
      .pipe(map(res => res.$values ? res.$values : res));
  }

  createCamion(camion: CreateCamion): Observable<Camion> {
    return this.http.post<Camion>(this.apiUrl, camion);
  }

  updateCamion(id: string, camion: UpdateCamion): Observable<Camion> {
    return this.http.put<Camion>(`${this.apiUrl}/${id}`, camion);
  }

  deleteCamion(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
