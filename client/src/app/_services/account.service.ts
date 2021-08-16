import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from "rxjs/operators";
import { User } from '../_models/user';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  private currentUserSource = new ReplaySubject<User>(1);
  public currentUser$  = this.currentUserSource.asObservable();

  constructor(
    private http: HttpClient
  ) { }

  public login(model: any) {
    return this.http.post( `${environment.apiUrl}account/login`, model).pipe(
      map((response: User) => {
        const user = response;
        console.log("Login", user);
        if(user) {
          this.setCurrentUser(user);
        }
        return response;
      })
    )
  }

  public register(model: any) {
    return this.http.post( `${environment .apiUrl}account/register`, model).pipe(
      map((response: User) => {
        const user = response;
        if(user) {
          this.setCurrentUser(user);
        }
        return response;
      })
    )
  }  

  public logout() {
    localStorage.removeItem("user"); 
    this.currentUserSource.next(null);
  }

  public setCurrentUser(user: User): void {
    localStorage.setItem("user", JSON.stringify(user));    
    this.currentUserSource.next(user);
  }
}
