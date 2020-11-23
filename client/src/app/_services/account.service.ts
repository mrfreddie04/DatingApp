import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';

import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl; //base address of api server

  private currentUserSource = new ReplaySubject<User>(1); //how many previous values do wa want to store
  currentUser$ = this.currentUserSource.asObservable();
  
  constructor(private http: HttpClient) { }

  register(model:any){
    return this.http.post(this.baseUrl+"account/register",model)
      .pipe(map((user:User)=>{
          if(user){
            this.setCurrentUser(user);
          }     
          //return user;               
      }));
  }

  login(model:any){
    return this.http.post(this.baseUrl+"account/login",model)
      .pipe(map((response:User)=>{
          const user = response;
          if(user){
            this.setCurrentUser(user);
          }                    
      }));
  }

  logout(){
    localStorage.removeItem("user");  
    this.currentUserSource.next(null);
  }

  setCurrentUser(user: User){
      localStorage.setItem("user", JSON.stringify(user)); 
      this.currentUserSource.next(user); //store the next value to the ReplayObject
  }
}
