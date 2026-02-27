import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Task, TaskCreateRequest, TaskUpdateRequest, TaskSearchResult, TaskSearchApiResult } from '../models/task.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private baseUrl = `${environment.apiBaseUrl}/tasks`;

  constructor(private http: HttpClient) { }

  getAll(): Observable<Task[]> {
    return this.http.get<Task[]>(this.baseUrl);
  }

  getById(id: string): Observable<Task> {
    return this.http.get<Task>(`${this.baseUrl}/${id}`);
  }

  create(task: TaskCreateRequest): Observable<Task> {
    return this.http.post<Task>(this.baseUrl, task);
  }

  update(id: string, task: TaskUpdateRequest): Observable<Task> {
    return this.http.put<Task>(`${this.baseUrl}/${id}`, task);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  search(query: string, status?: string): Observable<TaskSearchResult> {
    const params = new HttpParams().set('q', query || '');
    return this.http.get<TaskSearchApiResult>(`${this.baseUrl}/search`, { params }).pipe(
      map((res) => ({ tasks: res.items, totalCount: res.totalCount }))
    );
  }
}
