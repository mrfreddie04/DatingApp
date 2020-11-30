import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { of } from 'rxjs/internal/observable/of';
import { map, take } from 'rxjs/operators';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  
  constructor(private accountService: AccountService, private toastr: ToastrService){}
  
  canActivate( route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> 
  {
    return this.accountService.currentUser$
      .pipe(
        //take(1),
        map((user:User)=>{
          if(user && (user.roles.includes("Admin") || user.roles.includes("Moderator")))
            return true;
          this.toastr.error("You cannot enter this area!");
          return false;
      }));
  }  
}
