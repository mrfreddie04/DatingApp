import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpHeaders
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { switchMap, take } from 'rxjs/operators';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {

  constructor(private accountService: AccountService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    
    // const user: User = JSON.parse(localStorage.getItem("user"));
    // const token = user?.token;
    let currentUser: User;

    // this.accountService.currentUser$.pipe(take(1)).subscribe( user => currentUser = user);  

    // if(currentUser) {
    //   request = request.clone({
    //     setHeaders: {
    //       Authorization: `Bearer ${currentUser.token}`
    //     }
    //   });
    //   console.log("User token", currentUser.token);
    //   return next.handle(request);
    // }
    // console.log("No token");
    // return next.handle(request);    

    return this.accountService.currentUser$.pipe(
      take(1),
      switchMap( (user)=>{
        if(user) {
          request = request.clone({
            setHeaders: {
              Authorization: `Bearer ${user.token}`
            }
          });
          console.log("User token", user.token);
          return next.handle(request);
        }
        console.log("No token");
        return next.handle(request);
      })
    );
  }
}
