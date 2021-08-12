import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from "rxjs/operators";
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private baseUrl: string = "https://localhost:5001/api/";
  private currentUserSource = new ReplaySubject<User>(1);
  public currentUser$  = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) { }

  public login(model: any) {
    return this.http.post( `${this.baseUrl}account/login`, model).pipe(
      map((response: User) => {
        const user = response;
        if(user) {
          this.currentUserSource.next(user);
          localStorage.setItem("user", JSON.stringify(user)); 
        }
      })
    )
  }

  public register(model: any) {
    return this.http.post( `${this.baseUrl}account/register`, model).pipe(
      map((response: User) => {
        const user = response;
        if(user) {
          this.currentUserSource.next(user);
          localStorage.setItem("user", JSON.stringify(user)); 
        }
      })
    )
  }  

  public logout() {
    localStorage.removeItem("user"); 
    this.currentUserSource.next(null);
  }

  public setCurrentUser(user: User): void {
    this.currentUserSource.next(user);
  }
}
