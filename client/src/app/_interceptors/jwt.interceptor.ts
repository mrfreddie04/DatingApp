import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';

import { AccountService } from '../_services/account.service';
import { User } from '../_models/user';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {

  constructor(private accountService: AccountService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    let currentUser: User;
    
    //take will automatically unsubscribe - will complete right away? after taking 1 
    this.accountService.currentUser$.pipe(take(1)).subscribe(user=>{
      currentUser=user;
    });
    
    if(currentUser){
      request = request.clone({
        setHeaders: {
        Authorization: `Bearer ${JSON.parse(localStorage.getItem("user"))?.token}`
        }
      });
    }

    return next.handle(request);
  }
}
