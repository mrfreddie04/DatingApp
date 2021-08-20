import { PresenceService } from './presence.service';
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
    private http: HttpClient,
    private presenceService: PresenceService
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
    this.setCurrentUser(null);
    // localStorage.removeItem("user"); 
    // this.currentUserSource.next(null);
  }

  public setCurrentUser(user: User): void {

    //extract roles from the token
    if(user) {
      user.roles = [];
      const roles = this.getDecodedTokenPayload(user.token).role;
      if(roles) {
        if(typeof roles === "string")
          user.roles.push(roles);
        else if(Array.isArray(roles))  
          user.roles = roles;
      }
    }

    //push new user to the observable pipe
    this.currentUserSource.next(user);

    //save in the local storage & open/close hub connection
    if(user) {
      localStorage.setItem("user", JSON.stringify(user));   
      this.presenceService.createHubConnection(user);
    } else {
      localStorage.removeItem("user");   
      this.presenceService.stopHubConnection();
    }
  }

  public getDecodedTokenPayload(token: string) {
    const payload_encoded = token.split(".")[1]; //second section in the token
    const payload = atob(payload_encoded); //decodes a string of data which has been encoded using Base64 encoding
    return JSON.parse(payload); //parse payload into an object
  }
}
