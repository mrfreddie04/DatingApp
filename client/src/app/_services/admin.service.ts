import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Photo } from '../_models/photo';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl: string = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getUsersWithRoles()
  {
    return this.http.get<Partial<User[]>>(this.baseUrl+"admin/users-with-roles");
  }

  updateUserRoles(username: string, roles: string[])
  {
    //const url = this.baseUrl+"admin/edit-roles/"+username+"?roles="+roles.join(",");
    //console.log("URL",url);
    return this.http.post<string[]>(this.baseUrl+"admin/edit-roles/"+username+"?roles="+roles.join(","),{});
  }

  getPhotosForApproval()
  {
    return this.http.get<Partial<Photo[]>>(this.baseUrl+"admin/photos-to-moderate");
  }

  approvePhoto(id:number)
  {  
    return this.http.post(this.baseUrl+"admin/approve-photo/"+id,{});
  }

  rejectPhoto(id:number)
  {  
    return this.http.post(this.baseUrl+"admin/reject-photo/"+id,{});
  }  
}
