import { HttpClient } from '@angular/common/http';
import { environment } from './../../environments/environment';
import { Injectable } from '@angular/core';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  public getUsersWithRoles(){
    return this.http.get<Partial<User>[]>(`${this.baseUrl}admin/users-with-roles`);
  }

  public updateUserRoles(username: string, roles: string) {
    return this.http.post<string[]>(`${this.baseUrl}admin/edit-roles/${username}?roles=${roles}`,{});
  }
}
